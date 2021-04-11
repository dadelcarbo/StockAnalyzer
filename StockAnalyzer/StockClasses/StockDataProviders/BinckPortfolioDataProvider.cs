using StockAnalyzer.StockBinckPortfolio;
using System;
using System.Collections.Generic;
using System.IO;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class BinckPortfolioDataProvider : StockDataProviderBase
    {
        public const string PORTFOLIO_FOLDER = "portfolio";

        public override bool SupportsIntradayDownload
        {
            get { return false; }
        }

        public override void InitDictionary(StockDictionary stockDictionary, bool download)
        {
            string folder = Path.Combine(RootFolder, PORTFOLIO_FOLDER);
            try
            {
                NotifyProgress("Loading portfolio");
                Portofolios = StockPortfolio.LoadPortfolios(folder);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            return true;
        }

        public override bool LoadData(StockSerie stockSerie)
        {
            return true;
        }

        public static List<StockPortfolio> Portofolios { get; set; }

    }
}
