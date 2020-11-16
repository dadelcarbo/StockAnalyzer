using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockBinckPortfolio;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerTest
{
    [TestClass]
    public class StockTradeLogTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
            var folder = Path.Combine(Environment.CurrentDirectory, "TradeLog");
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
            Directory.CreateDirectory(folder);
        }

        [TestMethod]
        public void StockTradeLogPersistTest()
        {
            var folder = Path.Combine(Environment.CurrentDirectory, "TradeLog");
            var expectedPortfolio = new StockPortfolio()
            {
                Name = "TestPortfolio"
            };
            var tradeLog = expectedPortfolio.TradeLog;
            tradeLog.LogEntries.Add(new StockTradeLogEntry
            {
                BarDuration = StockBarDuration.Daily,
                EntryValue = 15,
                EntryQty = 100,
                EntryDate = DateTime.Today,
                StockName = "ACCOR"
            });

            expectedPortfolio.Save(folder);

            var actualPortfolio = StockPortfolio.LoadPortfolios(folder).First(p => p.Name == expectedPortfolio.Name);
            Assert.AreEqual(expectedPortfolio.TradeLog.LogEntries, expectedPortfolio.TradeLog.LogEntries);
            StockTradeLog.Load(folder, expectedPortfolio);
        }
    }
}
