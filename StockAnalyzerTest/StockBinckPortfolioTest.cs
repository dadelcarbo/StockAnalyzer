using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockAgent;
using StockAnalyzer.StockBinckPortfolio;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerTest
{
    [TestClass]
    public class StockBinckPortfolioTest
    {
        [TestMethod]
        public void PortfolioLoadAndSaveTest()
        {
            const string opLine1 = "0	01/03/2020 14:21	01/03/2020	Achat	427 USD / JPY CB BEST turbo short 125.3704	-500,00 €	9500 €";
            const string opLine2 = "0	10/03/2020 10:04	10/03/2020	Vente	427 USD / JPY CB BEST turbo short 125.3704	510,00 €	9510 €";
            const string opLine3 = "0	02/03/2020 14:21	02/03/2020	Achat	427 EUR / JPY CB BEST turbo short 125.3704	-500,00 €	9000 €";
            const string opLine4 = "0	12/03/2020 10:04	12/03/2020	Vente	427 EUR / JPY CB BEST turbo short 125.3704	450,00 €	9960 €";

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
        public void TradeListToPortfolioTest()
        {
            var serie = StockSerieTest.GenerateTestStockSerie(100);
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
            trade1.Close(10);
            portfolio.InitFromTradeSummary(trades);
            Assert.AreEqual(2, portfolio.Operations.Count);
            Assert.AreEqual(2, portfolio.Positions.Count);
            Assert.IsTrue(portfolio.PositionValue == 0f);
        }

    }
}