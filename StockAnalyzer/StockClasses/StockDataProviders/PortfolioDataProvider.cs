using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class PortfolioDataProvider : StockDataProviderBase
    {
        public override bool SupportsIntradayDownload => false;

        public override void InitDictionary(StockDictionary stockDictionary, bool download)
        {
            try
            {
                if (!Directory.Exists(Folders.Portfolio))
                {
                    Directory.CreateDirectory(Folders.Portfolio);
                }
                if (!Directory.Exists(Folders.AutoTrade))
                {
                    Directory.CreateDirectory(Folders.AutoTrade);
                }
                if (!Directory.Exists(Folders.Strategy))
                {
                    Directory.CreateDirectory(Folders.Strategy);
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
                foreach (var p in Portfolios.Where(p => !string.IsNullOrEmpty(p.SaxoClientId)))
                {
                    var stockSerie = new StockSerie(p.Name, p.SaxoClientId, StockSerie.Groups.Portfolio, StockDataProvider.Portfolio, BarDuration.Daily);
                    stockDictionary.Add(p.Name, stockSerie);
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
            var p = Portfolios.FirstOrDefault(p => p.Name == stockSerie.StockName);

            stockSerie.IsInitialised = false;
            if (p.AccountValue == null)
                return false;

            foreach (var v in p.AccountValue)
            {
                stockSerie.Add(v.Date, new StockDailyValue(v.Value, v.Value, v.Value, v.Value, 0, v.Date));
            }
            var lastAccountValue = p.AccountValue.Last();
            var lastCacDate = StockDictionary.Instance["CAC40"].LastValue.DATE.Date;
            if (lastAccountValue.Date.Date <= DateTime.Today && DateTime.Today.DayOfWeek != DayOfWeek.Sunday && DateTime.Today.DayOfWeek != DayOfWeek.Saturday)
            {
                stockSerie.Add(DateTime.Today, new StockDailyValue(p.TotalValue, p.TotalValue, p.TotalValue, p.TotalValue, 0, DateTime.Today));
            }
            return true;
        }

        public static List<StockPortfolio.StockPortfolio> Portfolios { get; set; }

        public override string DisplayName => "Portfolio";
    }
}
