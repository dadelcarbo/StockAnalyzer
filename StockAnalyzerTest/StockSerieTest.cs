using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzerTest.Utility;
using System;
using System.Linq;

namespace StockAnalyzerTest
{
    [TestClass]
    public class StockSerieTest
    {
        [TestMethod]
        public void StockSerieLoad()
        {
            var serie = StockTestUtility.StockSerieLoad("BX4", "BX4", StockSerie.Groups.FUND, StockDataProvider.ABC);

            serie.Initialise();

            serie.BarDuration = StockBarDuration.Daily;
            serie.BarDuration = new StockBarDuration() { Smoothing = 2 };
            serie.BarDuration = new StockBarDuration() { Smoothing = 3 };

            serie = StockTestUtility.StockSerieLoad("INT_ACCOR", "AC", StockSerie.Groups.INTRADAY, StockDataProvider.InvestingIntraday);

            serie.Initialise();

            serie.BarDuration = StockBarDuration.Daily;
            serie.BarDuration = new StockBarDuration() { Smoothing = 2 };
            serie.BarDuration = new StockBarDuration() { Smoothing = 3 };
        }

        [TestMethod]
        public void StockDictionnaryLoad()
        {
            var dataProviderType = StockDataProvider.InvestingIntraday;

            var stockDictionary = StockTestUtility.InitDictionnary(dataProviderType);
            Assert.AreNotEqual(0, stockDictionary.Count);

            foreach(var stockSerie in stockDictionary.Values.Take(10))
            {
                Assert.IsTrue(stockSerie.Initialise());
            }
        }
    }
}
