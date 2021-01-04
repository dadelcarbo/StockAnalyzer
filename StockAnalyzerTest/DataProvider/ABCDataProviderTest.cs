using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using System;
using System.IO;

namespace StockAnalyzerTest.DataProvider
{
    [TestClass]
    public class ABCDataProviderTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
            StockDataProviderBase.RootFolder = Environment.CurrentDirectory;
            ABCDataProvider.CreateDirectories();
        }
        [TestMethod]
        public void ForceDownload()
        {
            var dataProvider = new ABCDataProvider();
            var stockSerie = new StockSerie("Total", "FP", StockSerie.Groups.EURO_A, StockDataProvider.ABC, BarDuration.Daily) { ISIN = "FR0000120271"};
            dataProvider.ForceDownloadData(stockSerie);
        }
    }
}
