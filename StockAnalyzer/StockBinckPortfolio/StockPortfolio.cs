using StockAnalyzer.StockAgent;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace StockAnalyzer.StockBinckPortfolio
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

        public StockPortfolio()
        {
            this.TradeOperations = new List<StockTradeOperation>();
            this.Positions = new List<StockPosition>();
            this.MaxRisk = 0.02f;
        }

        //[XmlIgnore]
        // public List<StockOperation> Operations { get; }
        public List<StockTradeOperation> TradeOperations { get; set; }
        public List<StockPosition> Positions { get; }
        public IEnumerable<StockPosition> OpenedPositions => Positions.Where(p => !p.IsClosed);
        public string Name { get; set; }
        public string SaxoAccountId { get; set; }
        public float InitialBalance { get; set; }
        public float Balance { get; set; }
        public float MaxRisk { get; set; }
        [XmlIgnore]
        public float PositionValue { get; private set; }
        public float TotalValue => this.Balance + this.PositionValue;
        public float Return => (TotalValue - InitialBalance) / InitialBalance;
        public bool IsSimu { get; set; }

        #region PERSISTENCY
        public void Serialize(string folder)
        {
            string filepath = Path.Combine(folder, this.Name + PORTFOLIO_FILE_EXT);
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
            LoadMappings();

            // Load Binck Portfolio from Operations(mouvements)
            StockPortfolio.Portfolios = new List<StockPortfolio>();
            foreach (var file in Directory.EnumerateFiles(folder, "*" + PORTFOLIO_FILE_EXT).OrderBy(s => s))
            {
                StockPortfolio.Portfolios.Add(StockPortfolio.Deserialize(file));
            }
            foreach (var file in Directory.EnumerateFiles(folder, "*" + SAXOPORTFOLIO_FILE_EXT).OrderBy(s => s))
            {
                LoadFromSAXO(file, folder);
            }

            // Add simulation portfolio
            SimulationPortfolio = new StockPortfolio() { Name = SIMU_P, InitialBalance = 10000, IsSimu = true };
            StockPortfolio.Portfolios.Add(SimulationPortfolio);
            ReplayPortfolio = new StockPortfolio() { Name = REPLAY_P, InitialBalance = 10000, IsSimu = true };
            StockPortfolio.Portfolios.Add(ReplayPortfolio);
            return StockPortfolio.Portfolios.OrderBy(p => p.Name).ToList();
        }

        private static void LoadFromSAXO(string fileName, string folder)
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

                foreach (var tradeGroup in data.Where(r => r.Field<string>(5) == "Opération sur titres").Select(r => new
                {
                    TradeDate = r.Field<DateTime>(0),
                    AccountId = r.Field<string>(1),
                    Instrument = r.Field<string>(3),
                    InstrumentId = r.Field<double>(4),
                    Event = r.Field<string>(6),
                    Type = r.Field<string>(5),
                    TradeId = long.Parse(r.Field<string>(7)),
                    Amount = r.Field<double>(13)
                }).GroupBy(r => r.AccountId))
                {
                    var portofolio = Portfolios.FirstOrDefault(p => p.SaxoAccountId == tradeGroup.Key);
                    if (portofolio == null)
                        continue;

                    foreach (var row in tradeGroup.OrderBy(t => t.TradeId))
                    {
                        if (portofolio.TradeOperations.Any(t => t.Id == row.TradeId))
                            continue;

                        // Find stockNamefrom mapping
                        var stockName = row.Instrument.ToUpper()
                            .Replace(" SA", "")
                            .Replace(" UCITS ETF", "")
                            .Replace(" DAILY", "");

                        if (row.Event == "Dividende en espèces")
                        {
                            portofolio.DividendOperation(stockName, row.TradeDate, (float)row.Amount, row.TradeId);
                        }
                    }
                }
                foreach (var tradeGroup in data.Where(r => r.Field<string>(5) == "Opération").Select(r => new
                {
                    TradeDate = r.Field<DateTime>(0),
                    AccountId = r.Field<string>(1),
                    Instrument = r.Field<string>(3),
                    InstrumentId = r.Field<double>(4),
                    Event = r.Field<string>(6),
                    Type = r.Field<string>(5),
                    TradeId = (int)r.Field<double>(8),
                    Qty = (int)r.Field<double>(9),
                    Price = r.Field<double?>(10),
                    Amount = r.Field<double>(13)
                }).GroupBy(r => r.AccountId))
                {
                    var portofolio = Portfolios.FirstOrDefault(p => p.SaxoAccountId == tradeGroup.Key);
                    if (portofolio == null)
                        continue;

                    StockLog.Write($" ----------------------------- Processing Portfolio {portofolio.Name}");
                    foreach (var row in tradeGroup.OrderBy(t => t.TradeId))
                    {
                        if (portofolio.TradeOperations.Any(t => t.Id == row.TradeId))
                            continue;

                        // Find stockNamefrom mapping
                        var stockName = row.Instrument.ToUpper()
                            .Replace(" SA", "")
                            .Replace(" UCITS ETF", "")
                            .Replace(" DAILY", "");

                        StockLog.Write($"Processing {row.Event} {stockName}");
                        try
                        {
                            if ((row.Event == "Buy" || row.Event == "Achat") && row.Price != null)
                            {
                                portofolio.BuyTradeOperation(stockName, row.TradeDate, row.Qty, (float)row.Price, (float)(-row.Amount - (row.Qty * row.Price)), 0, null, BarDuration.Daily, null, row.TradeId);
                            }
                            else if (row.Event == "Transfert entrant")
                            {
                                portofolio.TransferOperation(stockName, row.TradeDate, row.Qty, (float)row.Price, row.TradeId);
                            }
                            else if (row.Event == "Sell" || row.Event == "Vente")
                            {
                                if (row.Price == null)
                                {
                                    portofolio.SellTradeOperation(stockName, row.TradeDate, -row.Qty, 0f, 0f, null, row.TradeId);
                                }
                                else
                                {
                                    portofolio.SellTradeOperation(stockName, row.TradeDate, -row.Qty, (float)row.Price, (float)((-row.Qty * row.Price) - row.Amount), null, row.TradeId);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            //MessageBox.Show(e.Message);&
                            StockLog.Write(e);
                        }
                    }
                    portofolio.Serialize(folder);
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);&
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
        public void BuyTradeOperation(string stockName, DateTime date, int qty, float value, float fee, float stop, string entryComment, StockBarDuration barDuration, string indicator, long id = -1)
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
                Fee = fee
            };
            this.TradeOperations.Add(operation);

            var position = this.OpenedPositions.FirstOrDefault(p => p.StockName == operation.StockName);
            if (position != null) // Position on this stock already exists, add new values
            {
                position.ExitDate = operation.Date;

                var openValue = (position.EntryValue * position.EntryQty + amount) / (position.EntryQty + operation.Qty);
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
                    EntryValue = amount / qty,
                    Stop = stop,
                    BarDuration = barDuration,
                    EntryComment = entryComment,
                    Indicator = indicator
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
                throw new InvalidOperationException($"Selling not opened position: {stockName} qty:{qty}");
            }
            var operation = new StockTradeOperation()
            {
                Id = id == -1 ? GetNextOperationId() : id,
                Date = date,
                OperationType = TradeOperationType.Sell,
                Qty = qty,
                StockName = stockName,
                Value = value,
                Fee = fee,
            };
            this.TradeOperations.Add(operation);
            var amount = operation.Value * operation.Qty - operation.Fee;
            this.Balance += amount;
            var logEntry = this.Positions.Find(l => l.Id == position.Id);
            logEntry.ExitDate = date;
            position.ExitDate = operation.Date;
            if (position.EntryQty != qty)
            {
                this.Positions.Add(new StockPosition
                {
                    Id = operation.Id,
                    EntryDate = operation.Date,
                    EntryQty = position.EntryQty - qty,
                    StockName = operation.StockName,
                    EntryValue = position.EntryValue
                });
            }
        }
        public void TransferOperation(string stockName, DateTime date, int qty, float value, long id)
        {
            if (stockName.StartsWith("*"))
                return;
            var operation = new StockTradeOperation()
            {
                Id = id == -1 ? GetNextOperationId() : id,
                Date = date,
                OperationType = TradeOperationType.Transfer,
                Qty = qty,
                StockName = stockName,
                Value = value
            };
            this.TradeOperations.Add(operation);

            var position = new StockPosition
            {
                Id = operation.Id,
                EntryDate = operation.Date,
                EntryQty = operation.Qty,
                StockName = operation.StockName,
                EntryValue = value
            };
            this.Positions.Add(position);
        }
        public void DepositOperation(DateTime date, float value)
        {
            var operation = new StockTradeOperation()
            {
                Id = GetNextOperationId(),
                Date = date,
                OperationType = TradeOperationType.Transfer,
                Qty = 1,
                StockName = value > 0 ? "Deposit" : "Withdraw",
                Value = value
            };
            this.TradeOperations.Add(operation);
            this.Balance += value;
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
                Value = amount
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
                                Console.WriteLine($"Selling not opened position: {stockName} qty:{qty}");
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
                            position.ExitDate = operation.Date;

                            var openValue = (position.EntryValue * position.EntryQty - operation.Amount) / (position.EntryQty + qty);

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
                            Console.WriteLine($"Selling not opened position: {stockName} qty:{qty}");
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
                Union(trades.Where(t => t.PartialExitIndex >= 0).Select(t => t.PartialExitDate)).
                Distinct().OrderBy(d => d).ToList();

            int openedPosition = 0;
            foreach (var date in dates)
            {
                // Sell Partially closed trades
                foreach (var trade in trades.Where(t => t.PartialExitDate == date))
                {
                    var pos = this.OpenedPositions.FirstOrDefault(p => p.StockName == trade.Serie.StockName);
                    if (pos == null)
                        continue;
                    var partialQty = (pos.EntryQty / 2);
                    var amount = partialQty * trade.PartialExitValue;
                    this.Balance += amount;
                    var id = this.GetNextOperationId();
                    var exit = StockOperation.FromSimu(id, trade.PartialExitDate, trade.Serie.StockName, StockOperation.SELL, partialQty, amount, !trade.IsLong);
                    exit.Balance = this.Balance;
                    this.AddOperation(exit);
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

                // Buy begining trades
                foreach (var trade in trades.Where(t => t.EntryDate == date))
                {
                    if (openedPosition >= MaxPositions)
                        break;

                    var amountToInvest = this.Balance / (float)(MaxPositions - openedPosition);
                    var qty = (int)(amountToInvest / trade.EntryValue);
                    var amount = qty * trade.EntryValue;
                    this.Balance -= amount;

                    var id = this.GetNextOperationId();
                    var entry = StockOperation.FromSimu(id, trade.EntryDate, trade.Serie.StockName, StockOperation.BUY, qty, -amount, !trade.IsLong);
                    entry.Balance = this.Balance;
                    this.AddOperation(entry);
                    openedPosition++;
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
            Console.WriteLine($"All Positions: {this.Positions.Count}");
            Console.WriteLine($"Opened Positions: {this.OpenedPositions.Count()}");

            foreach (var p in this.OpenedPositions.OrderBy(p => p.StockName))
            {
                p.Dump();
            }
        }
        public void Dump(DateTime date)
        {
            Console.WriteLine($"Dump for date:{date.ToShortDateString()}");
            Console.WriteLine($"All Positions: {this.Positions.Count}");
            Console.WriteLine($"Opened Positions: {this.OpenedPositions.Count()}");

            foreach (var p in this.Positions.Where(p => p.EntryDate < date && p.ExitDate > date).OrderBy(p => p.StockName))
            {
                p.Dump();
            }
        }
        public float EvaluateAt(DateTime date, StockClasses.BarDuration duration, out long volume)
        {
            // Calculate value for opened positions
            var positions = this.Positions.Where(p => p.ExitDate > date && p.EntryDate <= date);
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

            // Calculate cash balance
            StockOperation lastOperation = null;
            //foreach (var op in this.Operations)
            //{
            //    if (op.Date <= date)
            //    {
            //        lastOperation = op;
            //    }
            //    else
            //    {
            //        break;
            //    }
            //}
            positionValue += lastOperation == null ? this.InitialBalance : lastOperation.Balance;

            return positionValue;
        }


        #region Binck Name Mapping
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
            string fileName = Path.Combine(StockAnalyzerSettings.Properties.Settings.Default.RootFolder, "Portfolio");
            fileName = Path.Combine(fileName, "NameMappings.xml");
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
            string fileName = Path.Combine(StockAnalyzerSettings.Properties.Settings.Default.RootFolder, "Portfolio");
            fileName = Path.Combine(fileName, "NameMappings.xml");
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

        public static StockNameMapping GetMapping(string binckName)
        {
            return Mappings.FirstOrDefault(m => binckName.Contains(m.BinckName.ToUpper()));
        }
        #endregion

        public override string ToString()
        {
            return this.Name;
        }

    }
}