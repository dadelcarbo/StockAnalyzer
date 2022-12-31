using Newtonsoft.Json;
using Saxo.OpenAPI.AuthenticationServices;
using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockAgent;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Forms.VisualStyles;
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
            this.Positions = new List<StockPosition>();
            this.OpenOrders = new List<StockOpenedOrder>();
            this.MaxRisk = 0.02f;
            this.MaxPositionSize = 0.2f;
        }
        public List<StockTradeOperation> TradeOperations { get; set; }
        public List<StockPosition> Positions { get; }
        [JsonIgnore]
        public IEnumerable<StockPosition> OpenedPositions => Positions.Where(p => !p.IsClosed);
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
        public DateTime LastSyncDate { get; set; }
        public float InitialBalance { get; set; }
        public float Balance { get; set; }
        public float MaxRisk { get; set; }
        public float MaxPositionSize { get; set; }
        public DateTime CreationDate { get; set; }
        [JsonIgnore]
        public float PositionValue { get; set; }
        [JsonIgnore]
        public float TotalValue => this.Balance + this.PositionValue;
        [JsonIgnore]
        public float Return => (TotalValue - InitialBalance) / InitialBalance;
        public bool IsSimu { get; set; }
        public bool IsSaxoSimu { get; set; }

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
                var dateLimit = DateTime.Today.AddDays(-5);
                foreach (var file in Directory.EnumerateFiles(archiveDirectory).Where(f => File.GetLastWriteTime(f) < dateLimit))
                {
                    File.Delete(file);
                }
            }
            if (File.Exists(filepath))
            {
                var fileDate = File.GetLastWriteTime(filepath);
                var archiveFilePath = Path.Combine(archiveDirectory, this.Name + "_" + fileDate.ToString("yyyy_MM_dd hh_mm_ss_fff") + PORTFOLIO_FILE_EXT);
                if (!File.Exists(archiveFilePath))
                {
                    File.Move(filepath, archiveFilePath);
                }
            }

            File.WriteAllText(filepath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public static StockPortfolio Deserialize(string filepath)
        {
            return JsonConvert.DeserializeObject<StockPortfolio>(File.ReadAllText(filepath));
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
        public void BuyTradeOperation(string stockName, DateTime date, int qty, float value, float fee, float stop, string entryComment, BarDuration barDuration, string theme, long id = -1)
        {
            var amount = value * qty + fee;
            if (this.Balance < amount)
            {
                throw new InvalidOperationException($"You have insufficient cash to buy {qty} {stockName}");
            }
            this.Balance -= amount;
            var operation = new StockTradeOperation()
            {
                Id = id == -1 ? GetNextOperationId() : id,
                Date = date,
                OperationType = TradeOperationType.Buy,
                Qty = qty,
                StockName = stockName,
                Value = value,
                Fee = fee,
                Movement = -amount
            };
            this.TradeOperations.Add(operation);

            var position = this.OpenedPositions.FirstOrDefault(p => p.StockName == operation.StockName);
            if (position != null) // Position on this stock already exists, add new values
            {
                var openValue = (position.EntryValue * position.EntryQty + amount) / (position.EntryQty + operation.Qty);
                position.ExitDate = operation.Date;
                position.ExitValue = openValue;
                position = new StockPosition
                {
                    Id = operation.Id,
                    EntryDate = operation.Date,
                    EntryQty = position.EntryQty + operation.Qty,
                    StockName = operation.StockName,
                    EntryValue = openValue,
                    Stop = position.Stop,
                    TrailStop = position.TrailStop,
                    Theme = position.Theme,
                    BarDuration = position.BarDuration
                };
            }
            else // Position on this stock doen't exists, create a new one
            {
                position = new StockPosition
                {
                    Id = operation.Id,
                    EntryDate = operation.Date,
                    EntryQty = operation.Qty,
                    StockName = operation.StockName,
                    EntryValue = amount / qty,
                    Stop = stop,
                    TrailStop = stop,
                    BarDuration = barDuration,
                    EntryComment = entryComment,
                    Theme = theme
                };
            }

            this.Positions.Add(position);
        }
        public void SellTradeOperation(string stockName, DateTime date, int qty, float value, float fee, string exitComment, long id = -1)
        {
            var position = this.OpenedPositions.FirstOrDefault(p => p.StockName == stockName);
            if (position == null)
            {
                if (value == 0)
                    return;
                //throw new InvalidOperationException($"Selling not opened position: {stockName} qty:{qty}"); @@@@
                StockLog.Write($"Selling not opened position: {stockName} qty:{qty}");
                return;
            }
            var amount = value * qty - fee;
            var operation = new StockTradeOperation()
            {
                Id = id == -1 ? GetNextOperationId() : id,
                Date = date,
                OperationType = TradeOperationType.Sell,
                Qty = qty,
                StockName = stockName,
                Value = value,
                Fee = fee,
                Movement = amount
            };
            this.TradeOperations.Add(operation);
            this.Balance += amount;
            position.ExitDate = operation.Date;
            position.ExitValue = operation.Value;
            if (position.EntryQty != qty)
            {
                this.Positions.Add(new StockPosition
                {
                    Id = operation.Id,
                    StockName = operation.StockName,
                    EntryDate = operation.Date,
                    EntryQty = position.EntryQty - qty,
                    EntryValue = position.EntryValue,
                    Stop = position.Stop,
                    Theme = position.Theme,
                    BarDuration = position.BarDuration
                });
            }
        }
        public void TransferOperation(string stockName, DateTime date, int qty, float value, long id)
        {
            var operation = new StockTradeOperation()
            {
                Id = id == -1 ? GetNextOperationId() : id,
                Date = date,
                OperationType = TradeOperationType.Transfer,
                Qty = qty,
                StockName = stockName,
                Value = value,
                Movement = 0
            };
            this.TradeOperations.Add(operation);

            var position = this.OpenedPositions.FirstOrDefault(p => p.StockName == operation.StockName);
            if (position != null) // Position on this stock already exists, add new values
            {
                var openValue = (position.EntryValue * position.EntryQty + qty * value) / (position.EntryQty + operation.Qty);
                position.ExitDate = operation.Date;
                position.ExitValue = openValue;
                position = new StockPosition
                {
                    Id = operation.Id,
                    EntryDate = operation.Date,
                    EntryQty = position.EntryQty + operation.Qty,
                    StockName = operation.StockName,
                    EntryValue = openValue
                };
            }
            else // Position on this stock doen't exists, create a new one
            {
                position = new StockPosition
                {
                    Id = operation.Id,
                    EntryDate = operation.Date,
                    EntryQty = operation.Qty,
                    StockName = operation.StockName,
                    EntryValue = value
                };
            }
            this.Positions.Add(position);
        }
        public void CashOperation(DateTime date, float amount, long operationId)
        {
            var operation = new StockTradeOperation()
            {
                StockName = string.Empty,
                Id = operationId,
                Date = date,
                OperationType = TradeOperationType.Cash,
                Qty = 1,
                Value = amount,
                Movement = amount
            };
            this.TradeOperations.Add(operation);
            this.Balance += amount;
        }
        public void DividendOperation(string stockName, DateTime date, float amount, long operationId)
        {
            var operation = new StockTradeOperation()
            {
                Id = operationId,
                Date = date,
                OperationType = TradeOperationType.Dividend,
                Qty = 1,
                StockName = stockName,
                Value = amount,
                Movement = -amount
            };
            this.TradeOperations.Add(operation);
            this.Balance += amount;
        }
        public void AddOperation(StockOperation operation)
        {
            this.Balance = operation.Balance;
            if (operation.StockName.StartsWith("SRD "))
                return;
            switch (operation.OperationType.ToLower())
            {
                case StockOperation.DEPOSIT:
                    {
                        if (!operation.BinckName.StartsWith("DIVIDEND"))
                        {
                            this.Positions.Add(new StockPosition
                            {
                                EntryDate = operation.Date,
                                EntryQty = operation.Qty,
                                StockName = operation.StockName
                            });
                        }
                    }
                    break;
                case StockOperation.TRANSFER:
                    {
                        if (!operation.BinckName.StartsWith("DIVIDEND") && !operation.BinckName.StartsWith("DROITS"))
                        {
                            var qty = operation.Qty;
                            var stockName = operation.StockName;
                            var position = this.OpenedPositions.FirstOrDefault(p => p.StockName == stockName);
                            if (position != null)
                            {
                                position.ExitDate = operation.Date;
                                position.ExitValue = position.EntryValue;
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
                    }
                    break;
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
                        var position = this.OpenedPositions.FirstOrDefault(p => p.StockName == stockName);
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
                        var position = this.OpenedPositions.FirstOrDefault(p => p.StockName == stockName);
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
                    var pos = this.OpenedPositions.FirstOrDefault(p => p.StockName == trade.Serie.StockName);
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
                var pos = this.OpenedPositions.FirstOrDefault(p => p.StockName == trade.Serie.StockName);
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
            StockLog.Write($"Opened Positions: {this.OpenedPositions.Count()}");

            foreach (var p in this.OpenedPositions.OrderBy(p => p.StockName))
            {
                p.Dump();
            }
        }
        public void Dump(DateTime date)
        {
            StockLog.Write($"Dump for date:{date.ToShortDateString()}");
            StockLog.Write($"All Positions: {this.Positions.Count}");
            StockLog.Write($"Opened Positions: {this.OpenedPositions.Count()}");

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

        public List<StockOpenedOrder> OpenOrders { get; private set; }

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
        private StockSerie GetStockSerieFromUic(long uic)
        {
            var instrument = instrumentService.GetInstrumentById(uic);
            if (instrument == null)
            {
                StockLog.Write($"Instrument: {uic} not found !");
                return null;
            }
            // Find instrument in stock Dictionnary
            var symbol = instrument.Symbol.Split(':')[0];
            var stockName = instrument.Description.ToUpper().Trim();
            return StockDictionary.Instance.Values.FirstOrDefault(s => s.Symbol.Split('.')[0] == symbol || s.StockName == stockName);
        }

        Account account = null;
        AccountService accountService = new AccountService();
        InstrumentService instrumentService = new InstrumentService();
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

                // Check executedOrders (One day delay)
                var closedOrders = new OrderService().GetClosedOrders(account, this.LastSyncDate, DateTime.Today);
                if (closedOrders != null)
                {
                    foreach (var op in closedOrders.Data.OrderBy(o => o.TradeExecutionTime).ToList())
                    {
                        this.AddSaxoClosedOrder(op);
                    }
                }

                // Review opened Orders
                var openedOrders = new OrderService().GetOpenedOrders(account);
                foreach (var openedOrder in openedOrders.Data.Where(o => o.BuySell == "Buy"))
                {
                    long orderId = long.Parse(openedOrder.OrderId);
                    if (this.OpenOrders.Any(o => o.Id == orderId))
                        continue;
                    StockSerie stockSerie = GetStockSerieFromUic(openedOrder.Uic);
                    if (stockSerie == null)
                    {
                        MessageBox.Show($"Serie symbol: {openedOrder.Uic} not found !", "Porfolio sync error", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }
                    var instrument = instrumentService.GetInstrumentById(openedOrder.Uic);
                    StockLog.Write($"OrderId:{openedOrder.OrderId} {openedOrder.OpenOrderType} {instrument?.Symbol} {openedOrder.RelatedPositionId} {openedOrder.Status}");
                    var order = new StockOpenedOrder
                    {
                        Id = orderId,
                        StockName = stockSerie.StockName,
                        ISIN = stockSerie.ISIN,
                        Uic = openedOrder.Uic,
                        BuySell = openedOrder.BuySell,
                        OrderType = openedOrder.OpenOrderType,
                        Qty = (int)openedOrder.Amount,
                        Value = openedOrder.Price,
                        CreationDate = openedOrder.OrderTime,
                        Status = openedOrder.Status,
                    };
                    this.OpenOrders.Add(order);
                    var stopOrder = openedOrder.RelatedOpenOrders.FirstOrDefault(o => o.OpenOrderType.Contains("Stop"));
                    if (stopOrder != null)
                    {
                        order.StopValue = stopOrder.OrderPrice;
                    }
                }

                // Review opened Positions
                var saxoPositions = accountService.GetPositions(account).Where(p => p.PositionBase.CanBeClosed).OrderBy(p => p.PositionId).ToList();

                var saxoPositionsIds = saxoPositions.Select(p => long.Parse(p.PositionId));
                var untreatedPositions = this.OpenedPositions.Where(p => !saxoPositionsIds.Contains(p.Id)).ToList();
                this.PositionValue = 0;
                foreach (var saxoPosition in saxoPositions)
                {
                    var entryDate = new DateTime((saxoPosition.PositionBase.ExecutionTimeOpen.ToLocalTime().Ticks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond);

                    var instrument = instrumentService.GetInstrumentById(saxoPosition.PositionBase.Uic);
                    StockLog.Write($"{instrument.Symbol} PositionId: {saxoPosition.PositionId} OrderId: {saxoPosition.PositionBase.SourceOrderId} OpenDate:{saxoPosition.PositionBase.ExecutionTimeOpen}");
                    var posId = long.Parse(saxoPosition.PositionId);
                    var position = this.OpenedPositions.FirstOrDefault(p => p.Id == posId);
                    if (position == null)
                    {
                        StockSerie stockSerie = GetStockSerieFromUic(saxoPosition.PositionBase.Uic);
                        if (stockSerie == null)
                        {
                            MessageBox.Show($"Serie symbol: {saxoPosition.PositionBase.Uic} not found !", "Porfolio sync error", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;
                        }
                        position = new StockPosition()
                        {
                            Id = posId,
                            EntryQty = (int)saxoPosition.PositionBase.Amount,
                            EntryDate = entryDate,
                            EntryValue = saxoPosition.PositionBase.OpenPrice,
                            ISIN = stockSerie.ISIN,
                            Uic = saxoPosition.PositionBase.Uic,
                            StockName = stockSerie.StockName,
                            BarDuration = stockSerie.StockGroup == StockSerie.Groups.INTRADAY ? BarDuration.H_1 : BarDuration.Daily,
                        };
                        this.Positions.Add(position);

                        // Check if in the OpenedOrers
                        var openedOrder = this.OpenOrders.FirstOrDefault(o => o.Id == long.Parse(saxoPosition.PositionBase.SourceOrderId));
                        if (openedOrder != null)
                        {
                            position.BarDuration = openedOrder.BarDuration;
                            position.EntryComment = openedOrder.EntryComment;
                            position.Theme = openedOrder.Theme;
                            this.OpenOrders.Remove(openedOrder);
                        }
                    }
                    else
                    {
                        untreatedPositions.Remove(position);
                    }
                    // Update trailing stop
                    var stopOrder = openedOrders.Data.Where(o => o.Uic == saxoPosition.PositionBase.Uic && o.OpenOrderType == "StopIfTraded").FirstOrDefault();
                    if (stopOrder != null)
                    {
                        if (position.Stop == 0)
                        {
                            position.Stop = stopOrder.Price;
                        }
                        position.TrailStop = stopOrder.Price;
                        position.TrailStopId = stopOrder.OrderId;
                    }
                    else
                    {
                        position.Stop = 0;
                        position.TrailStop = 0;
                        position.TrailStopId = null;
                    }
                    position.EntryQty = (int)saxoPosition.PositionBase.Amount;
                }

                if (untreatedPositions.Count > 0 && !this.IsSaxoSimu)
                {
                    var closedPositions = accountService.GetClosedPositions(account)?.Data;
                    if (closedPositions != null)
                    {
                        foreach (var closedPosition in closedPositions)
                        {
                            var position = untreatedPositions.FirstOrDefault(p => p.Id == closedPosition.ClosedPosition.OpeningPositionId);
                            if (position == null) { continue; }
                            position.ExitDate = closedPosition.ClosedPosition.ExecutionTimeClose.ToLocalTime();
                            position.ExitValue = closedPosition.ClosedPosition.ClosingPrice;
                            untreatedPositions.Remove(position);
                        }
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
        public void AddSaxoClosedOrder(ClosedOrder closedOrder)
        {
            var orderId = long.Parse(closedOrder.OrderId);
            if (this.TradeOperations.Any(o => o.Id == orderId))
                return;
            var qty = Math.Abs((int)closedOrder.Amount);
            var stockSerie = GetStockSerieFromUic(closedOrder.Uic);
            string stockName = closedOrder.InstrumentSymbol;
            string isin = string.Empty;
            var tradeId = long.Parse(closedOrder.TradeId);
            var executionTime = new DateTime((closedOrder.TradeExecutionTime.ToLocalTime().Ticks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond);
            if (stockSerie == null)
            {
                StockLog.Write($"Serie symbol: {closedOrder.Uic} {closedOrder.InstrumentSymbol} not found !");
            }
            else
            {
                stockName = stockSerie.StockName;
                isin = stockSerie.ISIN;
            }
            StockLog.Write($"{closedOrder.TradeEventType} {stockName} Qty:{closedOrder.Amount} Id:{closedOrder.OrderId} TradeId:{closedOrder.TradeId} {executionTime}");
            switch (closedOrder.TradeEventType.ToLower())
            {
                case SaxoOperation.TRANSFER_IN:
                case SaxoOperation.BUY:
                    {
                        var tradeOperation = new StockTradeOperation
                        {
                            OperationType = TradeOperationType.Buy,
                            Date = executionTime,
                            Id = orderId,
                            Uic = closedOrder.Uic,
                            TradeId = tradeId,
                            Qty = qty,
                            StockName = stockName,
                            Value = closedOrder.Price,
                            ISIN = isin,
                            Fee = Math.Abs(closedOrder.BookedAmountClientCurrency - closedOrder.TradedValue)
                        };
                        this.TradeOperations.Add(tradeOperation);
                        var position = this.OpenedPositions.FirstOrDefault(p => p.ISIN == tradeOperation.ISIN);
                        if (position != null) // Position on this stock already exists, add new values
                        {
                            if (position.EntryDate == executionTime)
                                return;
                            var openValue = (position.EntryValue * position.EntryQty + closedOrder.Price * qty) / (position.EntryQty + qty);
                            position.ExitDate = executionTime;
                            position.ExitValue = openValue;
                            this.Positions.Add(new StockPosition
                            {
                                Id = position.Id,
                                Uic = position.Uic,
                                EntryDate = executionTime,
                                EntryQty = position.EntryQty + qty,
                                StockName = tradeOperation.StockName,
                                ISIN = tradeOperation.ISIN,
                                EntryValue = openValue
                            });
                        }
                        else // Position on this stock doen't exists, create a new one
                        {
                            position = new StockPosition
                            {
                                Id = orderId,
                                Uic = closedOrder.Uic,
                                EntryDate = executionTime,
                                EntryQty = qty,
                                StockName = tradeOperation.StockName,
                                ISIN = tradeOperation.ISIN,
                                EntryValue = closedOrder.Price
                            };
                            this.Positions.Add(position);
                            var openedOrder = this.OpenOrders.FirstOrDefault(o => o.Uic == closedOrder.Uic);
                            if (openedOrder != null)
                            {
                                position.BarDuration = openedOrder.BarDuration;
                                position.Theme = openedOrder.Theme;
                                position.EntryComment = openedOrder.EntryComment;
                                this.OpenOrders.Remove(openedOrder);
                            }
                        }
                    }
                    break;
                case SaxoOperation.SELL:
                    {
                        var tradeOperation = new StockTradeOperation
                        {
                            OperationType = TradeOperationType.Sell,
                            Date = executionTime,
                            Uic = closedOrder.Uic,
                            Id = orderId,
                            TradeId = tradeId,
                            Qty = qty,
                            StockName = stockName,
                            Value = closedOrder.Price,
                            ISIN = isin,
                            Fee = Math.Abs(closedOrder.BookedAmountClientCurrency - closedOrder.TradedValue)
                        };
                        var position = this.OpenedPositions.FirstOrDefault(p => p.ISIN == tradeOperation.ISIN);
                        if (position != null)
                        {
                            position.ExitDate = executionTime;
                            position.ExitValue = closedOrder.Price;
                            if (position.EntryQty != qty)
                            {
                                this.Positions.Add(new StockPosition
                                {
                                    Id = position.Id,
                                    Uic = position.Uic,
                                    EntryDate = executionTime,
                                    EntryQty = position.EntryQty - qty,
                                    StockName = tradeOperation.StockName,
                                    ISIN = tradeOperation.ISIN,
                                    EntryValue = position.EntryValue
                                });
                            }
                        }
                        else
                        {
                            StockLog.Write($"Selling not opened position: {tradeOperation.StockName} qty:{qty}");
                        }
                        this.TradeOperations.Add(tradeOperation);
                    }
                    break;
            }
        }
        public string SaxoBuyOrder(StockSerie stockSerie, OrderType orderType, int qty, float stopValue = 0, float orderValue = 0)
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
                decimal stop = instrumentDetail.RoundToTickSize(stopValue);

                var orderService = new OrderService();
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
                }
                return orderResponse?.OrderId;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
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

                var orderService = new OrderService();
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

                var orderService = new OrderService();
                decimal value = instrumentDetail.RoundToTickSize(exitValue);
                OrderResponse orderResponse = null;
                if (string.IsNullOrEmpty(position.TrailStopId))
                {
                    orderResponse = orderService.SellStopOrder(account, instrument, position.EntryQty, value);
                    position.TrailStopId = orderResponse.OrderId;
                }
                else
                {
                    orderResponse = orderService.PatchOrder(account, instrument, position.TrailStopId, SaxoOrderType.StopIfTraded, "Sell", position.EntryQty, value);
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