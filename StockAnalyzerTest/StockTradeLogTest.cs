using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockPortfolio;
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
        public void PortfolioCompositeOperationTest()
        {
            float expectedBalance = 10000f;
            var actualPortfolio = new StockPortfolio()
            {
                Name = "TestPortfolio",
                InitialBalance = expectedBalance,
                Balance = expectedBalance,
            };

            int nbOperation = 0;

            #region BUY/SELL in two times
            int qty = 100;
            float value = 15f;
            float fee = 2.5f;
            actualPortfolio.BuyTradeOperation("AIRBUS", DateTime.Today.AddDays(nbOperation++), qty, value, fee, 14f, "Entry1 for Unit Test", StockBarDuration.Daily, "CLOUD|TRAILATRBAND(20,2.5,-2.5,MA,3)");

            expectedBalance -= qty * value + fee;
            Assert.AreEqual(expectedBalance, actualPortfolio.Balance);
            Assert.AreEqual(nbOperation, actualPortfolio.GetNextOperationId());
            Assert.AreEqual(nbOperation, actualPortfolio.TradeOperations.Count);
            Assert.AreEqual(1, actualPortfolio.OpenedPositions.Count());
            var actualPosition = actualPortfolio.OpenedPositions.First();
            Assert.AreEqual(qty, actualPosition.EntryQty);
            Assert.AreEqual((qty * value + fee) / qty, actualPosition.EntryValue);
            Assert.AreEqual((qty * value + fee), actualPosition.EntryCost);

            actualPortfolio.BuyTradeOperation("AIRBUS", DateTime.Today.AddDays(nbOperation++), 50, 16f, fee, 15f, "Entry2 for Unit Test", StockBarDuration.Daily, "CLOUD|TRAILATRBAND(20,2.5,-2.5,MA,3)");

            expectedBalance -= 50f * 16f + 2.5f;
            Assert.AreEqual(expectedBalance, actualPortfolio.Balance);
            Assert.AreEqual(nbOperation, actualPortfolio.GetNextOperationId());
            Assert.AreEqual(nbOperation, actualPortfolio.TradeOperations.Count);
            Assert.AreEqual(1, actualPortfolio.OpenedPositions.Count());
            actualPosition = actualPortfolio.OpenedPositions.First();

            try
            {
                actualPortfolio.BuyTradeOperation("TTT", DateTime.Today, 1000, 100, 5, 90, "Should be rejected", StockBarDuration.Daily, null);
                Assert.Fail("Buy operation should have raised an exception");
            }
            catch
            {
            }

            actualPortfolio.SellTradeOperation("AIRBUS", DateTime.Today.AddDays(nbOperation++), 50, 16, 2.5f, "Partial Exit for Unit Test");
            expectedBalance += 50 * 16 - 2.5f;
            Assert.AreEqual(expectedBalance, actualPortfolio.Balance);
            Assert.AreEqual(nbOperation, actualPortfolio.GetNextOperationId());
            Assert.AreEqual(nbOperation, actualPortfolio.TradeOperations.Count);
            Assert.AreEqual(1, actualPortfolio.OpenedPositions.Count());

            actualPortfolio.SellTradeOperation("AIRBUS", DateTime.Today.AddDays(nbOperation++), 50, 18, 2.5f, "Partial Exit for Unit Test");
            expectedBalance += 50 * 18 - 2.5f;
            Assert.AreEqual(expectedBalance, actualPortfolio.Balance);
            Assert.AreEqual(nbOperation, actualPortfolio.GetNextOperationId());
            Assert.AreEqual(nbOperation, actualPortfolio.TradeOperations.Count);
            Assert.AreEqual(1, actualPortfolio.OpenedPositions.Count());
            #endregion
        }

        [TestMethod]
        public void PortfolioPersistTest()
        {
            var folder = Path.Combine(Environment.CurrentDirectory, "TradeLog");
            var expectedPortfolio = new StockPortfolio()
            {
                Name = "TestPortfolio",
                InitialBalance = 10000,
                Balance = 10000,
            };
            expectedPortfolio.BuyTradeOperation("ACCOR HOTELS", DateTime.Today, 100, 15f, 2.5f, 14f, "Entry for Unit Test", StockBarDuration.Daily, "CLOUD|TRAILATRBAND(20,2.5,-2.5,MA,3)");

            expectedPortfolio.Serialize();

            var actualPortfolio = StockPortfolio.LoadPortfolios(folder).First(p => p.Name == "TestPortfolio");

            Assert.AreEqual(expectedPortfolio.Positions.Count, actualPortfolio.Positions.Count);
            Assert.AreEqual(expectedPortfolio.TradeOperations.Count, actualPortfolio.TradeOperations.Count);


            //var actualPortfolio = StockPortfolio.LoadPortfolios(folder).First(p => p.Name == expectedPortfolio.Name);
            //Assert.AreEqual(expectedPortfolio.TradeLog.LogEntries, expectedPortfolio.TradeLog.LogEntries);
            //StockTradeLog.Load(folder, expectedPortfolio);
        }

        [TestMethod]
        public void LoadFromExcelFile()
        {
            var fileName = @"C:\Users\David\Downloads\Transactions_13040737_2020-01-01_2020-12-15.xlsx";

            var dataTable = LoadWorksheetInDataTable(fileName, "Transactions");

            var blabla = dataTable.Select().Where(r => r["Account ID"] as string == "78800/719479EUR");

            foreach (var row in blabla)
            {
                var x = row[""];
            }
        }

        DataTable LoadWorksheetInDataTable(string fileName, string sheetName)
        {
            DataTable sheetData = new DataTable();
            using (OleDbConnection conn = this.returnConnection(fileName))
            {
                conn.Open();
                // retrieve the data using data adapter
                OleDbDataAdapter sheetAdapter = new OleDbDataAdapter("select * from [" + sheetName + "$]", conn);
                sheetAdapter.Fill(sheetData);
                conn.Close();
            }
            return sheetData;
        }

        private OleDbConnection returnConnection(string fileName)
        {
            //return new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + "; Jet OLEDB:Engine Type=5;Extended Properties=\"Excel 8.0;\"");
            return new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties=Excel 12.0;");

        }
    }
}
