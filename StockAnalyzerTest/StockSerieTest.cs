using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;

namespace StockAnalyzerTest
{
    [TestClass]
    public class StockSerieTest
    {
        [TestMethod]
        public void StockSerieLoad()
        {
            StockDataProviderBase.RootFolder = @"C:\Users\David\AppData\Roaming\UltimateChartistRoot";
            var serie = new StockSerie("BX4", "BX4", StockSerie.Groups.FUND, StockAnalyzer.StockClasses.StockDataProviders.StockDataProvider.ABC);

            serie.Initialise();
        }
    }
}
