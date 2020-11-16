using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockAgent;
using StockAnalyzer.StockBinckPortfolio;

namespace StockAnalyzerTest
{
    [TestClass]
    public class StockBinckPortfolioTest
    {
        [TestMethod]
        public void PortfolioLoadAndSaveTest()
        {
            const string opLine1 = "0	01/03/2020 14:21	01/03/2020	Achat	427 USD / JPY CB BEST turbo short 125.3704	-500,00 €	9500 €";
            const string opLine3 = "1	02/03/2020 14:21	02/03/2020	Achat	427 EUR / JPY CB BEST turbo short 125.3704	-500,00 €	9000 €";
            const string opLine2 = "2	10/03/2020 10:04	10/03/2020	Vente	427 USD / JPY CB BEST turbo short 125.3704	510,00 €	9510 €";
            const string opLine4 = "3	12/03/2020 10:04	12/03/2020	Vente	427 EUR / JPY CB BEST turbo short 125.3704	450,00 €	9960 €";

            // Clean old files
            foreach (var file in Directory.EnumerateFiles(Environment.CurrentDirectory, "*.*ptf"))
            {
                File.Delete(file);
            }

            var portfolio = new StockPortfolio() { Balance = 10000, Name = "UnitTest" };
            portfolio.AddOperation(StockOperation.FromBinckLine(opLine1));
            portfolio.AddOperation(StockOperation.FromBinckLine(opLine2));
            portfolio.AddOperation(StockOperation.FromBinckLine(opLine3));
            portfolio.AddOperation(StockOperation.FromBinckLine(opLine4));

            portfolio.Save(Environment.CurrentDirectory);

            var actualPortfolio = StockPortfolio.LoadPortfolios(Environment.CurrentDirectory)[0];

            Assert.IsTrue(actualPortfolio.IsSimu);
            Assert.AreEqual(portfolio.Positions.Count, actualPortfolio.Positions.Count);
            Assert.AreEqual(portfolio.Operations.Count, actualPortfolio.Operations.Count);
        }

        [TestMethod]
        public void TradeTest()
        {
            var serie = StockSerieTest.GenerateTestStockSerie(100, 0.1f);

            // Test 1 Open but not closed
            var trade = new StockTrade(serie, 0);

            Assert.AreEqual(0, trade.Gain);
            Assert.AreEqual(0, trade.DrawDown);
            Assert.AreEqual(false, trade.IsClosed);
            Assert.AreEqual(false, trade.IsPartlyClosed);
            trade.Evaluate();
            Assert.AreEqual(serie.Values.Last().CLOSE, trade.ExitValue);

            // Test 2 Open and closed
            int closeIndex = 10;
            trade.CloseAtOpen(closeIndex);

            Assert.AreEqual(true, trade.IsClosed);
            Assert.AreEqual(false, trade.IsPartlyClosed);
            Assert.AreNotEqual(0, trade.Gain);
            Assert.AreEqual(0, trade.DrawDown);
            Assert.AreEqual(serie.Values.ElementAt(closeIndex).OPEN, trade.ExitValue);

            // Test 3 Open and Partly closed
            int partialCloseIndex = 5;
            trade = new StockTrade(serie, 0);
            trade.PartialClose(partialCloseIndex);

            Assert.AreEqual(false, trade.IsClosed);
            Assert.AreEqual(true, trade.IsPartlyClosed);
            Assert.AreEqual(0, trade.Gain);
            Assert.AreEqual(0, trade.DrawDown);
            trade.Evaluate();
            Assert.AreEqual(serie.Values.Last().CLOSE, trade.ExitValue);

            // Test 4 Open, partly closed and closed
            trade.CloseAtOpen(10);

            Assert.AreEqual(true, trade.IsClosed);
            Assert.AreEqual(false, trade.IsPartlyClosed);
            Assert.AreNotEqual(0, trade.Gain);
            Assert.AreEqual(0, trade.DrawDown);
            Assert.AreEqual(serie.Values.ElementAt(closeIndex).OPEN, trade.ExitValue);
        }

