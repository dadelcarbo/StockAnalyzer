using Newtonsoft.Json;
using Saxo.OpenAPI.AuthenticationServices;
using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockAgent;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StockAnalyzer.StockPortfolio
{
    public enum OrderType
    {
        Market,
        Limit,
        Threshold
    }
    public class StockPortfolio
    {
        public const string SIMU_P = "Simu_P";
        public const string REPLAY_P = "Replay_P";

        const string PORTFOLIO_FILE_EXT = ".ucptf";

        #region EVENTS
        public delegate void RefreshedHandler(StockPortfolio sender);
        public event RefreshedHandler Refreshed;
        #endregion

        #region STATIC METHODS/PROPERTIES
        static int instanceCount = 0;
        public static StockPortfolio SimulationPortfolio { get; private set; }
        public static StockPortfolio ReplayPortfolio { get; private set; }
        public static IStockPriceProvider PriceProvider { get; set; }
        public static List<StockPortfolio> Portfolios { get; private set; }
        static public StockPortfolio CreateSimulationPortfolio()
        {
            return new StockPortfolio() { Name = SIMU_P + "_" + instanceCount++, InitialBalance = 10000, IsSimu = true };
        }
        #endregion

        public StockPortfolio()
        {
            this.TradeOperations = new List<StockTradeOperation>();
            this.MaxRisk = 0.02f;
            this.MinRisk = 0.0025f;
            this.MaxDrawDown = 0.2f;
            this.MaxPositionSize = 0.2f;
        }

        public string Name
        {
            get => name;
            set
            {
                if (name == null)
                {
                    name = value;
                }
                else
                {
                    if (name != value)
                    {
                        RenameFile(value);
                        name = value;
                        this.Serialize();
                    }
                }
            }
        }
        public string SaxoAccountId { get; set; }
        public string SaxoClientId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastSyncDate { get; set; }
        public float InitialBalance { get; set; }
        public float Balance { get; set; }
        public float PositionValue { get; set; }
        [JsonIgnore]
        public float TotalValue => this.Balance + this.PositionValue;
        public float RiskFreeValue => this.Balance + this.Positions.Select(p => p.EntryQty * p.TrailStop).Sum();

        public float DrawDown => (MaxValue - TotalValue) / MaxValue;
        public float MaxValue { get; set; }

        public float MaxRisk { get; set; }
        public float MinRisk { get; set; }
        public float MaxDrawDown { get; set; }


        /// <summary>
        /// Calculate risk based on the current drawdown using linear function.
        /// </summary>
        [JsonIgnore]
        public float DynamicRisk => this.DrawDown > this.MaxDrawDown ? this.MinRisk : (MinRisk - MaxRisk) / MaxDrawDown * this.DrawDown + MaxRisk;

        public float AutoTradeRisk { get; set; } = 0.001f;
        public float MaxPositionSize { get; set; }
        [JsonIgnore]
        public float Return => (TotalValue - InitialBalance) / InitialBalance;
        public bool IsSimu { get; set; }
        public bool IsSaxoSimu { get; set; }

        public long LastLogId { get; set; }

        [JsonIgnore]
        public List<SaxoOrder> SaxoOpenOrders { get; } = new List<SaxoOrder>();

        public List<SaxoOrder> SaxoOrders { get; private set; } = new List<SaxoOrder>();

        public IEnumerable<SaxoOrder> GetExecutedOrders(string stockName)
        {
            return this.SaxoOrders.Where(o => o.StockName == stockName && o.IsExecuted);
        }
        public IEnumerable<StockOpenedOrder> GetActiveOrders(string stockName)
        {
            return this.SaxoOpenOrders.Where(o => o.StockName == stockName).Select(o => new StockOpenedOrder(o));
        }
        public IEnumerable<StockOpenedOrder> GetActiveOrders()
        {
            return this.SaxoOpenOrders.Select(o => new StockOpenedOrder(o));
        }

        public List<OrderActivity> SaxoOrderActivity { get; private set; } = new List<OrderActivity>();


        public List<StockPosition> Positions { get; } = new List<StockPosition>();
        public List<StockPosition> ClosedPositions { get; } = new List<StockPosition>();

        [JsonIgnore]
        public List<StockTradeOperation> TradeOperations { get; set; }

        #region PERSISTENCY
        public void RenameFile(string newName)
        {
            using var ml = new MethodLogger(this);
            string filepath = Path.Combine(Folders.Portfolio, this.Name + PORTFOLIO_FILE_EXT);
            string newFilepath = Path.Combine(Folders.Portfolio, newName + PORTFOLIO_FILE_EXT);
            if (File.Exists(filepath))
            {
                File.Move(filepath, newFilepath);
            }
        }
        private static bool archivePortfolioFile = true;
        public void Serialize()
        {
            using var ml = new MethodLogger(this, true, this.Name);
            lock (this)
            {
                string filepath = Path.Combine(Folders.Portfolio, this.Name + PORTFOLIO_FILE_EXT);
                if (archivePortfolioFile)
                {
                    // Archive PTF files
                    string archiveDirectory = Path.Combine(Folders.Portfolio, "Archive");
                    if (!Directory.Exists(archiveDirectory))
                        Directory.CreateDirectory(archiveDirectory);
                    else
                    {
                        var dateLimit = DateTime.Today.AddDays(-2);
                        foreach (var file in Directory.EnumerateFiles(archiveDirectory).Where(f => File.GetLastWriteTime(f) < dateLimit))
                        {
                            File.Delete(file);
                        }
                    }
                    var fileDate = File.GetLastWriteTime(filepath);
                    var archiveFilePath = Path.Combine(archiveDirectory, this.Name + "_" + fileDate.ToString("yyyy_MM_dd HH_mm_ss.ff") + PORTFOLIO_FILE_EXT);
                    File.Copy(filepath, archiveFilePath);
                }

                var activityDateLimit = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
                this.SaxoOrderActivity = this.SaxoOrderActivity.OrderByDescending(a => a.ActivityTime).Where(a => a.ActivityTime > activityDateLimit).ToList();

                if (this.SaxoAccountId == "78800/719479EUR")
                {
                    this.SaxoOrders = this.SaxoOrders.OrderByDescending(a => a.ActivityTime).Where(a => a.ActivityTime > activityDateLimit).ToList();
                }
                File.WriteAllText(filepath, JsonConvert.SerializeObject(this, Formatting.Indented, jsonSerializerSettings));

            }
        }
        static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings { DateFormatString = @"yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffZ" };
        public static StockPortfolio Deserialize(string filepath)
        {
            using var ml = new MethodLogger(typeof(StockPortfolio));
            return JsonConvert.DeserializeObject<StockPortfolio>(File.ReadAllText(filepath), jsonSerializerSettings);
        }
        public static List<StockPortfolio> LoadPortfolios(string folder)
        {
            using var ml = new MethodLogger(typeof(StockPortfolio));
            try
            {
                // Load saved portfolio
                StockPortfolio.Portfolios = new List<StockPortfolio>();
                foreach (var portfolio in Directory.EnumerateFiles(folder, "*" + PORTFOLIO_FILE_EXT).OrderBy(s => s).Select(s => StockPortfolio.Deserialize(s)))
                {
                    if (portfolio.LastSyncDate < portfolio.CreationDate)
                        portfolio.LastSyncDate = portfolio.CreationDate;

                    StockPortfolio.Portfolios.Add(portfolio);
                }
                foreach (var position in StockPortfolio.Portfolios.SelectMany(p => p.Positions))
                {
                    if (position.TrailStop == 0f && position.Stop != 0f)
                        position.TrailStop = position.Stop;
                }

                // Add simulation portfolio
                SimulationPortfolio = new StockPortfolio() { Name = SIMU_P, InitialBalance = 10000, IsSimu = true };
                StockPortfolio.Portfolios.Add(SimulationPortfolio);
                ReplayPortfolio = new StockPortfolio() { Name = REPLAY_P, InitialBalance = 10000, IsSimu = true };
                StockPortfolio.Portfolios.Add(ReplayPortfolio);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading portfolio file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return StockPortfolio.Portfolios.OrderBy(p => p.Name).ToList();
        }
        #endregion

        public int MaxPositions { get; set; } = 10;

        #region OPERATION MANAGEMENT
        public long GetNextOperationId()
        {
            return this.TradeOperations.Count == 0 ? 0 : this.TradeOperations.Max(o => o.Id) + 1;
        }
        public void AddOperation(StockOperation operation)
        {
            this.Balance = operation.Balance;
            if (operation.StockName.StartsWith("SRD "))
                return;
            switch (operation.OperationType.ToLower())
            {
                case StockOperation.BUY:
                    {
                        this.TradeOperations.Add(new StockTradeOperation
                        {
                            OperationType = TradeOperationType.Buy,
                            Date = operation.Date,
                            Id = operation.Id,
                            Qty = operation.Qty,
                            StockName = operation.StockName,
                            Value = operation.Amount / operation.Qty
                        });
                        var qty = operation.Qty;
                        var stockName = operation.StockName;
                        var position = this.Positions.FirstOrDefault(p => p.StockName == stockName);
                        if (position != null) // Position on this stock already exists, add new values
                        {

                            var openValue = (position.EntryValue * position.EntryQty - operation.Amount) / (position.EntryQty + qty);

                            position.ExitDate = operation.Date;
                            position.ExitValue = openValue;
                            this.Positions.Add(new StockPosition
                            {
                                EntryDate = operation.Date,
                                EntryQty = position.EntryQty + qty,
                                StockName = stockName,
                                EntryValue = openValue
                            });
                        }
                        else // Position on this stock doen't exists, create a new one
                        {
                            this.Positions.Add(new StockPosition
                            {
                                EntryDate = operation.Date,
                                EntryQty = qty,
                                StockName = stockName,
                                EntryValue = -operation.Amount / qty
                            });
                        }
                    }
                    break;
                case StockOperation.SELL:
                    {
                        this.TradeOperations.Add(new StockTradeOperation
                        {
                            OperationType = TradeOperationType.Sell,
                            Date = operation.Date,
                            Id = operation.Id,
                            Qty = operation.Qty,
                            StockName = operation.StockName,
                            Value = operation.Amount / operation.Qty
                        });
                        var qty = operation.Qty;
                        var stockName = operation.StockName;
                        var position = this.Positions.FirstOrDefault(p => p.StockName == stockName);
                        if (position != null)
                        {
                            position.ExitDate = operation.Date;
                            position.ExitValue = operation.Amount / operation.Qty;
                            if (position.EntryQty != qty)
                            {
                                this.Positions.Add(new StockPosition
                                {
                                    EntryDate = operation.Date,
                                    EntryQty = position.EntryQty - qty,
                                    StockName = operation.StockName,
                                    EntryValue = position.EntryValue
                                });
                            }
                        }
                        else
                        {
                            StockLog.Write($"Selling not opened position: {stockName} qty:{qty}");
                        }
                    }
                    break;
            }
        }
        #endregion
        public void InitPositionFromTradeSummary(IList<StockTrade> trades)
        {
            this.Clear();

            // Sell completed trades
            foreach (var trade in trades)
            {
                this.ClosedPositions.Add(new StockPosition
                {
                    EntryDate = trade.EntryDate,
                    EntryQty = trade.Qty,
                    EntryValue = trade.EntryValue,
                    ExitDate = trade.IsClosed ? (DateTime?)trade.ExitDate : null,
                    ExitValue = trade.ExitValue,
                    StockName = trade.Serie.StockName,

                    Stop = trade.EntryStop,
                    TrailStop = trade.EntryStop
                });
            }
        }

        /// <summary>
        /// Converts from Trade list to portfolio operations. This function is limited to the MaxPosition variable
        /// </summary>
        /// <param name="tradeSummary"></param>
        public void InitFromTradeSummary(IList<StockTrade> trades)
        {
            this.Clear();
            var dates = trades.Select(t => t.EntryDate).
                Union(trades.Where(t => t.ExitIndex >= 0).Select(t => t.ExitDate)).
                Distinct().OrderBy(d => d).ToList();

            int openedPosition = 0;
            foreach (var date in dates)
            {
                // Buy begining trades
                foreach (var trade in trades.Where(t => t.EntryDate == date))
                {
                    if (openedPosition >= MaxPositions)
                        break;

                    var qty = trade.Qty;
                    var amount = qty * trade.EntryValue;
                    this.Balance -= amount;

                    var id = this.GetNextOperationId();
                    var entry = StockOperation.FromSimu(id, trade.EntryDate, trade.Serie.StockName, StockOperation.BUY, qty, -amount, !trade.IsLong);
                    entry.Balance = this.Balance;
                    this.AddOperation(entry);
                    var lastPosition = this.Positions.Last();
                    lastPosition.Stop = trade.EntryStop;
                    lastPosition.TrailStop = trade.EntryStop;
                    openedPosition++;
                }
                // Sell completed trades
                foreach (var trade in trades.Where(t => t.ExitDate == date))
                {
                    var pos = this.Positions.FirstOrDefault(p => p.StockName == trade.Serie.StockName);
                    if (pos == null)
                        continue;
                    var amount = pos.EntryQty * trade.ExitValue;
                    this.Balance += amount;
                    var id = this.GetNextOperationId();
                    var exit = StockOperation.FromSimu(id, trade.ExitDate, trade.Serie.StockName, StockOperation.SELL, pos.EntryQty, amount, !trade.IsLong);
                    exit.Balance = this.Balance;
                    this.AddOperation(exit);
                    openedPosition--;
                }
            }

            // Evaluate opened position
            this.PositionValue = 0;
            foreach (var trade in trades.Where(t => !t.IsClosed))
            {
                var pos = this.Positions.FirstOrDefault(p => p.StockName == trade.Serie.StockName);
                if (pos == null)
                    continue;
                this.PositionValue += pos.EntryQty * trade.Serie.Values.Last().CLOSE;
            }
        }
        public void Clear()
        {
            using var ml = new MethodLogger(this);
            this.ClosedPositions.Clear();
            this.Positions.Clear();
            this.TradeOperations.Clear();
            this.Balance = this.InitialBalance;
            this.PositionValue = 0;
        }
        public void EvaluateOpenedPositions()
        {
            using var ml = new MethodLogger(this);
            // Calculate value for opened positions
            var positions = this.Positions.Where(p => !p.IsClosed);
            float positionValue = 0f;
            foreach (var pos in positions)
            {
                float value = PriceProvider.GetLastClosingPrice(pos.StockName);
                if (value == 0.0f)
                {
                    positionValue += pos.EntryQty * pos.EntryValue;
                }
                else
                {
                    positionValue += pos.EntryQty * value;
                }
            }
            this.PositionValue = positionValue;
        }

        public override string ToString()
        {
            return this.Name;
        }

        #region SAXO Integration Management

        private static readonly SortedDictionary<long, StockSerie> UicToSerieCache = new SortedDictionary<long, StockSerie>();

        public StockSerie GetStockSerieFromUic(long uic)
        {
            using var ml = new MethodLogger(this);
            if (UicToSerieCache.ContainsKey(uic))
                return UicToSerieCache[uic];

            StockSerie stockSerie = null;
            var instrument = instrumentService.GetInstrumentById(uic);
            if (instrument == null)
            {
                stockSerie = new StockSerie(uic.ToString(), uic.ToString(), StockSerie.Groups.ALL, StockDataProvider.Generated, BarDuration.Daily);
                UicToSerieCache.Add(uic, stockSerie);
                StockLog.Write($"Instrument: {uic} not found !");
                return stockSerie;
            }

            // Find StockSerie by ISIN
            if (!string.IsNullOrEmpty(instrument.Isin))
            {
                stockSerie = StockDictionary.Instance.Values.FirstOrDefault(s => s.ISIN == instrument.Isin);
                if (stockSerie != null)
                {
                    UicToSerieCache.Add(uic, stockSerie);
                    return stockSerie;
                }
                stockSerie = StockDictionary.Instance.Values.FirstOrDefault(s => s.ISIN == instrument.Isin);
                if (stockSerie != null)
                {
                    UicToSerieCache.Add(uic, stockSerie);
                    return stockSerie;
                }
            }

            // Find instrument in stock Dictionnary by Symbol
            var symbol = instrument.Symbol.Split(':')[0];
            var stockName = instrument.Description.ToUpper().Replace("SA", "").Replace("SCA", "").Trim();
            stockSerie = StockDictionary.Instance.Values.FirstOrDefault(s => (s.Symbol == symbol) || s.StockName == stockName);
            if (stockSerie == null)
            {
                if (instrument.ExchangeId == "CATS_SAXO" || instrument.AssetType == "WarrantOpenEndKnockOut")
                {
                    stockSerie = new StockSerie(instrument.Description, symbol, StockSerie.Groups.TURBO, StockDataProvider.SaxoIntraday, BarDuration.H_1);
                    stockSerie.ISIN = symbol;
                }
            }
            if (string.IsNullOrEmpty(instrument.Isin) && !string.IsNullOrEmpty(stockSerie?.ISIN))
            {
                instrument.Isin = stockSerie.ISIN;
                instrumentService.GetInstrumentByIsin(instrument.Isin);
            }
            UicToSerieCache.Add(uic, stockSerie);
            return stockSerie;
        }

        Account account = null;
        readonly AccountService accountService = new AccountService();
        readonly InstrumentService instrumentService = new InstrumentService();
        readonly OrderService orderService = new OrderService();
        private string name;

        public bool SaxoLogin()
        {
            using var ml = new MethodLogger(this, true, this.Name);
            if (this.IsSimu)
                return true;

            if (string.IsNullOrEmpty(SaxoAccountId))
            {
                MessageBox.Show("Missing Saxo Account Id, check portfolio file", "Saxo connection exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (string.IsNullOrEmpty(SaxoClientId))
            {
                MessageBox.Show("Missing Saxo Client Id, check portfolio file", "Saxo connection exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            var saxoSession = LoginService.Login(this.SaxoClientId, Folders.Portfolio, this.IsSaxoSimu);
            if (saxoSession == null)
                return false;

            if (account == null)
            {
                account = accountService.GetAccounts()?.FirstOrDefault(a => a.AccountId == this.SaxoAccountId);

                if (account == null)
                {
                    MessageBox.Show($"Account: {this.SaxoAccountId} not found !", "Porfolio sync error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            return true;
        }
        public bool SaxoSilentLogin()
        {
            using var ml = new MethodLogger(this, true, this.Name);
            if (string.IsNullOrEmpty(SaxoAccountId) || string.IsNullOrEmpty(SaxoClientId))
            {
                return false;
            }
            var saxoSession = LoginService.SilentLogin(this.SaxoClientId, Folders.Portfolio, this.IsSaxoSimu);
            if (saxoSession == null)
                return false;

            StockLog.Write($"Silent login success for portfolio: {this.Name}");
            if (account == null)
            {
                account = accountService.GetAccounts()?.FirstOrDefault(a => a.AccountId == this.SaxoAccountId);
                if (account == null)
                {
                    return false;
                }
            }
            return true;
        }

        public TimeSeries[] AccountValue { get; set; }


        private DateTime lastRefreshDate = DateTime.Today;
        public void Refresh(bool force = false)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (!force && lastRefreshDate.AddMinutes(1) > DateTime.Now)
                    return;

                if (!this.SaxoLogin())
                    return;

                this.GetPerformance();

                // Update portfolio balance
                var balance = accountService.GetBalance(account);
                if (balance != null)
                {
                    this.Balance = balance.CashAvailableForTrading;
                    this.PositionValue = balance.UnrealizedPositionsValue;
                    this.MaxValue = Math.Max(this.MaxValue, balance.TotalValue);

                    var lastValue = this.AccountValue.Last();
                    if (lastValue.Date == DateTime.Today)
                    {
                        lastValue.Value = balance.TotalValue - balance.CostToClosePositions;
                    }
                    else if (DateTime.Today.DayOfWeek != DayOfWeek.Saturday && DateTime.Today.DayOfWeek != DayOfWeek.Sunday && lastValue.Date < DateTime.Today)
                    {
                        this.AccountValue = this.AccountValue.Append(new TimeSeries { Date = DateTime.Today, Value = balance.TotalValue }).ToArray();
                    }
                }

                // Get Opened Orders
                this.SaxoOpenOrders.Clear();
                var saxoOpenedOrders = orderService.GetOpenedOrders(account);
                if (saxoOpenedOrders != null && saxoOpenedOrders.Length > 0)
                {
                    foreach (var order in saxoOpenedOrders)
                    {
                        var stockSerie = GetStockSerieFromUic(order.Uic);
                        var saxoOrder = new SaxoOrder(order);
                        saxoOrder.StockName = stockSerie == null ? order.Uic.ToString() : stockSerie.StockName;

                        this.SaxoOpenOrders.Add(saxoOrder);
                    }
                }

                // Check activity Orders
                var upToDate = DateTime.Today;
                var fromDate = this.LastSyncDate;
                var toDate = DateTime.Today.AddDays(1);
                var orders = orderService.GetOrderActivities(account, fromDate, toDate);
                if (orders != null && orders.Count > 0)
                {
                    foreach (var op in orders.OrderBy(o => o.LogId).Where(o => o.LogId > this.LastLogId).ToList())
                    {
                        this.AddSaxoActivityOrder(op);
                    }
                }

                this.LastSyncDate = DateTime.Today;
                this.Serialize();

                this.Refreshed?.Invoke(this);
                lastRefreshDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Porfolio sync error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public PriceInfo GetPrice(StockSerie stockSerie)
        {
            if (string.IsNullOrEmpty(stockSerie?.ISIN))
                return null;

            if (!this.SaxoSilentLogin())
                return null;

            var instrument = instrumentService.GetInstrumentByIsin(stockSerie.ISIN);
            if (instrument == null)
                return null;

            return instrumentService.GetInstrumentPrice(instrument, this.account);
        }

        public async Task<PriceInfo> GetPriceAsync(StockSerie stockSerie)
        {
            if (string.IsNullOrEmpty(stockSerie?.ISIN))
                return null;

            if (!this.SaxoSilentLogin())
                return null;

            var instrument = instrumentService.GetInstrumentByIsin(stockSerie.ISIN);
            if (instrument == null)
                return null;

            return await instrumentService.GetInstrumentPriceAsync(instrument, this.account);
        }

        public void GetPerformance()
        {
            if (!this.SaxoSilentLogin())
                return;

            if (this.AccountValue == null || this.AccountValue.Length == 0)
            {
                this.AccountValue = accountService.GetAccountValue(account, this.CreationDate);
            }
            else
            {
                var lastCacDate = StockDictionary.Instance["CAC40"].LastValue.DATE.Date;
                var lastDate = this.AccountValue.Last().Date;
                if (lastDate <= lastCacDate)
                {
                    var newAccountValues = accountService.GetAccountValue(account, lastDate);
                    if (newAccountValues != null && newAccountValues.Length > 0)
                    {
                        this.AccountValue = this.AccountValue.Where(av => av.Date < lastDate).Concat(newAccountValues).ToArray();
                    }
                }
            }
        }
        public void AddSaxoActivityOrder(OrderActivity activityOrder)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            if (activityOrder.LogId <= this.LastLogId)
                return;

            this.SaxoOrderActivity.Insert(0, activityOrder);

            activityOrder.ActivityTime = activityOrder.ActivityTime.ToLocalTime();

            switch (activityOrder.Status)
            {
                case "FinalFill": // Order fully executed
                    {
                        var order = new SaxoOrder(activityOrder);
                        this.SaxoOrders.Add(order);
                        var stockSerie = GetStockSerieFromUic(order.Uic);
                        if (stockSerie == null)
                        {
                            StockLog.Write($"StockSerie for UIC:{order.Uic} not found");
                            var instrument = instrumentService.GetInstrumentById(order.Uic);
                            if (instrument == null)
                                return;
                            order.StockName = instrument.Description;
                            order.Isin = instrument.Isin;
                        }
                        else
                        {
                            order.StockName = stockSerie.StockName;
                            order.Isin = stockSerie.ISIN;
                        }
                        ProcessFullyFilledOrder(activityOrder, order);
                    }
                    break;
                case "Changed": // Update stop loss or buy order
                    {
                        if (activityOrder.BuySell != "Buy" && activityOrder.OrderType != "Market")
                        {
                            var position = this.Positions.FirstOrDefault(p => p.Uic == activityOrder.Uic);
                            if (position != null)
                            {
                                position.TrailStopId = activityOrder.OrderId;
                                position.TrailStop = activityOrder.Price.Value;
                                if (position.Stop == 0)
                                    position.Stop = position.TrailStop;
                            }
                        }
                    }
                    break;
                case "Fill": // Order partially executed
                    break;
                case "Expired":
                case "Cancelled": // Order cancelled remove if it's a stop order.
                    {
                        if (activityOrder.OrderRelation == "StandAlone" && activityOrder.BuySell == "Sell")
                        {
                            var position = this.Positions.FirstOrDefault(p => p.TrailStopId == activityOrder.OrderId);
                            if (position != null)
                            {
                                position.TrailStopId = 0;
                                position.TrailStop = 0;
                            }
                        }
                    }
                    break;
                case "Placed": // Order sent to market
                    if (activityOrder.SubStatus == "Rejected")
                    {
                        break;
                    }
                    // Check if sell order on opened position.
                    if (activityOrder.BuySell == "Sell" && activityOrder.OrderRelation == "StandAlone" && !(activityOrder.OrderType == "Market" || activityOrder.OrderType == "Limit"))
                    {
                        var position = this.Positions.FirstOrDefault(p => p.Uic == activityOrder.Uic);
                        if (position != null)
                        {
                            position.TrailStopId = activityOrder.OrderId;
                            position.TrailStop = activityOrder.Price.Value;
                            if (position.Stop == 0)
                                position.Stop = position.TrailStop;
                        }
                        else
                        {
                            StockLog.Write($"Selling not opened position: {activityOrder.Uic} qty:{activityOrder.Amount}");
                        }
                    }
                    break;
                case "Working": // Order waiting for execution ==> noting to do.
                case "DoneForDay": // noting to do.
                    break;
                default:
                    throw new NotImplementedException($"AddSaxoActivityOrder Order Status:{activityOrder.Status} not implemented");
            }

            this.LastLogId = Math.Max(this.LastLogId, activityOrder.LogId);
        }

        private void ProcessFullyFilledOrder(OrderActivity activityOrder, SaxoOrder order)
        {
            var position = this.Positions.FirstOrDefault(p => p.Uic == order.Uic);
            if (activityOrder.BuySell == "Buy")
            {
                if (position == null)
                {
                    // Create new Position
                    position = new StockPosition
                    {
                        Id = order.PositionId.Value,
                        Uic = order.Uic,
                        EntryDate = order.ActivityTime,
                        EntryQty = order.Qty,
                        StockName = order.StockName,
                        ISIN = order.Isin,
                        EntryValue = order.AveragePrice.Value,
                        PortfolioValue = this.TotalValue
                    };
                    this.Positions.Add(position);

                    // Check if related stop order placed before order filled
                    if (activityOrder.RelatedOrders?.Count > 0)
                    {
                        var stopOrder = this.SaxoOrderActivity.FirstOrDefault(o => o.Uic == order.Uic && o.Status == "Placed" && activityOrder.RelatedOrders.Contains(o.OrderId));
                        if (stopOrder != null)
                        {
                            position.TrailStopId = stopOrder.OrderId;
                            position.TrailStop = stopOrder.Price.Value;
                            position.Stop = stopOrder.Price.Value;
                        }
                    }
                }
                else // Increase existing position
                {
                    position.EntryValue = (position.EntryValue * position.EntryQty + order.AveragePrice.Value * order.Qty) / (position.EntryQty + order.Qty);
                    position.EntryQty += order.Qty;
                }
            }
            else
            {
                if (position == null)
                {
                    StockLog.Write($"Selling not opened position: {order.StockName} qty:{order.Qty}");
                    return;
                }
                if (position.EntryQty <= order.Qty)
                {
                    position.ExitValue = order.AveragePrice.Value;
                    position.ExitDate = order.ActivityTime;
                    this.Positions.Remove(position);
                    this.ClosedPositions.Add(position);
                }
                else
                {
                    position.EntryQty -= order.Qty;
                }
            }
        }

        public Position[] SaxoGetPositions()
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                return accountService?.GetPositions(account);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public OpenedOrder[] SaxoGetOrders(long uic = 0)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                return orderService?.GetOpenedOrders(account, uic);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public OrderActivity[] SaxoGetOrderActivities(long orderId)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                return orderService?.GetOrderActivities(account, orderId)?.Data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Position SaxoGetPosition(StockSerie stockSerie)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                var instrument = instrumentService.GetInstrumentByIsin(stockSerie.ISIN == null ? stockSerie.Symbol : stockSerie.ISIN);
                if (instrument == null)
                    return null;

                return accountService?.GetPositions(account).FirstOrDefault(p => p.PositionBase.Uic == instrument.Identifier);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public Position SaxoGetPosition(long positionId)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                return accountService?.GetPositionById(account, positionId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public long SaxoBuyOrder(StockSerie stockSerie, OrderType orderType, int qty, float stopValue = 0, float orderValue = 0, bool t1 = false)
        {
            using var ml = new MethodLogger(this, true, $"{this.Name} Buy {qty}-{orderType}-{stockSerie.StockName}  Stop:{stopValue}");
            try
            {
                if (!this.SaxoLogin())
                    return 0;

                var instrument = instrumentService.GetInstrumentByIsin(stockSerie.ISIN == null ? stockSerie.Symbol : stockSerie.ISIN);
                if (instrument == null)
                {
                    MessageBox.Show($"Instrument: {stockSerie.StockName}:{stockSerie.ISIN} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0;
                }

                var instrumentDetail = instrumentService.GetInstrumentDetailsById(instrument.Identifier, instrument.AssetType, account);
                if (instrumentDetail == null)
                {
                    MessageBox.Show($"InstrumentDetails: {stockSerie.StockName}:{stockSerie.ISIN} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0;
                }
                decimal stop = instrumentDetail.RoundToTickSize(stopValue);

                OrderResponse orderResponse = null;
                switch (orderType)
                {
                    case OrderType.Market:
                        orderResponse = orderService.BuyMarketOrder(account, instrument, qty, stop);
                        break;
                    case OrderType.Limit:
                        decimal limit = instrumentDetail.RoundToTickSize(orderValue);
                        orderResponse = orderService.BuyLimitOrder(account, instrument, qty, limit, stop, t1);
                        break;
                    case OrderType.Threshold:
                        decimal threshold = instrumentDetail.RoundToTickSize(orderValue);
                        orderResponse = orderService.BuyTresholdOrder(account, instrument, qty, threshold, stop);
                        break;
                    default:
                        break;
                }

                StockLog.Write($"OrderResponse.OrderId:{orderResponse?.OrderId}");

                this.Refresh(true);
                if (!string.IsNullOrEmpty(orderResponse?.OrderId))
                {
                    return long.Parse(orderResponse?.OrderId);
                }
            }
            catch (SaxoApiException ex)
            {
                MessageBox.Show(ex.ErrorInfo.Message, "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return 0;
        }
        public long SaxoSellOrder(StockSerie stockSerie, OrderType orderType, int qty, float orderValue = 0)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (!this.SaxoLogin())
                    return 0;

                var instrument = instrumentService.GetInstrumentByIsin(stockSerie.ISIN == null ? stockSerie.Symbol : stockSerie.ISIN);
                if (instrument == null)
                {
                    MessageBox.Show($"Instrument: {stockSerie.StockName}:{stockSerie.ISIN} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0;
                }

                var instrumentDetail = instrumentService.GetInstrumentDetailsById(instrument.Identifier, instrument.AssetType, account);
                if (instrumentDetail == null)
                {
                    MessageBox.Show($"InstrumentDetails: {stockSerie.StockName}:{stockSerie.ISIN} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0;
                }

                OrderResponse orderResponse = null;
                switch (orderType)
                {
                    case OrderType.Market:
                        orderResponse = orderService.SellMarketOrder(account, instrument, qty);
                        break;
                    case OrderType.Limit:
                        decimal limit = instrumentDetail.RoundToTickSize(orderValue);
                        orderResponse = orderService.SellLimitOrder(account, instrument, qty, limit);
                        break;
                    case OrderType.Threshold:
                        decimal threshold = instrumentDetail.RoundToTickSize(orderValue);
                        orderResponse = orderService.SellStopOrder(account, instrument, qty, threshold);
                        break;
                    default:
                        break;
                }
                this.Refresh();
                return long.Parse(orderResponse?.OrderId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return 0;
        }
        public string SaxoUpdateStopOrder(StockPosition position, float exitValue)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (!this.SaxoLogin())
                    return null;

                var instrument = instrumentService.GetInstrumentById(position.Uic);
                if (instrument == null)
                {
                    MessageBox.Show($"Instrument:{position.StockName} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                var instrumentDetail = instrumentService.GetInstrumentDetailsById(instrument.Identifier, instrument.AssetType, account);
                if (instrumentDetail == null)
                {
                    MessageBox.Show($"InstrumentDetails: {position.StockName}:{instrument.Symbol} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                decimal value = instrumentDetail.RoundToTickSize(exitValue);
                OrderResponse orderResponse = null;
                if (position.TrailStopId == 0)
                {
                    orderResponse = orderService.SellStopOrder(account, instrument, position.EntryQty, value);
                    position.TrailStopId = orderResponse?.OrderId == null ? 0 : long.Parse(orderResponse.OrderId);
                }
                else
                {
                    orderResponse = orderService.PatchOrder(account, instrument, position.TrailStopId, SaxoOrderType.StopIfTraded.ToString(), "Sell", position.EntryQty, value);
                }
                if (!string.IsNullOrEmpty(orderResponse?.OrderId))
                {
                    position.TrailStop = (float)value;
                    if (position.Stop == 0) { position.Stop = (float)value; }
                    this.Serialize();
                }
                return orderResponse?.OrderId;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }
        public long SaxoClosePosition(long positionId)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (!this.SaxoSilentLogin())
                    return 0;

                var position = this.SaxoGetPosition(positionId);
                if (position == null)
                    return 0;

                // Close related Orders
                var openOrders = this.SaxoGetOrders(position.PositionBase.Uic);
                if (openOrders != null)
                {
                    foreach (var order in openOrders.Where(o => o.BuySell == "Sell"))
                    {
                        orderService.CancelOrder(account, order.OrderId);
                    }
                }

                var instrument = this.GetInstrument(position.PositionBase.Uic);
                OrderResponse orderResponse = orderResponse = orderService.SellMarketOrder(account, instrument, (int)position.PositionBase.Amount);
                return orderResponse?.OrderId == null ? 0 : long.Parse(orderResponse.OrderId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return 0;
        }

        public string SaxoClosePosition(StockPosition position, OrderType orderType, float exitValue = 0.0f)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (!this.SaxoLogin())
                    return null;

                var instrument = instrumentService.GetInstrumentById(position.Uic);
                if (instrument == null)
                {
                    MessageBox.Show($"Instrument:{position.StockName} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                var instrumentDetail = instrumentService.GetInstrumentDetailsById(instrument.Identifier, instrument.AssetType, account);
                if (instrumentDetail == null)
                {
                    MessageBox.Show($"InstrumentDetails: {position.StockName}:{instrument.Symbol} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                OrderResponse orderResponse = null;
                switch (orderType)
                {
                    case OrderType.Market:
                        if (position.TrailStopId != 0)
                        {
                            if (!orderService.CancelOrder(account, position.TrailStopId))
                                return null;
                        }
                        if (position.LimitOrderId != 0)
                        {
                            if (!orderService.CancelOrder(account, position.LimitOrderId))
                                return null;
                        }
                        orderResponse = orderService.SellMarketOrder(account, instrument, position.EntryQty);
                        break;
                    case OrderType.Limit:
                        decimal value = instrumentDetail.RoundToTickSize(exitValue);
                        if (position.LimitOrderId != 0)
                        {
                            orderResponse = orderService.PatchOrder(account, instrument, position.LimitOrderId, SaxoOrderType.Market.ToString(), "Sell", position.EntryQty, 0);
                        }
                        else
                        {
                            orderResponse = orderService.SellLimitOrder(account, instrument, position.EntryQty, instrumentDetail.RoundToTickSize(exitValue));
                            long.TryParse(orderResponse?.OrderId, out var id);
                            position.LimitOrderId = id;
                        }
                        break;
                    case OrderType.Threshold:
                        break;
                }
                return orderResponse?.OrderId;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }
        public string SaxoUpdateOpenOrder(StockOpenedOrder openOrder, float newValue)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (!this.SaxoLogin())
                    return null;

                var instrument = instrumentService.GetInstrumentById(openOrder.Uic);
                if (instrument == null)
                {
                    MessageBox.Show($"Instrument:{openOrder.StockName} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                var instrumentDetail = instrumentService.GetInstrumentDetailsById(instrument.Identifier, instrument.AssetType, account);
                if (instrumentDetail == null)
                {
                    MessageBox.Show($"InstrumentDetails: {openOrder.StockName}:{instrument.Symbol} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                decimal value = instrumentDetail.RoundToTickSize(newValue);
                OrderResponse orderResponse = null;
                orderResponse = orderService.PatchOrder(account, instrument, openOrder.Id, openOrder.OrderType, openOrder.BuySell, openOrder.Qty, value);
                if (!string.IsNullOrEmpty(orderResponse?.OrderId))
                {
                    openOrder.Value = (float)value;
                    this.Serialize();
                }
                return orderResponse?.OrderId;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }
        public void SaxoCancelOpenOrder(long orderId)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (orderId <= 0)
                    return;
                if (!this.SaxoLogin())
                    return;

                if (orderService.CancelOrder(account, orderId))
                {
                    this.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cancel order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return;
        }

        public Instrument GetInstrument(long uic)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (!this.SaxoLogin())
                    return null;

                return instrumentService.GetInstrumentById(uic);
            }
            catch (Exception)
            {
            }
            return null;
        }
        public InstrumentDetails GetInstrumentDetails(StockSerie stockSerie)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (!this.SaxoLogin())
                    return null;

                var instrument = instrumentService.GetInstrumentByIsin(stockSerie.ISIN == null ? stockSerie.Symbol : stockSerie.ISIN);
                if (instrument == null)
                {
                    MessageBox.Show($"Instrument: {stockSerie.StockName}:{stockSerie.ISIN} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                var instrumentDetail = instrumentService.GetInstrumentDetailsById(instrument.Identifier, instrument.AssetType, account);
                if (instrumentDetail == null)
                {
                    MessageBox.Show($"InstrumentDetails: {stockSerie.StockName}:{stockSerie.ISIN} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
                return instrumentDetail;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }

        #endregion
    }
}