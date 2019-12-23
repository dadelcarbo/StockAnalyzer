using System;
using System.Collections.Generic;
using System.IO;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class BinckPortfolioDataProvider : StockDataProviderBase
    {
        private const string PORTFOLIO_FOLDER = "portfolio";

        public override bool SupportsIntradayDownload
        {
            get { return false; }
        }

        public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
        {
            string folder = Path.Combine(rootFolder, PORTFOLIO_FOLDER);
            try
            {
                Portofolios = StockPortfolio3.StockPortfolio.LoadPortfolios(folder);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
        {
            return true;
        }

        public override bool LoadData(string rootFolder, StockSerie stockSerie)
        {
            return true;
        }

        public static List<StockPortfolio3.StockPortfolio> Portofolios { get; set; }
    }
}
