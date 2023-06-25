using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockPortfolio.StockStrategy;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

                StockStrategy strategy = new StockStrategy()
                {
                    BarDuration = BarDuration.Daily,
                    Name = "Test",
                    Portfolio = Portfolios.First().Name,
                    StockName = "ERAMET"
                };
                strategy.EntryEvents.Add(new StockStrategyEvent
                {
                    IndicatorType = "TrailStop",
                    Indicator = "TRAILATR(35,35,0.75,-0.75,EMA,6)",
                    Event = "BrokenUp"
                });
                strategy.ExitEvents.Add(new StockStrategyEvent
                {
                    IndicatorType = "TrailStop",
                    Indicator = "TRAILATR(35,35,0.75,-0.75,EMA,6)",
                    Event = "BrokenDown"
                });
                strategy.Serialize();
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
            if (p?.Performance == null)
                return false;

            stockSerie.IsInitialised = false;
            foreach (var v in p.Performance.Balance.AccountValue)
            {
                stockSerie.Add(v.Date, new StockDailyValue(v.Value, v.Value, v.Value, v.Value, 0, v.Date));
            }
            return true;
        }

        public static List<StockPortfolio.StockPortfolio> Portfolios { get; set; }

        public override string DisplayName => "Portfolio";
    }
}
