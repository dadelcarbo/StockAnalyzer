using StockAnalyzer.StockAgent;
using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace StockAnalyzer.StockBinckPortfolio
{
    public class StockPortfolio
    {
        public const string SIMU_P = "Simu_P";
        public const string REPLAY_P = "Replay_P";

        const string PORTFOLIO_FILE_EXT = ".ucptf";
        const string SIMU_PORTFOLIO_FILE_EXT = ".tptf";
        const string BINCK_PORTFOLIO_FILE_EXT = ".ptf";

        public static StockPortfolio SimulationPortfolio { get; private set; }
        public static StockPortfolio ReplayPortfolio { get; private set; }

        public static IStockPriceProvider PriceProvider { get; set; }

        public static List<StockPortfolio> Portfolios { get; private set; }

        public static List<StockPortfolio> LoadPortfolios(string folder)
        {
            LoadMappings();
            // Load Binck Portfolio from Operations(mouvements)
            StockPortfolio.Portfolios = new List<StockPortfolio>();
            foreach (var file in Directory.EnumerateFiles(folder, "*" + BINCK_PORTFOLIO_FILE_EXT).OrderBy(s => s))
            {
                StockPortfolio.Portfolios.Add(new StockPortfolio(file));
            }
            foreach (var file in Directory.EnumerateFiles(folder, "*" + SIMU_PORTFOLIO_FILE_EXT).OrderBy(s => s))
            {
                StockPortfolio.Portfolios.Add(new StockPortfolio(file));
            }
            foreach (var file in Directory.EnumerateFiles(folder, "*" + PORTFOLIO_FILE_EXT).OrderBy(s => s))
            {
                StockPortfolio.Portfolios.Add(StockPortfolio.Deserialize(file));
            }
            // Add simulation portfolio
            SimulationPortfolio = new StockPortfolio() { Name = SIMU_P, InitialBalance = 10000, IsSimu = true };
            StockPortfolio.Portfolios.Add(SimulationPortfolio);
            ReplayPortfolio = new StockPortfolio() { Name = REPLAY_P, InitialBalance = 10000, IsSimu = true };
            StockPortfolio.Portfolios.Add(ReplayPortfolio);
            return StockPortfolio.Portfolios.OrderBy(p => p.Name).ToList();
        }
        public StockPortfolio()
        {
            this.TradeOperations = new List<StockTradeOperation>();
            this.Operations = new List<StockOperation>();
            this.Positions = new List<StockPosition>();
            this.LogEntries = new List<StockTradeLogEntry>();
        }

        static int instanceCount = 0;
        static public StockPortfolio CreateSimulationPortfolio()
        {
            return new StockPortfolio() { Name = SIMU_P + "_" + instanceCount++, InitialBalance = 10000, IsSimu = true };
        }

        public StockPortfolio(string fileName)
        {
            this.Name = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            this.IsSimu = ext == SIMU_PORTFOLIO_FILE_EXT;
            this.Operations = new List<StockOperation>();
            this.Positions = new List<StockPosition>();
            int id = 0;
            var lines = File.ReadAllLines(fileName, Encoding.GetEncoding(1252));
            bool hasId = lines[0].StartsWith("#ID");
            foreach (var l in lines.Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#")))
            {
                var operation = IsSimu ? StockOperation.FromSimuLine(hasId, id++, l) : StockOperation.FromBinckLine(l);
                this.AddOperation(operation);
            }
            this.Operations = this.Operations.OrderByDescending(o => o.Id).ToList();
        }
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

        public static int MaxPositions { get; set; } = 20;

        public void Save(string folder)
        {
            const string header = "#ID\tDATE\tTYPE\tNAME\tSHORT\tQTY\tAMOUNT\tBALANCE";
            string content = header;
            float balance = this.Balance;
            foreach (var operation in this.Operations.OrderBy(o => o.Id))
            {
                balance += operation.Amount;
                content += Environment.NewLine + operation.ToFileString() + "\t" + balance;
            }
            File.WriteAllText(Path.Combine(folder, this.Name + SIMU_PORTFOLIO_FILE_EXT), content);
        }
        public int GetNextOperationId()
        {
            return this.TradeOperations.Count == 0 ? 0 : this.TradeOperations.Max(o => o.Id) + 1;
        }
        public void BuyTradeOperation(string stockName, DateTime date, int qty, float Value, float fee, float stop, string entryComment, StockBarDuration barDuration, string indicator)
        {
            var operation = new StockTradeOperation()
            {
                Id = GetNextOperationId(),
                Date = date,
                OperationType = TradeOperationType.Buy,
                Qty = qty,
                StockName = stockName,
                Value = Value,
                Fee = fee,
            };
            this.TradeOperations.Add(operation);
            var amount = operation.Value * operation.Qty + operation.Fee;
            this.Balance -= amount;
            var position = this.OpenedPositions.FirstOrDefault(p => p.StockName == operation.StockName);
            if (position != null) // Position on this stock already exists, add new values
            {
                position.EndDate = operation.Date;
                var logEntry = this.LogEntries.Find(l => l.Id == position.Id);

                var openValue = (position.OpenValue * position.Qty - amount) / (position.Qty + operation.Qty);
                position = new StockPosition
                {
                    StartDate = operation.Date,
                    Qty = position.Qty + operation.Qty,
                    StockName = operation.StockName,
                    OpenValue = openValue
                };
            }
            else // Position on this stock doen't exists, create a new one
            {
                position = new StockPosition
                {
                    Id = operation.Id,
                    StartDate = operation.Date,
                    Qty = operation.Qty,
                    StockName = operation.StockName,
                    OpenValue = operation.Value
                };
                var logEntry = new StockTradeLogEntry
                {
                    Id = operation.Id,
                    EntryDate = operation.Date,
                    EntryQty = operation.Qty,
                    StockName = operation.StockName,
                    EntryValue = operation.Value,
                    Stop = stop,
                    BarDuration = barDuration,
                    EntryComment = entryComment,
                    Indicator = indicator
                };
                this.LogEntries.Add(logEntry);
            }

            this.Positions.Add(position);
        }
        public void SellTradeOperation(string stockName, DateTime date, int qty, float Value, float fee, string exitComment)
        {
            var operation = new StockTradeOperation()
            {
                Id = GetNextOperationId(),
                Date = date,
                OperationType = TradeOperationType.Sell,
                Qty = qty,
                StockName = stockName,
                Value = Value,
                Fee = fee,
            };
            this.TradeOperations.Add(operation);
            var amount = operation.Value * operation.Qty - operation.Fee;
            this.Balance += amount;
            var position = this.OpenedPositions.FirstOrDefault(p => p.StockName == operation.StockName);
            if (position != null)
            {
                var logEntry = this.LogEntries.Find(l => l.Id == position.Id);
                logEntry.EndDate = date;
                position.EndDate = operation.Date;
                if (position.Qty != qty)
                {
                    this.Positions.Add(new StockPosition
                    {
                        StartDate = operation.Date,
                        Qty = position.Qty - qty,
                        StockName = operation.StockName,
                        OpenValue = position.OpenValue
                    });
                }
            }
            else
            {
                throw new InvalidOperationException($"Selling not opened position: {stockName} qty:{qty}");
            }
        }
        public void DepositTradeOperation(DateTime date, float value)
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
        public void DividendTradeOperation(string stockName, DateTime date, float value, int qty)
        {
            var operation = new StockTradeOperation()
            {
                Id = GetNextOperationId(),
                Date = date,
                OperationType = TradeOperationType.Dividend,
                Qty = qty,
                StockName = stockName,
                Value = value
            };
            this.TradeOperations.Add(operation);
            this.Balance += value * qty;
        }
        public void AddOperation(StockOperation operation)
        {
            //if (this.Operations.Any(op => op.Id == operation.Id))
            //    throw new InvalidOperationException($"Operation {operation.Id} already in portfolio");

            this.Operations.Add(operation);

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
                                StartDate = operation.Date,
                                Qty = operation.Qty,
                                StockName = operation.StockName,
                                Leverage = operation.NameMapping == null ? 1 : operation.NameMapping.Leverage
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
                                position.EndDate = operation.Date;
                                if (position.Qty != qty)
                                {
                                    this.Positions.Add(new StockPosition
                                    {
                                        StartDate = operation.Date,
                                        Qty = position.Qty - qty,
                                        StockName = operation.StockName,
                                        OpenValue = position.OpenValue,
                                        Leverage = operation.NameMapping == null ? 1 : operation.NameMapping.Leverage
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
                        var qty = operation.Qty;
                        var stockName = operation.StockName;
                        var position = this.OpenedPositions.FirstOrDefault(p => p.StockName == stockName);
                        if (position != null) // Position on this stock already exists, add new values
                        {
                            position.EndDate = operation.Date;

                            var openValue = (position.OpenValue * position.Qty - operation.Amount) / (position.Qty + qty);

                            this.Positions.Add(new StockPosition
                            {
                                StartDate = operation.Date,
                                Qty = position.Qty + qty,
                                StockName = stockName,
                                OpenValue = openValue,
                                IsShort = operation.IsShort,
                                Leverage = operation.NameMapping == null ? 1 : operation.NameMapping.Leverage
                            });
                        }
                        else // Position on this stock doen't exists, create a new one
                        {
                            this.Positions.Add(new StockPosition
                            {
                                StartDate = operation.Date,
                                Qty = qty,
                                StockName = stockName,
                                OpenValue = -operation.Amount / qty,
                                IsShort = operation.IsShort,
                                Leverage = operation.NameMapping == null ? 1 : operation.NameMapping.Leverage
                            });
                        }
                    }
                    break;
                case StockOperation.SELL:
                    {
                        var qty = operation.Qty;
                        var stockName = operation.StockName;
                        var position = this.OpenedPositions.FirstOrDefault(p => p.StockName == stockName);
                        if (position != null)
                        {
                            position.EndDate = operation.Date;
                            if (position.Qty != qty)
                            {
                                this.Positions.Add(new StockPosition
                                {
                                    StartDate = operation.Date,
                                    Qty = position.Qty - qty,
                                    StockName = operation.StockName,
                                    OpenValue = position.OpenValue,
                                    IsShort = operation.IsShort,
                                    Leverage = operation.NameMapping == null ? 1 : operation.NameMapping.Leverage
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
        /// <summary>
        /// Converts from Trade list to portfolio operations. This function is limited to the MaxPosition variable
        /// </summary>
        /// <param name="tradeSummary"></param>
        public void InitFromTradeSummary(List<StockTrade> trades)
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
                    var partialQty = (pos.Qty / 2);
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
                    var amount = pos.Qty * trade.ExitValue;
                    this.Balance += amount;
                    var id = this.GetNextOperationId();
                    var exit = StockOperation.FromSimu(id, trade.ExitDate, trade.Serie.StockName, StockOperation.SELL, pos.Qty, amount, !trade.IsLong);
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
                this.PositionValue += pos.Qty * trade.Serie.Values.Last().CLOSE;
            }
        }
        public void Clear()
        {
            this.Operations.Clear();
            this.Positions.Clear();
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

            foreach (var p in this.Positions.Where(p => p.StartDate < date && p.EndDate > date).OrderBy(p => p.StockName))
            {
                p.Dump();
            }
        }
        public float EvaluateAt(DateTime date, StockClasses.BarDuration duration, out long volume)
        {
            // Calculate value for opened positions
            var positions = this.Positions.Where(p => p.EndDate > date && p.StartDate <= date);
            float positionValue = 0f;
            volume = 0;
            foreach (var pos in positions)
            {
                volume++;
                float value = PriceProvider.GetClosingPrice(pos.StockName, date, duration);
                if (value == 0.0f)
                {
                    positionValue += pos.Qty * pos.OpenValue;
                }
                else
                {
                    positionValue += pos.Qty * value;
                }
            }

            // Calculate cash balance
            StockOperation lastOperation = null;
            foreach (var op in this.Operations)
            {
                if (op.Date <= date)
                {
                    lastOperation = op;
                }
                else
                {
                    break;
                }
            }
            positionValue += lastOperation == null ? this.InitialBalance : lastOperation.Balance;

            return positionValue;
        }
        [XmlIgnore]
        public List<StockOperation> Operations { get; }
        public List<StockTradeOperation> TradeOperations { get; }
        public List<StockTradeLogEntry> LogEntries { get; set; }
        public List<StockPosition> Positions { get; }
        public IEnumerable<StockPosition> OpenedPositions => Positions.Where(p => !p.IsClosed);
        public string Name { get; set; }
        public float InitialBalance { get; set; }
        public float Balance { get; set; }
        [XmlIgnore]
        public float PositionValue { get; private set; }
        public float TotalValue => this.Balance + this.PositionValue;
        public float Return => (TotalValue - InitialBalance) / InitialBalance;
        public bool IsSimu { get; set; }

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