        [TestMethod]
        public void TradeListToPortfolio_ClosedTrade_Test()
        {
            var serie = StockSerieTest.GenerateTestStockSerie(100, 0.1f);
            var portfolio = StockPortfolio.CreateSimulationPortfolio();
            var trades = new List<StockTrade>();

            // Test 1 Open but not closed
            var trade1 = new StockTrade(serie, 0);
            trades.Add(trade1);

            portfolio.InitFromTradeSummary(trades);
            Assert.AreEqual(1, portfolio.Operations.Count);
            Assert.AreEqual(1, portfolio.Positions.Count);
            Assert.IsTrue(portfolio.PositionValue > 0f);

            // Test 2 Open and close
            trade1.CloseAtOpen(10);
            portfolio.InitFromTradeSummary(trades);
            Assert.AreEqual(2, portfolio.Operations.Count);
            Assert.AreEqual(2, portfolio.Positions.Count);
            Assert.IsTrue(portfolio.PositionValue == 0f);
        }
        [TestMethod]
        public void TradeListToPortfolio_PartlyClosedTrade_Test()
        {
            #region Test with rising stock
            {
                var serie = StockSerieTest.GenerateTestStockSerie(100, 0.1f);
                var portfolio = StockPortfolio.CreateSimulationPortfolio();
                var trades = new List<StockTrade>();

                // Test 1 Open but not closed
                var trade1 = new StockTrade(serie, 0);
                trades.Add(trade1);

                portfolio.InitFromTradeSummary(trades);
                Assert.AreEqual(1, portfolio.Operations.Count);
                Assert.AreEqual(1, portfolio.Positions.Count);
                Assert.IsTrue(portfolio.PositionValue > 0f);
                Assert.IsTrue(portfolio.Return > 0f);

                // Test 2 Opened and partial close
                trade1.PartialClose(5);
                portfolio.InitFromTradeSummary(trades);
                Assert.AreEqual(2, portfolio.Operations.Count);
                Assert.AreEqual(2, portfolio.Positions.Count);
                Assert.IsTrue(portfolio.PositionValue > 0f);
                Assert.IsTrue(portfolio.Return > 0f);

                // Test 3 Opened, partially closed and closed
                trade1.CloseAtOpen(10);
                portfolio.InitFromTradeSummary(trades);
                Assert.AreEqual(3, portfolio.Operations.Count);
                Assert.AreEqual(2, portfolio.Positions.Count);
                Assert.IsTrue(portfolio.PositionValue == 0f);
                Assert.IsTrue(portfolio.Return > 0f);
            }
            #endregion
            #region Test with declining stock
            {
                var serie = StockSerieTest.GenerateTestStockSerie(100, -0.01f);
                var portfolio = StockPortfolio.CreateSimulationPortfolio();
                var trades = new List<StockTrade>();

                // Test 1 Open but not closed
                var trade1 = new StockTrade(serie, 0);
                trades.Add(trade1);

                portfolio.InitFromTradeSummary(trades);
                Assert.AreEqual(1, portfolio.Operations.Count);
                Assert.AreEqual(1, portfolio.Positions.Count);
                Assert.IsTrue(portfolio.PositionValue > 0f);
                Assert.IsTrue(portfolio.Return < 0f);

                // Test 2 Opened and partial close
                trade1.PartialClose(5);
                portfolio.InitFromTradeSummary(trades);
                Assert.AreEqual(2, portfolio.Operations.Count);
                Assert.AreEqual(2, portfolio.Positions.Count);
                Assert.IsTrue(portfolio.PositionValue > 0f);
                Assert.IsTrue(portfolio.Return < 0f);

                // Test 3 Opened, partially closed and closed
                trade1.CloseAtOpen(10);
                portfolio.InitFromTradeSummary(trades);
                Assert.AreEqual(3, portfolio.Operations.Count);
                Assert.AreEqual(2, portfolio.Positions.Count);
                Assert.IsTrue(portfolio.PositionValue == 0f);
                Assert.IsTrue(portfolio.Return < 0f);
            }
            #endregion
        }
    }
}