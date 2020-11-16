using StockAnalyzer.StockBinckPortfolio;
using StockAnalyzer.StockClasses;
using StockAnalyzerSettings.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace StockAnalyzer.StockBinckPortfolio
{
    public class StockTradeLog
    {
        public StockTradeLog()
        {
            this.LogEntries = new List<StockTradeLogEntry>();
        }
        public StockTradeLog(StockPortfolio portfolio)
        {
            this.Portfolio = portfolio;
            this.LogEntries = new List<StockTradeLogEntry>();
        }
        [XmlIgnore]
        public StockPortfolio Portfolio { get; set; }
        public List<StockTradeLogEntry> LogEntries { get; set; }

        #region PERSISTENCY
        public static StockTradeLog Load(string folder, StockPortfolio portfolio)
        {
            StockTradeLog tradeLog = null;

            string filepath = Path.Combine(folder, portfolio.Name + ".tlog");
            if (File.Exists(filepath))
            {
                using (var fs = new FileStream(filepath, FileMode.Open))
                {
                    System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings
                    {
                        IgnoreWhitespace = true
                    };
                    var xmlReader = System.Xml.XmlReader.Create(fs, settings);
                    var serializer = new XmlSerializer(typeof(StockTradeLog));
                    tradeLog = serializer.Deserialize(xmlReader) as StockTradeLog;
                    tradeLog.Portfolio = portfolio;
                }
            }
            else
            {
                tradeLog = new StockTradeLog(portfolio);
            }

            return tradeLog;
        }
        public void Save(string folder)
        {
            string filepath = Path.Combine(folder, this.Portfolio.Name + ".tlog");
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
        #endregion
    }
}
