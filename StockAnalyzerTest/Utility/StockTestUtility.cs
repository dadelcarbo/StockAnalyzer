using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using System;
using static StockAnalyzer.StockClasses.StockSerie;

namespace StockAnalyzerTest.Utility
{
    public static class StockTestUtility
    {
        public static StockSerie StockSerieLoad(string stockName, string shortName, Groups stockGroup, StockDataProvider dataProvider)
        {
            //StockDataProviderBase.RootFolder = @"C:\Users\r395930\AppData\Roaming\UltimateChartistRoot";
            StockDataProviderBase.RootFolder = @"C:\Users\David\AppData\Roaming\UltimateChartistRoot";
            var serie = new StockSerie(stockName, shortName, stockGroup, dataProvider);

            if (serie.Initialise())
            {
                return serie;
            }

            throw new ArgumentException("Cannot create stock serie for " + stockName);
        }

        public static StockDictionary InitDictionnary(StockDataProvider dataProviderType)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            StockDataProviderBase.RootFolder = path + @"\UltimateChartistRoot";

            var stockDictionary = new StockDictionary(new DateTime(DateTime.Now.Year - 1, 01, 01));

            IStockDataProvider dataProvider = StockDataProviderBase.GetDataProvider(dataProviderType);

            dataProvider.InitDictionary(StockDataProviderBase.RootFolder, stockDictionary, false);

            return stockDictionary;
        }
    }
}
