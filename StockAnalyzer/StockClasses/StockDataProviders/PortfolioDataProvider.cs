using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.IO;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class PortfolioDataProvider : StockDataProviderBase
    {
        public override bool SupportsIntradayDownload
        {
            get { return false; }
        }

        public override void InitDictionary(StockDictionary stockDictionary, bool download)
        {
            string folder = Folders.Portfolio;
            try
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                NotifyProgress("Loading portfolio");
                Portfolios = StockPortfolio.StockPortfolio.LoadPortfolios(folder);

                folder += @"\Report";
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
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

        public static List<StockPortfolio.StockPortfolio> Portfolios { get; set; }

    }
}
