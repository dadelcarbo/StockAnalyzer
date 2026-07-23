using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using StockAnalyzer.UltimatePortfolio;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockData.DataProviders.SaxoPortfolio
{
    public class SaxoPortfolioDataProvider : DataProviderBase
    {
        public override string DisplayName => "Saxo Portfolio";

        public override BarDuration[] SupportedDurations => new BarDuration[] { BarDuration.Daily, BarDuration.Weekly, BarDuration.Monthly };
        public override BarDuration DefaultDuration => BarDuration.Daily;

        public override DataProvider Provider => DataProvider.SaxoPortfolio;

        public override void OpenInDataProvider(StockInstrument stockInstrument)
        {
            Process.Start("https://www.saxotrader.fr/d/trading/open-positions");
        }

        protected override StockInstrument CreateInstrumentFromConfigLine(string line)
        {
            throw new NotImplementedException();
        }

        List<StockPortfolio.StockPortfolio> portfolios;
        protected override void PreInitDictionary(bool download)
        {
            NotifyProgress("Loading portfolio");
            portfolios = StockPortfolio.StockPortfolio.Portfolios.Where(p => !string.IsNullOrEmpty(p.SaxoClientId)).ToList();
            foreach (var p in portfolios)
            {
                var instrument = new StockInstrument
                {
                    Id = p.Name,
                    Name = p.Name,
                    Symbol = p.SaxoClientId,
                    Group = Groups.Portfolio,
                    Provider = DataProvider.SaxoPortfolio,
                    Market = Market.TURBO
                };
                StockDictionary.Instruments.Add(p.Name, instrument);

                if (NeedDownload(instrument))
                    DownloadData(instrument);
            }
        }

        protected override void PostInitDictionary(bool download)
        {
        }

        public override DataSerie DownloadData(StockInstrument instrument)
        {
            try
            {
                var portfolio = portfolios.FirstOrDefault(p => p.Name == instrument.Id);
                if (portfolio?.AccountValue == null || portfolio.AccountValue.Length == 0)
                    return null;

                DataSerie dataSerie = LoadData(instrument, DefaultDuration);
                DateTime startDate = dataSerie?.LastValue != null ? dataSerie.LastValue.DATE.Date : DateTime.MinValue;

                var newBars = portfolio.AccountValue.Where(v => v.Date > startDate).Select(v => new StockDailyValue(v.Value, v.Value, v.Value, v.Value, 0, v.Date)).ToArray();


                NotifyProgress($"Downloading {instrument.DisplayName}");

                if (newBars != null && newBars.Length > 0)
                {
                    if (IsMarketOpened(instrument) && newBars.Last().DATE == DateTime.Today)
                        newBars.Last().IsComplete = false;

                    var pivotDate = newBars[0].DATE;
                    newBars = dataSerie == null ? newBars : dataSerie.Values.Where(v => v.DATE < pivotDate).Union(newBars).ToArray();

                    StockBar.Serialize(GetInstrumentFilePath(instrument), newBars);

                    dataSerie = new DataSerie(instrument, DefaultDuration, newBars);
                    instrument.SetDataSerie(DefaultDuration, dataSerie);

                    var history = GetDownloadHistory(instrument);
                    history.LastDate = dataSerie.LastCompleteValue.DATE;
                    history.DownloadDate = DateTime.Now;
                }
                else
                {
                    StockLog.Write($"Download {instrument.DisplayName} failed");
                }

                return dataSerie;

            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            return null;
        }
    }
}
