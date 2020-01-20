using StockAnalyzer.StockClasses;
using StockAnalyzer.StockPortfolio;
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
        public static IStockPriceProvider PriceProvider { get; set; }

        public static List<StockPortfolio> Portfolios { get; private set; }

        public static List<StockPortfolio> LoadPortfolios(string folder)
        {
            LoadMappings();
            StockPortfolio.Portfolios = new List<StockPortfolio>();
            foreach (var file in Directory.EnumerateFiles(folder, "*.ptf").OrderBy(s => s))
            {
                StockPortfolio.Portfolios.Add(new StockPortfolio(file));
            }
            return StockPortfolio.Portfolios;
        }
        public StockPortfolio()
        {
        }
        public StockPortfolio(string fileName)
        {
            this.Name = Path.GetFileNameWithoutExtension(fileName);
            this.Orders = new List<StockOrder>();
            this.Operations = new List<StockOperation>();
            this.Positions = new List<StockPosition>();

            foreach (var operation in File.ReadAllLines(fileName, Encoding.GetEncoding(1252)).Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#")).Select(l => new StockOperation(l)).OrderBy(o => o.Id))
            {
                this.AddOperation(operation);
            }
        }

        public void AddOperation(StockOperation operation)
        {
            if (this.Operations.Any(op => op.Id == operation.Id))
                throw new InvalidOperationException($"Operation {operation.Id} already in portfolio");

            this.Operations.Add(operation);

            this.Balance = operation.Balance;
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
                                StockName = operation.BinckName
                            });
                        }
                    }
                    break;
                case StockOperation.BUY:
                    {
                        var qty = operation.Qty;
                        var stockName = operation.BinckName;
                        var position = this.Positions.FirstOrDefault(p => !p.IsClosed && p.StockName == stockName);
                        if (position != null) // Position on this stock already exists, add new values
                        {
                            position.EndDate = operation.Date;

                            var openValue = (position.OpenValue * position.Qty - operation.Amount) / (position.Qty + qty);

                            this.Positions.Add(new StockPosition
                            {
                                StartDate = operation.Date,
                                Qty = position.Qty + qty,
                                StockName = operation.BinckName,
                                OpenValue = openValue
                            });
                        }
                        else // Position on this stock doen't exists, create a new one
                        {
                            this.Positions.Add(new StockPosition
                            {
                                StartDate = operation.Date,
                                Qty = qty,
                                StockName = stockName,
                                OpenValue = -operation.Amount / qty
                            });
                        }
                    }
                    break;
                case StockOperation.SELL:
                    {
                        var qty = operation.Qty;
                        var stockName = operation.BinckName;
                        var position = this.Positions.FirstOrDefault(p => !p.IsClosed && p.StockName == stockName);
                        if (position != null)
                        {
                            position.EndDate = operation.Date;
                            if (position.Qty != qty)
                            {
                                this.Positions.Add(new StockPosition
                                {
                                    StartDate = operation.Date,
                                    Qty = position.Qty - qty,
                                    StockName = operation.BinckName,
                                    OpenValue = position.OpenValue
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

        public void Dump()
        {
            Console.WriteLine($"All Positions: {this.Positions.Count}");
            Console.WriteLine($"Opened Positions: {this.Positions.Count(p => !p.IsClosed)}");

            foreach (var p in this.Positions.Where(p => !p.IsClosed).OrderBy(p => p.StockName))
            {
                p.Dump();
            }
        }
        public void Dump(DateTime date)
        {
            Console.WriteLine($"Dump for date:{date.ToShortDateString()}");
            Console.WriteLine($"All Positions: {this.Positions.Count}");
            Console.WriteLine($"Opened Positions: {this.Positions.Count(p => !p.IsClosed)}");

            foreach (var p in this.Positions.Where(p => p.StartDate < date && p.EndDate > date).OrderBy(p => p.StockName))
            {
                p.Dump();
            }
        }

        public float EvaluateAt(DateTime date)
        {
            // Calculate value for opened positions
            var positions = this.Positions.Where(p => p.EndDate > date && p.StartDate <= date);
            float positionValue = 0f;
            foreach (var pos in positions)
            {
                float value = PriceProvider.GetClosingPrice(pos.StockName, date);
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

        public List<StockOrder> Orders { get; }
        public List<StockOperation> Operations { get; }
        public List<StockPosition> Positions { get; }
        public string Name { get; set; }
        public float InitialBalance { get; set; }
        public float Balance { get; set; }

        #region Binck Name Mapping
        private static List<StockNameMapping> mappings;
        public static List<StockNameMapping> Mappings => LoadMappings();

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
                    XmlSerializer serializer = new XmlSerializer(typeof(List<StockNameMapping>));
                    mappings = (List<StockNameMapping>)serializer.Deserialize(xmlReader);
                }
            }
            else
            {
                mappings = new List<StockNameMapping>();
            }
            return mappings;
        }

        public static StockNameMapping GetMapping(string binckName)
        {
            return Mappings.FirstOrDefault(m => binckName.Contains(m.BinckName.ToUpper()));
        }
        #endregion
    }
}