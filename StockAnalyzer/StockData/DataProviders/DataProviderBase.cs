using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace StockAnalyzer.StockData.DataProviders
{
    public class MarketHours
    {
        public TimeSpan Open { get; set; }
        public TimeSpan Close { get; set; }

        /// <summary>
        /// Specifyies what time download is available. For US time is 24h + 6h (as data available at 6:00 am the next day)
        /// </summary>
        public TimeSpan DownloadAvailability { get; set; }

        public bool IsOpened => DateTime.Now.TimeOfDay >= Open && DateTime.Now.TimeOfDay <= Close;
    }

    public abstract class DataProviderBase : IDataProvider
    {
        SortedDictionary<Market, MarketHours> MarketHoursTable = new SortedDictionary<Market, MarketHours>() {
            { Market.EURONEXT , new MarketHours() { Open = new TimeSpan(9,0,0), Close = new TimeSpan(17,35,00), DownloadAvailability = new TimeSpan(18,0,0)} },
            { Market.XETRA , new MarketHours() { Open = new TimeSpan(9,0,0), Close = new TimeSpan(17,35,00), DownloadAvailability = new TimeSpan(21,0,0)} },
            { Market.NYSE,  new MarketHours() { Open = new TimeSpan(15,30,0), Close = new TimeSpan(22,00,00), DownloadAvailability = new TimeSpan(30,0,0)} },
            { Market.TURBO ,  new MarketHours() { Open = new TimeSpan(8,0,0), Close = new TimeSpan(22,00,00), DownloadAvailability = new TimeSpan(22,0,0)} },
            { Market.MIXED ,  new MarketHours() { Open = new TimeSpan(8,0,0), Close = new TimeSpan(22,00,00), DownloadAvailability = new TimeSpan(21,0,0)} }
        };

        protected TimeSpan shortDelay = new TimeSpan(0, 2, 0);

        protected static readonly DateTime refDate = new DateTime(1970, 01, 01);

        public abstract string DisplayName { get; }

        public abstract BarDuration[] SupportedDurations { get; }

        public abstract BarDuration DefaultDuration { get; }

        public bool SupportsDuration(BarDuration duration) => this.SupportedDurations.Contains(duration);

        public abstract DataProvider Provider { get; }

        protected string DataRootFolder { get; private set; }
        protected string DataFolder { get; private set; }
        protected string ConfigFile { get; private set; }
        protected string HistoryFile { get; private set; }

        public static event DownloadingEventHandler DownloadStarted;
        protected void NotifyProgress(string text)
        {
            if (DownloadStarted != null)
            {
                DownloadStarted(text);
            }
        }

        static protected CultureInfo frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
        static protected CultureInfo usCulture = CultureInfo.GetCultureInfo("en-US");

        public const int ARCHIVE_START_YEAR = 2025;

        public void AddSplit(StockInstrument instrument, DateTime date, float before, float after)
        {
            throw new NotImplementedException();
        }

        protected string GetInstrumentFilePath(StockInstrument instrument)
        {
            return string.IsNullOrEmpty(instrument.Symbol) ? Path.Combine(DataFolder, $"{instrument.Isin}.dat") : Path.Combine(DataFolder, $"{instrument.Isin}_{instrument.Symbol}.dat");
        }

        public DataSerie LoadData(StockInstrument instrument, BarDuration barDuration)
        {
            if (barDuration != DefaultDuration)
            {
                return instrument.GetDefaultDataSerie()?.ConvertConvertToDuration(barDuration);
            }
            else
            {
                var fileName = GetInstrumentFilePath(instrument);
                if (File.Exists(fileName))
                {
                    var bars = StockBar.Deserialize(fileName);
                    if (bars != null && bars.Length > 0)
                    {
                        return new DataSerie(instrument, barDuration, bars);
                    }
                }
            }
            return null;
        }

        static SortedDictionary<DataProvider, IDataProvider> DataProviders { get; } = new SortedDictionary<DataProvider, IDataProvider>()
        {
            {DataProvider.ABC, new AbcBourse.AbcDataProvider() },
            {DataProvider.SaxoTurbo, new SaxoTurbos.SaxoTurboDataProvider()},
            {DataProvider.Vontobel, new Vontobel.VontobelDataProvider()},
            {DataProvider.SocGen, new SocGen.SocGenDataProvider()}
        };

        public static IDataProvider GetDataProvider(DataProvider dataProvider)
        {
            if (DataProviders.TryGetValue(dataProvider, out var provider))
            {
                return provider;
            }
            return null;
        }

        public static void Initialize(bool download)
        {
            foreach (var provider in DataProviders)
            {
                StockLog.Write($"Initializing data provider: {provider.Key}");
                provider.Value.InitDictionary(download);
            }
        }
        /// <summary>
        /// DataSerie used to check id new download is required. If the last bar of the RefSerie is older than the last bar of the instrument, then a new download is required.
        /// </summary>
        protected DataSerie RefSerie { get; private set; }
        protected bool needDownload { get; private set; } = true;

        protected List<InstrumentDownloadHistory> InstrumentsHistory = new List<InstrumentDownloadHistory>();
        public void InitDictionary(bool download)
        {
            this.DataRootFolder = Path.Combine(Folders.DataFolder, Provider.ToString());
            this.DataFolder = Path.Combine(DataRootFolder, "Dat");
            this.ConfigFile = Path.Combine(Folders.PersonalFolder, $"Provider_{Provider}.cfg");
            this.HistoryFile = Path.Combine(DataRootFolder, $"History_{Provider}.cfg");

            if (!Directory.Exists(DataFolder))
            {
                Directory.CreateDirectory(DataFolder);
            }
            if (!File.Exists(ConfigFile))
            {
                File.Create(ConfigFile).Dispose();
            }
            if (!File.Exists(HistoryFile))
            {
                File.Create(HistoryFile).Dispose();
            }
            else
            {
                InstrumentsHistory = InstrumentDownloadHistory.Load(HistoryFile);
            }

            // Process custom initialization for the data provider
            PreInitDictionary(download);

            // Load the instruments from the config file
            foreach (var line in File.ReadAllLines(ConfigFile).Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#")))
            {
                var instrument = CreateInstrumentFromConfigLine(line);

                if (instrument == null)
                    continue;
                if (StockDictionary.Instruments.ContainsKey(instrument.Id))
                {
                    StockLog.Write($"Duplicate instrument found in config file: {Provider} {instrument.Id} line: {line}. Skipping.");
                    continue;
                }
                StockDictionary.Instruments.Add(instrument.Id, instrument);

                if (download && needDownload)
                {
                    if (RefSerie == null)
                    {
                        if (NeedDownload(instrument))
                        {
                            RefSerie = LoadData(instrument, DefaultDuration);

                            // Download Instrument
                            var newSerie = DownloadData(instrument);
                            if (newSerie?.LastValue != null)
                            {
                                if (RefSerie?.LastValue == null || RefSerie.LastValue.DATE < newSerie.LastValue.DATE)
                                {
                                    RefSerie = newSerie;
                                    instrument.SetDataSerie(DefaultDuration, newSerie);
                                }
                                else
                                {
                                    needDownload = false;
                                }
                            }
                        }
                        else
                        {
                            needDownload = false;
                        }
                    }
                    else
                    {
                        if (NeedDownload(instrument))
                        {
                            var dataSerie = DownloadData(instrument);
                            if (dataSerie != null)
                            {
                                instrument.SetDataSerie(DefaultDuration, dataSerie);
                            }
                        }
                    }
                }
            }

            InstrumentDownloadHistory.Save(HistoryFile, InstrumentsHistory);

            // Process custom initialization for the data provider
            PostInitDictionary(download);
        }

        protected virtual void PreInitDictionary(bool download) { }

        protected virtual void PostInitDictionary(bool download) { }

        protected abstract StockInstrument CreateInstrumentFromConfigLine(string line);

        public virtual void OpenInDataProvider(StockInstrument stockInstrument) { }

        public virtual bool Remove(StockInstrument instrument)
        {
            throw new NotImplementedException();
        }

        public void KeepOnyBars(StockInstrument instrument, Func<StockDailyValue, bool> predicate)
        {
            var dataSerie = instrument.GetDefaultDataSerie();
            if (dataSerie?.Values == null || dataSerie.Values.Length == 0)
                return;

            var nbBars = dataSerie.Values.Length;
            var remainingBars = dataSerie.Values.Where(predicate).ToArray();
            var filePath = GetInstrumentFilePath(instrument);
            var history = GetDownloadHistory(instrument);
            if (remainingBars.Length == 0)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                history.LastDate = DateTime.MinValue;
            }
            else
            {
                StockBar.Serialize(filePath, remainingBars);
                history.LastDate = remainingBars.Last().DATE;
            }
            history.DownloadDate = DateTime.MinValue;

            InstrumentDownloadHistory.Save(HistoryFile, InstrumentsHistory);

            instrument.ClearCache();
        }

        public virtual void ForceDownloadData(StockInstrument instrument)
        {
            var history = GetDownloadHistory(instrument);
            history.LastDate = history.DownloadDate = DateTime.MinValue;

            if (File.Exists(GetInstrumentFilePath(instrument)))
            {
                File.Delete(GetInstrumentFilePath(instrument));
            }

            instrument.ClearCache();

            this.DownloadData(instrument);

        }

        protected IDataHttpClient dataClient;
        public virtual DataSerie DownloadData(StockInstrument instrument)
        {
            try
            {
                if (this.dataClient == null)
                    throw new InvalidOperationException($"DataProvider {Provider} doesn't have a dataClient initialized");

                NotifyProgress($"Downloading {instrument.DisplayName}");

                DataSerie dataSerie = LoadData(instrument, BarDuration.H_1);
                DateTime startDate = dataSerie?.LastValue != null ? dataSerie.LastValue.DATE.Date : DateTime.MinValue;

                // Improvement check if last day is complete

                var newBars = this.dataClient.GetData(instrument, startDate);

                if (newBars != null && newBars.Length > 0)
                {
                    var finalBars = dataSerie == null ? newBars : dataSerie.Values.Where(v => v.DATE < startDate).Union(newBars).ToArray();

                    // Serialize todays bar only if time is greater that 22:10 pm
                    var isLate = DateTime.Now.TimeOfDay > new TimeSpan(22, 10, 0);
                    var serializeBars = finalBars.Where(b => b.DATE.Date < DateTime.Now.Date || isLate).ToArray();
                    StockBar.Serialize(GetInstrumentFilePath(instrument), serializeBars);

                    dataSerie = new DataSerie(instrument, DefaultDuration, finalBars);

                    instrument.SetDataSerie(DefaultDuration, dataSerie);

                    var history = GetDownloadHistory(instrument);
                    history.LastDate = dataSerie.LastValue.DATE;
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

        public abstract bool NeedDownload(StockInstrument instrument);

        public InstrumentDownloadHistory GetDownloadHistory(StockInstrument instrument)
        {
            var instrumentHistory = InstrumentsHistory.FirstOrDefault(h => h.Id == instrument.Id);
            if (instrumentHistory == null)
            {
                instrumentHistory = new InstrumentDownloadHistory(instrument, DateTime.MinValue, DateTime.MinValue);
                InstrumentsHistory.Add(instrumentHistory);
            }
            return instrumentHistory;
        }

        public bool IsMarketOpened(StockInstrument instrument)
        {
            if (MarketHoursTable.TryGetValue(instrument.Market, out var marketHours))
            {
                return marketHours.IsOpened;
            }
            return false;
        }

        public bool UpdateIntradayData(StockInstrument instrument)
        {
            StockLog.Write($"UpdateIntradayData: {instrument.DisplayName}");
            if (!IsMarketOpened(instrument))
                return false;

            return UpdateIntradayDataSpecific(instrument);
        }

        protected virtual bool UpdateIntradayDataSpecific(StockInstrument instrument)
        {
            if (GetDownloadHistory(instrument).DownloadDate.Add(shortDelay) > DateTime.Now)
                return false;

            return DownloadData(instrument) != null;
        }
    }
}
