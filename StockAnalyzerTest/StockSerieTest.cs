using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzerTest.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using static StockAnalyzer.StockClasses.StockSerie;

namespace StockAnalyzerTest
{
    [TestClass]
    public class StockSerieTest
    {
        public static StockSerie GenerateTestStockSerie(int size, float variation)
        {
            const string stockName = "TEST";
            StockSerie stockSerie = new StockSerie(stockName, stockName, Groups.NONE, StockDataProvider.Generated, BarDuration.Daily);
            stockSerie.IsPortofolioSerie = false;

            float value = 10.0f;
            for (DateTime date = DateTime.Today.AddDays(-size); date <= DateTime.Today; date = date.AddDays(1))
            {
                stockSerie.Add(date, new StockDailyValue(value, value, value, value, 0, date));
                value += variation;
            }

            // Initialise the serie
            stockSerie.Initialise();
            return stockSerie;
        }

        [TestMethod]
        public void StockSerieLoad()
        {
            var serie = StockTestUtility.StockSerieLoad("Lyxor CAC 40 (-2x) Inverse", "BX4.PA", StockSerie.Groups.FUND, StockDataProvider.ABC);

            serie.BarDuration = StockBarDuration.Daily;
            serie.BarDuration = new StockBarDuration() { Smoothing = 2 };
            serie.BarDuration = new StockBarDuration() { Smoothing = 3 };

            serie = StockTestUtility.StockSerieLoad("INT_ACCOR", "AC", StockSerie.Groups.INTRADAY, StockDataProvider.InvestingIntraday);

            serie.BarDuration = StockBarDuration.Daily;
            serie.BarDuration = new StockBarDuration() { Smoothing = 2 };
            serie.BarDuration = new StockBarDuration() { Smoothing = 3 };
        }

        [TestMethod]
        public void StockSerieMultipleTimeFrameDataTest()
        {
            var serie = StockTestUtility.StockSerieLoad("Lyxor CAC 40 (-2x) Inverse", "BX4.PA", StockSerie.Groups.FUND, StockDataProvider.ABC);

            var durations = new List<StockBarDuration> { StockBarDuration.Daily, StockBarDuration.Weekly, StockBarDuration.Monthly };

            var vars = serie.GetMTFVariation(durations, 10, new DateTime(2020, 02, 19));

            var dump = vars.Select(v => v.Select(va => va.ToString("P2")).Aggregate((i, j) => i + " " + j)).Aggregate((i, j) => i + Environment.NewLine + j);
            Assert.IsNotNull(dump);
        }

        [TestMethod]
        public void StockDictionnaryLoad()
        {
            var dataProviderType = StockDataProvider.InvestingIntraday;

            var stockDictionary = StockTestUtility.InitDictionnary(dataProviderType);
            Assert.AreNotEqual(0, stockDictionary.Count);

            foreach (var stockSerie in stockDictionary.Values.Take(10))
            {
                Assert.IsTrue(stockSerie.Initialise());
            }
        }
    }
}
