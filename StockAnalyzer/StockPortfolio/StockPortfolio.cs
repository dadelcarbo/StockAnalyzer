using Newtonsoft.Json;
using Saxo.OpenAPI.AuthenticationServices;
using Saxo.OpenAPI.TradingServices;
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
using System.Windows;
using System.Xml.Serialization;

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
        public float MaxRisk { get; set; }
        public float MaxPositionSize { get; set; }
        [JsonIgnore]
        public float Return => (TotalValue - InitialBalance) / InitialBalance;
        public bool IsSimu { get; set; }
        public bool IsSaxoSimu { get; set; }

        public List<OrderActivity> ActivityOrders { get; } = new List<OrderActivity>();
        public List<SaxoPosition> SaxoPositions { get; } = new List<SaxoPosition>();

        public IEnumerable<StockOpenedOrder> GetOpenOrders(string stockName)
        {
            return this.ActivityOrders.Where(o => o.StockName == stockName && o.Status == "Working").Select(o => new StockOpenedOrder(o));
        }
        public IEnumerable<StockOpenedOrder> GetOpenOrders()
        {
            return this.ActivityOrders.Where(o => o.Status == "Working").Select(o => new StockOpenedOrder(o));
        }
        public List<StockPosition> Positions { get; } = new List<StockPosition>();
        public List<StockPosition> ClosedPositions { get; } = new List<StockPosition>();

        public List<StockTradeOperation> TradeOperations { get; set; }

        #region PERSISTENCY
        public void RenameFile(string newName)
        {
            string filepath = Path.Combine(Folders.Portfolio, this.Name + PORTFOLIO_FILE_EXT);
            string newFilepath = Path.Combine(Folders.Portfolio, newName + PORTFOLIO_FILE_EXT);
            if (File.Exists(filepath))
            {
                File.Move(filepath, newFilepath);
            }
        }
        public void Serialize()
        {
            string filepath = Path.Combine(Folders.Portfolio, this.Name + PORTFOLIO_FILE_EXT);

            // Archive PTF files
            string archiveDirectory = Path.Combine(Folders.Portfolio, "Archive");
            if (!Directory.Exists(archiveDirectory))
                Directory.CreateDirectory(archiveDirectory);
            else
            {
                var dateLimit = DateTime.Today.AddDays(-50);
                foreach (var file in Directory.EnumerateFiles(archiveDirectory).Where(f => File.GetLastWriteTime(f) < dateLimit))
                {
                    File.Delete(file);
                }
            }

            File.WriteAllText(filepath, JsonConvert.SerializeObject(this, Formatting.Indented, jsonSerializerSettings));
            var fileDate = File.GetLastWriteTime(filepath);
            var archiveFilePath = Path.Combine(archiveDirectory, this.Name + "_" + fileDate.ToString("yyyy_MM_dd HH_mm_ss.ff") + PORTFOLIO_FILE_EXT);
            File.Copy(filepath, archiveFilePath);
        }
        static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings { DateFormatString = @"yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffZ" };
        public static StockPortfolio Deserialize(string filepath)
        {
            return JsonConvert.DeserializeObject<StockPortfolio>(File.ReadAllText(filepath), jsonSerializerSettings);
        }
        public static List<StockPortfolio> LoadPortfolios(string folder)
        {
            try
            {
                LoadMappings();

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
            this.Positions.Clear();
            this.TradeOperations.Clear();
            this.Balance = this.InitialBalance;
            this.PositionValue = 0;
        }

        public void Dump()
        {
            StockLog.Write($"All Positions: {this.Positions.Count}");
            StockLog.Write($"Opened Positions: {this.Positions.Count()}");

            foreach (var p in this.Positions.OrderBy(p => p.StockName))
            {
                p.Dump();
            }
        }
        public void Dump(DateTime date)
        {
            StockLog.Write($"Dump for date:{date.ToShortDateString()}");
            StockLog.Write($"All Positions: {this.Positions.Count}");
            StockLog.Write($"Opened Positions: {this.Positions.Count()}");

            foreach (var p in this.Positions.Where(p => p.EntryDate < date && p.ExitDate > date).OrderBy(p => p.StockName))
            {
                p.Dump();
            }
        }
        public float EvaluateOpenedPositionsAt(DateTime date, StockClasses.BarDuration duration, out long volume)
        {
            // Calculate value for opened positions
            var positions = this.Positions.Where(p => (p.ExitDate == null || p.ExitDate > date) && p.EntryDate <= date);
            float positionValue = 0f;
            volume = 0;
            foreach (var pos in positions)
            {
                volume++;
                float value = PriceProvider.GetClosingPrice(pos.StockName, date, duration);
                if (value == 0.0f)
                {
                    positionValue += pos.EntryQty * pos.EntryValue;
                }
                else
                {
                    positionValue += pos.EntryQty * value;
                }
            }
            return positionValue;
        }
        public void EvaluateOpenedPositions()
        {
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


        #region SAXO Name Mapping
        private static List<StockNameMapping> mappings;
        public static List<StockNameMapping> Mappings => LoadMappings();

        public static void ResetMappings()
        {
            mappings = null;
        }
        public static List<StockNameMapping> LoadMappings()
        {
            if (mappings != null)
                return mappings;
            var fileName = Path.Combine(Folders.Portfolio, "NameMappings.xml");
            if (File.Exists(fileName))
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
                    settings.IgnoreWhitespace = true;
                    System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);
                    var serializer = new XmlSerializer(typeof(List<StockNameMapping>));
                    mappings = (List<StockNameMapping>)serializer.Deserialize(xmlReader);
                }
            }
            else
            {
                mappings = new List<StockNameMapping>();
            }
            return mappings;
        }
        public static void SaveMappings(List<StockNameMapping> newMappings)
        {
            var fileName = Path.Combine(Folders.Portfolio, "NameMappings.xml");
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                var settings = new System.Xml.XmlWriterSettings
                {
                    Indent = true,
                    NewLineOnAttributes = true
                };
                var xmlWriter = System.Xml.XmlWriter.Create(fs, settings);
                var serializer = new XmlSerializer(typeof(List<StockNameMapping>));
                serializer.Serialize(xmlWriter, newMappings);
            }
            ResetMappings();
        }

        public static StockNameMapping GetMapping(string saxoName, string isin)
        {
            if (!string.IsNullOrEmpty(isin))
            {
                var stockSerie = StockDictionary.Instance.Values.FirstOrDefault(s => s.ISIN == isin);
                if (stockSerie != null)
                    return new StockNameMapping { SaxoName = saxoName, StockName = stockSerie.StockName };
            }
            if (saxoName == null)
                return null;
            return Mappings.FirstOrDefault(m => saxoName.Contains(m.SaxoName.ToUpper()));
        }
        #endregion

        public override string ToString()
        {
            return this.Name;
        }


        #region SAXO Integration Management

        private static SortedDictionary<long, StockSerie> UicToSerieCache = new SortedDictionary<long, StockSerie>();

        public StockSerie GetStockSerieFromUic(long uic)
        {
            if (UicToSerieCache.ContainsKey(uic))
                return UicToSerieCache[uic];

            var instrument = instrumentService.GetInstrumentById(uic);
            if (instrument == null)
            {
                UicToSerieCache.Add(uic, null);
                StockLog.Write($"Instrument: {uic} not found !");
                return null;
            }

            // Find StockSerie by ISIN
            StockSerie stockSerie = null;
            if (!string.IsNullOrEmpty(instrument.Isin))
            {
                stockSerie = StockDictionary.Instance.Values.FirstOrDefault(s => s.ISIN == instrument.Isin);
                UicToSerieCache.Add(uic, stockSerie);
                if (stockSerie != null)
                    return stockSerie;
            }

            // Find instrument in stock Dictionnary
            var symbol = instrument.Symbol.Split(':')[0];
            var stockName = instrument.Description.ToUpper().Replace("SA", "").Trim();
            stockSerie = StockDictionary.Instance.Values.FirstOrDefault(s => s.Symbol.Split('.')[0] == symbol || s.StockName == stockName || (instrument.Isin != null && s.ISIN == instrument.Isin));
            if (stockSerie == null)
            {
                if (instrument.ExchangeId == "CATS_SAXO" || instrument.AssetType == "WarrantOpenEndKnockOut")
                {
                    stockSerie = new StockSerie(instrument.Description, symbol, StockSerie.Groups.INTRADAY, StockDataProvider.SaxoIntraday, BarDuration.H_1);
                    stockSerie.ISIN = symbol;
                }
            }
            if (string.IsNullOrEmpty(instrument.Isin) && !string.IsNullOrEmpty(stockSerie.ISIN))
            {
                instrument.Isin = stockSerie.ISIN;
                instrumentService.GetInstrumentByIsin(instrument.Isin);
            }
            UicToSerieCache.Add(uic, stockSerie);
            return stockSerie;
        }

        Account account = null;
        AccountService accountService = new AccountService();
        InstrumentService instrumentService = new InstrumentService();
        OrderService orderService = new OrderService();
        private string name;

        public bool SaxoLogin()
        {
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

            account = accountService.GetAccounts()?.FirstOrDefault(a => a.AccountId == this.SaxoAccountId);
            if (account == null)
            {
                MessageBox.Show($"Account: {this.SaxoAccountId} not found !", "Porfolio sync error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
        public bool SaxoSilentLogin()
        {
            if (string.IsNullOrEmpty(SaxoAccountId))
            {
                return false;
            }
            if (string.IsNullOrEmpty(SaxoClientId))
            {
                return false;
            }
            var saxoSession = LoginService.SilentLogin(this.SaxoClientId, Folders.Portfolio, this.IsSaxoSimu);
            if (saxoSession == null)
                return false;

            StockLog.Write($"Silent login success for portfolio: {this.Name}");

            account = accountService.GetAccounts()?.FirstOrDefault(a => a.AccountId == this.SaxoAccountId);
            if (account == null)
            {
                return false;
            }
            this.Refresh();
            return true;
        }

        public void Refresh()
        {
            try
            {
                if (!this.SaxoLogin())
                    return;

                // Update portfolio balance
                var balance = accountService.GetBalance(account);
                if (balance != null)
                {
                    this.Balance = balance.CashAvailableForTrading;
                }

                // Check activity Orders
                var upToDate = DateTime.Today;
                var fromDate = this.LastSyncDate;
                var toDate = DateTime.Today.AddDays(1);
                var orders = orderService.GetOrderActivities(account, fromDate, toDate);
                if (orders?.Data != null && orders.Data.Length > 0)
                {
                    foreach (var op in orders.Data.OrderBy(o => o.LogId).ToList())
                    {
                        this.AddSaxoActivityOrder(op);
                    }
                }

                this.LastSyncDate = DateTime.Today;
                this.Serialize();

                this.Refreshed?.Invoke(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Porfolio sync error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void AddSaxoActivityOrder(OrderActivity activityOrder)
        {
            // Check if order already treated
            activityOrder.ActivityTime = activityOrder.ActivityTime.ToLocalTime();
            var order = this.ActivityOrders.FirstOrDefault(o => o.OrderId == activityOrder.OrderId);
            if (order != null)
            {
                if (activityOrder.LogId <= order.LogId) // Order activity already managed
                    return;
                order.CopyFrom(activityOrder);
            }
            else
            {
                this.ActivityOrders.Add(activityOrder);
                order = activityOrder;
                activityOrder.CreationTime = activityOrder.ActivityTime;
            }

            var stockSerie = GetStockSerieFromUic(order.Uic);
            if (stockSerie == null)
            {
                throw new InvalidOperationException($"StockSerie for UIC:{order.Uic} not found");
            }
            activityOrder.StockName = stockSerie.StockName;

            switch (activityOrder.Status)
            {
                case "Changed": // Update stop loss or buy order
                    {
                        if (activityOrder.BuySell != "Buy")
                        {
                            var position = this.Positions.FirstOrDefault(p => p.Uic == order.Uic && p.TrailStopId == activityOrder.OrderId.ToString());
                            if (position != null)
                                position.TrailStop = activityOrder.Price.Value;
                        }
                        activityOrder.Status = "Working";
                    }
                    return;
                case "FinalFill": // Order fully executed
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
                                    EntryQty = (int)order.Amount,
                                    StockName = stockSerie.StockName,
                                    ISIN = stockSerie.ISIN,
                                    EntryValue = order.AveragePrice.Value
                                };
                                this.Positions.Add(position);

                                if (activityOrder.RelatedOrders != null)
                                {
                                    foreach (var orderId in activityOrder.RelatedOrders)
                                    {
                                        var relatedOrder = this.ActivityOrders.FirstOrDefault(o => o.OrderId == long.Parse(orderId));
                                        if (relatedOrder != null)
                                        {
                                            relatedOrder.Status = "Working";
                                            position.TrailStopId = orderId;
                                        }

                                        position.Stop = relatedOrder.Price.Value;
                                        position.TrailStop = relatedOrder.Price.Value;
                                    }
                                }
                            }
                            else // Increase existing position
                            {

                            }
                        }
                        else
                        {
                            if (position == null)
                            {
                                StockLog.Write($"Selling not opened position: {stockSerie.StockName} qty:{(int)order.Amount}");
                                return;
                            }
                            position.ExitValue = order.AveragePrice.Value;
                            position.ExitDate = order.ActivityTime;
                            this.Positions.Remove(position);
                            this.ClosedPositions.Add(position);
                        }
                    }
                    break;
                case "Fill": // Order partially executed
                    activityOrder.Status = "Working";
                    break;
                case "Placed": // Order sent to market
                case "Cancelled": // Order cancelled remove if it's a stop order.
                case "Working": // Order waiting for execution ==> noting to do.
                case "DoneForDay": // noting to do.
                    break;
                default:
                    throw new NotImplementedException($"AddSaxoActivityOrder Order Status:{activityOrder.Status} not implemented");
            }
        }
        public long SaxoBuyOrder(StockSerie stockSerie, OrderType orderType, int qty, float stopValue = 0, float orderValue = 0)
        {
            try
            {
                if (!this.SaxoLogin())
                    return 0;

                var instrument = instrumentService.GetInstrumentByIsin(stockSerie.ISIN == null ? stockSerie.Symbol : stockSerie.ISIN);
                if (instrument == null)
                {
                    MessageBox.Show($"Instrument: {stockSerie.StockName}:{stockSerie.StockName} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0;
                }

                var instrumentDetail = instrumentService.GetInstrumentDetailsById(instrument.Identifier, instrument.AssetType, account);
                if (instrumentDetail == null)
                {
                    MessageBox.Show($"InstrumentDetails: {stockSerie.StockName}:{stockSerie.StockName} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        orderResponse = orderService.BuyLimitOrder(account, instrument, qty, limit, stop);
                        break;
                    case OrderType.Threshold:
                        decimal threshold = instrumentDetail.RoundToTickSize(orderValue);
                        orderResponse = orderService.BuyTresholdOrder(account, instrument, qty, threshold, stop);
                        break;
                    default:
                        break;
                }
                if (!string.IsNullOrEmpty(orderResponse?.OrderId))
                {
                    this.Refresh();
                    return long.Parse(orderResponse?.OrderId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return 0;
        }
        public string SaxoSellOrder(StockSerie stockSerie, OrderType orderType, int qty, float orderValue = 0)
        {
            try
            {
                if (!this.SaxoLogin())
                    return null;

                var instrument = instrumentService.GetInstrumentByIsin(stockSerie.ISIN == null ? stockSerie.Symbol : stockSerie.ISIN);
                if (instrument == null)
                {
                    MessageBox.Show($"Instrument: {stockSerie.StockName}:{stockSerie.StockName} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                var instrumentDetail = instrumentService.GetInstrumentDetailsById(instrument.Identifier, instrument.AssetType, account);
                if (instrumentDetail == null)
                {
                    MessageBox.Show($"InstrumentDetails: {stockSerie.StockName}:{stockSerie.StockName} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
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
                return orderResponse.OrderId;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }
        public string SaxoUpdateStopOrder(StockPosition position, float exitValue)
        {
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
                if (string.IsNullOrEmpty(position.TrailStopId))
                {
                    orderResponse = orderService.SellStopOrder(account, instrument, position.EntryQty, value);
                    position.TrailStopId = orderResponse.OrderId;
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
        public string SaxoClosePosition(StockPosition position, OrderType orderType, float exitValue = 0.0f)
        {
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
                        if (!string.IsNullOrEmpty(position.TrailStopId))
                        {
                            if (!orderService.CancelOrder(account, position.TrailStopId))
                                return null;
                        }
                        if (!string.IsNullOrEmpty(position.LimitOrderId))
                        {
                            if (!orderService.CancelOrder(account, position.LimitOrderId))
                                return null;
                        }
                        orderResponse = orderService.SellMarketOrder(account, instrument, position.EntryQty);
                        position.TrailStopId = null;
                        break;
                    case OrderType.Limit:
                        decimal value = instrumentDetail.RoundToTickSize(exitValue);
                        if (!string.IsNullOrEmpty(position.LimitOrderId))
                        {
                            orderResponse = orderService.PatchOrder(account, instrument, position.LimitOrderId, SaxoOrderType.Market.ToString(), "Sell", position.EntryQty, 0);
                        }
                        else
                        {
                            orderResponse = orderService.SellLimitOrder(account, instrument, position.EntryQty, instrumentDetail.RoundToTickSize(exitValue));
                            position.LimitOrderId = orderResponse.OrderId;
                        }
                        break;
                    case OrderType.Threshold:
                        break;
                }
                //if (string.IsNullOrEmpty(position.TrailStopId))
                //{
                //    orderResponse = orderService.SellMarketOrder(account, instrument, position.EntryQty);
                //    position.TrailStopId = orderResponse.OrderId;
                //}
                //else
                //{
                //    orderResponse = orderService.PatchOrder(account, instrument, position.TrailStopId, SaxoOrderType.Market.ToString(), "Sell", position.EntryQty, 0);
                //}
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
                orderResponse = orderService.PatchOrder(account, instrument, openOrder.Id.ToString(), openOrder.OrderType, openOrder.BuySell, openOrder.Qty, value);
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
        public void SaxoCancelOpenOrder(string orderId)
        {
            try
            {
                if (!this.SaxoLogin())
                    return;

                if (orderService.CancelOrder(account, orderId))
                {
                    this.Serialize();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cancel order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return;
        }

        public InstrumentDetails GetInstrumentDetails(StockSerie stockSerie)
        {
            try
            {
                if (!this.SaxoLogin())
                    return null;

                var instrument = instrumentService.GetInstrumentByIsin(stockSerie.ISIN == null ? stockSerie.Symbol : stockSerie.ISIN);
                if (instrument == null)
                {
                    MessageBox.Show($"Instrument: {stockSerie.StockName}:{stockSerie.StockName} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                var instrumentDetail = instrumentService.GetInstrumentDetailsById(instrument.Identifier, instrument.AssetType, account);
                if (instrumentDetail == null)
                {
                    MessageBox.Show($"InstrumentDetails: {stockSerie.StockName}:{stockSerie.StockName} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
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