using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockClasses.StockDataProviders;

namespace StockAnalyzerTest
{
    [TestClass]
    public class StockBarChartTest
    {
        [TestMethod]
        public void DownloadDailyBarChartTest()
        {
            BarChartDataProvider bcDataProvider = new BarChartDataProvider();

            string url = BarChartDataProvider.FormatDailyURL("AAPL", DateTime.Today.AddMonths(6), DateTime.Today);

        }
    }
}
