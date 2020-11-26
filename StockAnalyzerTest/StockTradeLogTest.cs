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
        public void StockPortfolioOperationTest()
        {
            float expectedBalance = 10000f;
            var actualPortfolio = new StockPortfolio()
            {
                Name = "TestPortfolio",
                InitialBalance = expectedBalance,
                Balance = expectedBalance,
                IsSimu = false
            };

            int nbOperation = 0;

            #region DEPOSIT/WITHDRAWAL
            actualPortfolio.DepositTradeOperation(DateTime.Today.AddDays(nbOperation++), 1000f);

            expectedBalance += 1000f;
            Assert.AreEqual(expectedBalance, actualPortfolio.Balance);
            Assert.AreEqual(nbOperation, actualPortfolio.GetNextOperationId());
            Assert.AreEqual(nbOperation, actualPortfolio.TradeOperations.Count);

            actualPortfolio.DepositTradeOperation(DateTime.Today.AddDays(nbOperation++), -1000f);

            expectedBalance -= 1000f;
            Assert.AreEqual(expectedBalance, actualPortfolio.Balance);
            Assert.AreEqual(nbOperation, actualPortfolio.GetNextOperationId());
            Assert.AreEqual(nbOperation, actualPortfolio.TradeOperations.Count);

            actualPortfolio.DividendTradeOperation("SOLOCAL", DateTime.Today.AddDays(nbOperation++), 0.25f, 100);

            expectedBalance += 0.25f * 100;
            Assert.AreEqual(expectedBalance, actualPortfolio.Balance);
            Assert.AreEqual(nbOperation, actualPortfolio.GetNextOperationId());
            Assert.AreEqual(nbOperation, actualPortfolio.TradeOperations.Count);
            #endregion


            #region BUY/SELL Full Positions
            actualPortfolio.BuyTradeOperation("ACCOR HOTELS", DateTime.Today.AddDays(nbOperation++), 100, 15f, 2.5f, 14f, "Entry for Unit Test", StockBarDuration.Daily, "CLOUD|TRAILATRBAND(20,2.5,-2.5,MA,3)");

            expectedBalance -= 100f * 15f + 2.5f;
            Assert.AreEqual(expectedBalance, actualPortfolio.Balance);
            Assert.AreEqual(nbOperation, actualPortfolio.GetNextOperationId());
            Assert.AreEqual(nbOperation, actualPortfolio.TradeOperations.Count);
            Assert.AreEqual(1, actualPortfolio.OpenedPositions.Count());

            actualPortfolio.SellTradeOperation("ACCOR HOTELS", DateTime.Today.AddDays(nbOperation++), 100, 20f, 2.5f, "Exit for Unit Test");
            expectedBalance += 100f * 20f - 2.5f;
            Assert.AreEqual(expectedBalance, actualPortfolio.Balance);
            Assert.AreEqual(nbOperation, actualPortfolio.GetNextOperationId());
            Assert.AreEqual(nbOperation, actualPortfolio.TradeOperations.Count);
            Assert.AreEqual(0, actualPortfolio.OpenedPositions.Count());
            #endregion

        }

        [TestMethod]
        public void StockPortfolioPersistTest()
        {
            var folder = Path.Combine(Environment.CurrentDirectory, "TradeLog");
            var expectedPortfolio = new StockPortfolio()
            {
                Name = "TestPortfolio",
                InitialBalance = 10000,
                Balance = 10000,
                IsSimu = false
            };
            expectedPortfolio.BuyTradeOperation("ACCOR HOTELS", DateTime.Today, 100, 15, 2.5f, 2.25f,
                "Entry for Unit Test", StockBarDuration.Daily, "CLOUD|TRAILATRBAND(20,2.5,-2.5,MA,3)");

            expectedPortfolio.LogEntries.Add(new StockTradeLogEntry
            {
                BarDuration = StockBarDuration.Daily,
                EntryValue = 15,
                EntryQty = 100,
                EntryDate = DateTime.Today,
                StockName = "ACCOR"
            });

            expectedPortfolio.Serialize(folder);

            var actualPortfolio = StockPortfolio.LoadPortfolios(folder).First(p => p.Name == "TestPortfolio");

            Assert.AreEqual(0, actualPortfolio.Operations.Count);
            Assert.AreEqual(expectedPortfolio.Positions.Count, actualPortfolio.Positions.Count);
            Assert.AreEqual(expectedPortfolio.TradeOperations.Count, actualPortfolio.TradeOperations.Count);
            Assert.AreEqual(expectedPortfolio.LogEntries.Count, actualPortfolio.LogEntries.Count);


            //var actualPortfolio = StockPortfolio.LoadPortfolios(folder).First(p => p.Name == expectedPortfolio.Name);
            //Assert.AreEqual(expectedPortfolio.TradeLog.LogEntries, expectedPortfolio.TradeLog.LogEntries);
            //StockTradeLog.Load(folder, expectedPortfolio);
        }
    }
}
