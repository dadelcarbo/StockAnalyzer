using Saxo.OpenAPI.AuthenticationServices;
using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockAgent;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using StockAnalyzerSettings.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace StockAnalyzer.StockPortfolio
{
    public class StockPortfolio
    {
        public const string SIMU_P = "Simu_P";
        public const string REPLAY_P = "Replay_P";

        const string PORTFOLIO_FILE_EXT = ".ucptf";
        const string SAXOPORTFOLIO_FILE_EXT = ".xlsx";

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

        #region SAXO Sync
        public void SaxoSync()
        {
            // new AccountService

        }
        #endregion

        public StockPortfolio()
        {
            this.TradeOperations = new List<StockTradeOperation>();
            this.Positions = new List<StockPosition>();
            this.MaxRisk = 0.02f;
        }
        public List<StockTradeOperation> TradeOperations { get; set; }
        public List<StockPosition> Positions { get; }
        public IEnumerable<StockPosition> SummaryPositions
        {
            get
            {
                return this.Positions.Where(p => !p.IsClosed);
            }
        }
        public IEnumerable<StockPosition> OpenedPositions => Positions.Where(p => !p.IsClosed);
        public string Name { get; set; }
        public string SaxoAccountId { get; set; }
        public string SaxoClientId { get; set; }
        public DateTime LastSyncDate { get; set; }
        public float InitialBalance { get; set; }
        public float Balance { get; set; }
        public float MaxRisk { get; set; }
        public DateTime CreationDate { get; set; }
        [XmlIgnore]
        public float PositionValue { get; set; }
        public float TotalValue => this.Balance + this.PositionValue;
        public float Return => (TotalValue - InitialBalance) / InitialBalance;
        public bool IsSimu { get; set; }
        public bool IsSaxoSimu { get; set; }

        #region PERSISTENCY
        public void Serialize()
        {
            string filepath = Path.Combine(Folders.Portfolio, this.Name + PORTFOLIO_FILE_EXT);
            using (FileStream fs = new FileStream(filepath, FileMode.Create))
            {
                System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings
                {
                    Indent = true,
                    NewLineOnAttributes = true
                };
                var xmlWriter = System.Xml.XmlWriter.Create(fs, settings);
                var serializer = new XmlSerializer(this.GetType());
                serializer.Serialize(xmlWriter, this);
            }
        }
        public static StockPortfolio Deserialize(string filepath)
        {
            using (FileStream fs = new FileStream(filepath, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(StockPortfolio));
                return serializer.Deserialize(fs) as StockPortfolio;
            }
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
                // Load from SAXO export
                var processedFolder = Path.Combine(Folders.Portfolio, "Processed");
                foreach (var file in Directory.EnumerateFiles(folder, "Transactions_*" + SAXOPORTFOLIO_FILE_EXT).OrderBy(s => s))
                {
                    LoadFromSAXO(file);
                    File.Move(file, Path.Combine(processedFolder, Path.GetFileName(file)));
                }
                var downloadFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                foreach (var file in Directory.EnumerateFiles(downloadFolder, "Transactions_*" + SAXOPORTFOLIO_FILE_EXT).OrderBy(s => s))
                {
                    LoadFromSAXO(file);
                    File.Move(file, Path.Combine(processedFolder, Path.GetFileName(file)));
                }

                // Load Saxo Operations.
                foreach (var file in Directory.EnumerateFiles(downloadFolder, "TradesExecuted_*" + SAXOPORTFOLIO_FILE_EXT).OrderBy(s => s))
                {
                    var operations = SaxoOperation.LoadFromFile(file);
                    if (operations == null)
                        continue;
                    foreach (var opGroup in operations.GroupBy(o => o.AccountId))
                    {
                        var portfolio = StockPortfolio.Portfolios.FirstOrDefault(p => p.SaxoAccountId == opGroup.Key);
                        if (portfolio == null)
                            continue;
                        foreach (var op in opGroup)
                        {
                            portfolio.AddOperation(op);
                        }
                    }
                    var destFile = Path.Combine(processedFolder, Path.GetFileName(file));
                    if (File.Exists(destFile))
                        File.Delete(destFile);
                    File.Move(file, destFile);
                }

                // Save SAXO Portfolio
                StockPortfolio.Portfolios.ForEach(p => p.Serialize());

                // Add simulation portfolio
                SimulationPortfolio = new StockPortfolio() { Name = SIMU_P, InitialBalance = 10000, IsSimu = true };
                StockPortfolio.Portfolios.Add(SimulationPortfolio);
                ReplayPortfolio = new StockPortfolio() { Name = REPLAY_P, InitialBalance = 10000, IsSimu = true };
                StockPortfolio.Portfolios.Add(ReplayPortfolio);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error loading portfolio file", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return StockPortfolio.Portfolios.OrderBy(p => p.Name).ToList();
        }

        private static void LoadFromSAXO(string fileName)
        {
            try
            {
                StockLog.Write("----------------------------------------------------------------------------");
                StockLog.Write($"Loading saxo report {fileName}");
                var connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0; data source={fileName};Extended Properties=""Excel 12.0"";";

                var adapter = new OleDbDataAdapter("SELECT * FROM [Transactions$]", connectionString);
                var ds = new DataSet();

                adapter.Fill(ds, "Transactions");

                var data = ds.Tables["Transactions"].AsEnumerable();

                foreach (var tradeGroup in data.Select(r => new
                {
                    TradeDate = DateTime.Parse(r.Field<string>(0)),
                    AccountId = r.Field<string>(1),
                    Instrument = r.Field<string>(3),
                    Type = r.Field<string>(5),
                    Event = r.Field<string>(6),
                    OpId = r.Field<object>(7),
                    TradeId = r.Field<object>(8),
                    Row = r
                }).Reverse().GroupBy(r => r.AccountId))
                {
                    var portfolio = Portfolios.FirstOrDefault(p => p.SaxoAccountId == tradeGroup.Key);
                    if (portfolio == null)
                        continue;
                    StockLog.Write($" ----------------------------- Processing Portfolio {portfolio.Name}");
                    foreach (var row in tradeGroup)
                    {
                        try
                        {
                            if (!long.TryParse(row.TradeId?.ToString(), out long tradeId))
                            {
                                tradeId = row.TradeDate.Year * 10000 + row.TradeDate.Month * 100 + row.TradeDate.Day;
                            }
                            if (portfolio.TradeOperations.Any(t => t.Id == tradeId))
                                continue;

                            StockLog.Write($"Processing : {row.TradeDate.ToShortDateString()}\t{row.Instrument}\t{row.Type}\t{row.Event}");

                            // Find stockName from mapping
                            var stockName = GetStockNameFromSaxo(row.Instrument);

                            switch (row.Type)
                            {
                                case "Liquidités":
                                    {
                                        portfolio.CashOperation(row.TradeDate, (float)row.Row.Field<double>(13), tradeId);
                                    }
                                    break;
                                case "Opération":
                                    {
                                        if (stockName == null || stockName.EndsWith(" Assented Rights"))
                                            continue;
                                        if (row.Row.ItemArray[10].GetType() == typeof(DBNull))
                                            continue;
                                        var price = float.Parse(row.Row.ItemArray[10].ToString().Replace(",", "."));
                                        var qty = (int)row.Row.Field<double>(9);
                                        var amount = (float)row.Row.Field<double>(13);
                                        switch (row.Event)
                                        {
                                            case "Achat":
                                            case "Acheter":
                                            case "Buy":
                                                if (amount == 0)
                                                    continue;
                                                portfolio.BuyTradeOperation(stockName, row.TradeDate, qty, price, -amount - (qty * price), 0, null, BarDuration.Daily, null, tradeId);
                                                break;
                                            case "Sell":
                                            case "Vendre":
                                            case "Vente":
                                                portfolio.SellTradeOperation(stockName, row.TradeDate, -qty, price, -(qty * price) - amount, null, tradeId);
                                                break;
                                            case "Transfert entrant":
                                                portfolio.TransferOperation(stockName, row.TradeDate, qty, price, tradeId);
                                                break;
                                            default:
                                                StockLog.Write($"Not processed:  {row.TradeDate}\t{row.Instrument}\t{row.Type}\t{row.Event}");
                                                System.Windows.Forms.MessageBox.Show($"Not processed:  {row.TradeDate}\t{row.Instrument}\t{row.Type}\t{row.Event}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                break;
                                        }
                                    }
                                    break;
                                case "Opération sur titres":
                                    switch (row.Event)
                                    {
                                        case "Dividende en espèces":
                                            {
                                                var amount = (float)row.Row.Field<double>(13);
                                                portfolio.DividendOperation(stockName, row.TradeDate, amount, tradeId);
                                                break;
                                            }
                                        default:
                                            StockLog.Write($"Not processed:  {row.TradeDate}\t{row.Instrument}\t{row.Type}\t{row.Event}");
                                            break;
                                    }
                                    break;
                                default:
                                    StockLog.Write($"Not processed:  {row.TradeDate}\t{row.Instrument}\t{row.Type}\t{row.Event}");
                                    System.Windows.Forms.MessageBox.Show($"Not processed:  {row.TradeDate}\t{row.Instrument}\t{row.Type}\t{row.Event}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    break;
                            }

                            //StockLog.Write($"Portfolio balance : {portfolio.Balance}");
                        }
                        catch (Exception ex)
                        {
                            StockLog.Write(ex);
                        }
                    }
                }
                return;
            }
            catch (Exception e)
            {
                StockLog.Write(e);
            }
        }
        #endregion

        public int MaxPositions { get; set; } = 10;

        #region OPERATION MANAGEMENT
        public long GetNextOperationId()
        {
            return this.TradeOperations.Count == 0 ? 0 : this.TradeOperations.Max(o => o.Id) + 1;
        }
        public void BuyTradeOperation(string stockName, DateTime date, int qty, float value, float fee, float stop, string entryComment, StockBarDuration barDuration, string theme, long id = -1)
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
        public void AddClosedOrder(ClosedOrder closedOrder)
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
            StockLog.Write($"{closedOrder.TradeEventType} {stockName} Id:{closedOrder.OrderId} TradeId:{closedOrder.TradeId} {executionTime}");
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
                                Id = 0,
                                OrderId = tradeId,
                                EntryDate = executionTime,
                                EntryQty = position.EntryQty + qty,
                                StockName = tradeOperation.StockName,
                                ISIN = tradeOperation.ISIN,
                                EntryValue = openValue
                            });
                        }
                        else // Position on this stock doen't exists, create a new one
                        {
                            this.Positions.Add(new StockPosition
                            {
                                Id = tradeId,
                                OrderId = tradeId,
                                EntryDate = executionTime,
                                EntryQty = qty,
                                StockName = tradeOperation.StockName,
                                ISIN = tradeOperation.ISIN,
                                EntryValue = closedOrder.Price
                            });
                        }
                    }
                    break;
                case SaxoOperation.SELL:
                    {
                        var tradeOperation = new StockTradeOperation
                        {
                            OperationType = TradeOperationType.Sell,
                            Date = executionTime,
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
                                    Id = 0,
                                    OrderId = tradeId,
                                    EntryDate = executionTime,
                                    EntryQty = position.EntryQty - qty,
                                    StockName = tradeOperation.StockName,
                                    ISIN = tradeOperation.ISIN,
                                    EntryValue = position.EntryValue
                                });
                            }

                            this.TradeOperations.Add(tradeOperation);
                        }
                        else
                        {
                            StockLog.Write($"Selling not opened position: {tradeOperation.StockName} qty:{qty}");
                        }
                    }
                    break;
            }
        }

        public void AddOperation(SaxoOperation operation)
        {
            if (this.TradeOperations.Any(o => o.Id == operation.Id))
                return;
            StockLog.Write($"Id:{operation.Id} {operation.OperationType} {operation.Instrument}");
            switch (operation.OperationType.ToLower())
            {
                case SaxoOperation.TRANSFER_IN:
                case SaxoOperation.BUY:
                    {
                        this.Balance += operation.GrossAmount;
                        var tradeOperation = new StockTradeOperation
                        {
                            OperationType = TradeOperationType.Buy,
                            Date = operation.Date,
                            Id = operation.Id,
                            Qty = operation.Qty,
                            StockName = operation.GetStockName(),
                            Value = operation.Value,
                            ISIN = operation.GetISIN(),
                            Fee = Math.Abs(operation.GrossAmount - operation.NetAmount)
                        };
                        this.TradeOperations.Add(tradeOperation);
                        var position = this.OpenedPositions.FirstOrDefault(p => p.ISIN == tradeOperation.ISIN);
                        if (position != null) // Position on this stock already exists, add new values
                        {
                            var openValue = (position.EntryValue * position.EntryQty + operation.Value * operation.Qty) / (position.EntryQty + operation.Qty);

                            position.ExitDate = operation.Date;
                            position.ExitValue = openValue;
                            this.Positions.Add(new StockPosition
                            {
                                EntryDate = operation.Date,
                                EntryQty = position.EntryQty + operation.Qty,
                                StockName = tradeOperation.StockName,
                                ISIN = tradeOperation.ISIN,
                                EntryValue = openValue
                            });
                        }
                        else // Position on this stock doen't exists, create a new one
                        {
                            this.Positions.Add(new StockPosition
                            {
                                EntryDate = operation.Date,
                                EntryQty = operation.Qty,
                                StockName = tradeOperation.StockName,
                                ISIN = tradeOperation.ISIN,
                                EntryValue = operation.Value
                            });
                        }
                    }
                    break;
                case SaxoOperation.SELL:
                    {
                        this.Balance += operation.GrossAmount;
                        var tradeOperation = new StockTradeOperation
                        {
                            OperationType = TradeOperationType.Sell,
                            Date = operation.Date,
                            Id = operation.Id,
                            Qty = operation.Qty,
                            StockName = operation.GetStockName(),
                            Value = operation.Value,
                            ISIN = operation.GetISIN(),
                            Fee = Math.Abs(operation.GrossAmount - operation.NetAmount)
                        };
                        var qty = operation.Qty;
                        var position = this.OpenedPositions.FirstOrDefault(p => p.ISIN == tradeOperation.ISIN);
                        if (position != null)
                        {
                            position.ExitDate = operation.Date;
                            position.ExitValue = operation.Value;
                            if (position.EntryQty != qty)
                            {
                                this.Positions.Add(new StockPosition
                                {
                                    EntryDate = operation.Date,
                                    EntryQty = position.EntryQty - qty,
                                    StockName = tradeOperation.StockName,
                                    ISIN = tradeOperation.ISIN,
                                    EntryValue = position.EntryValue
                                });
                            }

                            this.TradeOperations.Add(tradeOperation);
                        }
                        else
                        {
                            StockLog.Write($"Selling not opened position: {tradeOperation.StockName} qty:{qty}");
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

        private static string GetStockNameFromSaxo(string saxoName)
        {
            if (saxoName == null) return null;

            // Find stockName from mapping
            var stockName = saxoName.ToUpper()
                .Replace(" SA", "")
                .Replace(" S A", "")
                .Replace(" UCITS ETF", "")
                .Replace(" SE", "")
                .Replace(" NV", "")
                .Replace(" DAILY", "")
                .Replace("-", " ")
                .Replace("  ", " ");

            var mapping = StockPortfolio.GetMapping(stockName, null);
            if (mapping != null)
            {
                stockName = mapping.StockName;
            }
            return stockName;
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

        private StockSerie GetStockSerieFromUic(long uic)
        {
            var instrument = new InstrumentService().GetInstrumentById(uic);
            if (instrument == null)
            {
                StockLog.Write($"Instrument: {uic} not found !");
                return null;
            }
            // Find instrument in stock Dictionnary
            var symbol = instrument.Symbol.Split(':')[0];
            var stockName = instrument.Description.ToUpper();
            return StockDictionary.Instance.Values.FirstOrDefault(s => s.Symbol == symbol || s.StockName == stockName);
        }

        public void Refresh()
        {
            if (string.IsNullOrEmpty(this.SaxoClientId))
            {
                Task.Delay(500).Wait();
                return;
            }

            try
            {
                LoginService.Login(this.SaxoClientId, Folders.Portfolio, this.IsSaxoSimu);
                var accountService = new AccountService();
                var account = accountService.GetAccounts()?.FirstOrDefault(a => a.AccountId == this.SaxoAccountId);
                if (account == null)
                {
                    MessageBox.Show($"Account: {this.SaxoAccountId} not found !", "Porfolio sync error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Update portfolio balance
                var balance = accountService.GetBalance(account);
                if (balance != null)
                {
                    this.Balance = balance.CashAvailableForTrading;
                }

                //{
                //    var instrument = new InstrumentService().GetInstrumentByIsin("FR0000120073");
                //    var orderService = new OrderService();
                //    var orderResponse = orderService.BuyMarketOrder(account, instrument, 100, 25);
                //    Console.WriteLine(orderResponse.OrderId);
                //    foreach (var o in orderResponse.Orders)
                //    {
                //        Console.WriteLine(o.OrderId);
                //    }
                //}
                // Review opened Orders
                var openedOrders = new OrderService().GetOpenedOrders(account);
                foreach (var openedOrder in openedOrders.Data)
                {
                    var instrument = new InstrumentService().GetInstrumentById(openedOrder.Uic);
                    Console.WriteLine($"OrderId:{openedOrder.OrderId} {openedOrder.OpenOrderType} {instrument?.Symbol} {openedOrder.RelatedPositionId} {openedOrder.Status}");
                }

                // Review opened Positions
                var saxoPositions = accountService.GetPositions(account).Where(p => p.PositionBase.CanBeClosed).OrderBy(p => p.PositionId).ToList();

                var saxoPositionsIds = saxoPositions.Select(p => long.Parse(p.PositionId));
                var untreatedPositions = this.OpenedPositions.Where(p => !saxoPositionsIds.Contains(p.Id)).ToList();
                this.PositionValue = 0;
                foreach (var saxoPosition in saxoPositions)
                {
                    var instrument = new InstrumentService().GetInstrumentById(saxoPosition.PositionBase.Uic);
                    Console.WriteLine($"{instrument.Symbol} PositionId: {saxoPosition.PositionId} OrderId: {saxoPosition.PositionBase.SourceOrderId} OpenDate:{saxoPosition.PositionBase.ExecutionTimeOpen}");
                    var posId = long.Parse(saxoPosition.PositionId);
                    var orderId = long.Parse(saxoPosition.PositionBase.SourceOrderId);
                    var position = this.OpenedPositions.FirstOrDefault(p => p.Id == posId || p.OrderId == orderId);
                    if (position == null)
                    {
                        StockSerie stockSerie = GetStockSerieFromUic(saxoPosition.PositionBase.Uic);
                        if (stockSerie == null)
                        {
                            MessageBox.Show($"Serie symbol: {saxoPosition.PositionBase.Uic} not found !", "Porfolio sync error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            continue;
                        }
                        stockSerie.Initialise();
                        var executionTime = new DateTime((saxoPosition.PositionBase.ExecutionTimeOpen.ToLocalTime().Ticks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond);

                        var newStockPosition = new StockPosition()
                        {
                            Id = posId,
                            EntryQty = (int)saxoPosition.PositionBase.Amount,
                            EntryDate = executionTime,
                            EntryValue = saxoPosition.PositionBase.OpenPrice,
                            ISIN = stockSerie.ISIN,
                            Uic = saxoPosition.PositionBase.Uic,
                            StockName = stockSerie.StockName,
                            BarDuration = stockSerie.StockGroup == StockSerie.Groups.INTRADAY ? BarDuration.H_1 : BarDuration.Daily,
                        };
                        this.Positions.Add(newStockPosition);

                        // Find Stop orders
                        var stopOrder = openedOrders.Data.Where(o => o.Uic == saxoPosition.PositionBase.Uic && o.OpenOrderType == "StopIfTraded").FirstOrDefault();
                        if (stopOrder != null)
                        {
                            newStockPosition.Stop = stopOrder.Price;
                            newStockPosition.TrailStop = stopOrder.Price;
                            newStockPosition.TrailStopId = stopOrder.OrderId;
                        }

                        this.PositionValue += saxoPosition.PositionView.MarketValue != 0 ? saxoPosition.PositionView.MarketValue : stockSerie.LastValue.CLOSE * saxoPosition.PositionBase.Amount;
                    }
                    else
                    {
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
                    }
                }

                // Check executedOrders (One day dalay)
                var closedOrders = new OrderService().GetClosedOrders(account, this.LastSyncDate.AddDays(-1), DateTime.Now);
                foreach (var op in closedOrders.Data.OrderBy(o => o.TradeExecutionTime))
                {
                    this.AddClosedOrder(op);
                }
                foreach (var pos in accountService.GetHistoricalClosedPositions(account, LastSyncDate).Data)
                {
                    Console.WriteLine($"{pos.InstrumentSymbol} OpenPosId: {pos.OpenPositionId} ClosePosId:{pos.ClosePositionId} {pos.TradeDateOpen}");
                }

                this.LastSyncDate = DateTime.Today;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Porfolio sync error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}