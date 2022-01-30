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
            try
            {
                if (!Directory.Exists(Folders.Portfolio))
                {
                    Directory.CreateDirectory(Folders.Portfolio);
                }
                var processedFolder = Path.Combine(Folders.Portfolio, "Processed");
                if (!Directory.Exists(processedFolder))
                {
                    Directory.CreateDirectory(processedFolder);
                }
                var reportFolder = Path.Combine(Folders.Portfolio, "Report");
                if (!Directory.Exists(reportFolder))
                {
                    Directory.CreateDirectory(reportFolder);
                }

                NotifyProgress("Loading portfolio");
                Portfolios = StockPortfolio.StockPortfolio.LoadPortfolios(Folders.Portfolio);
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
