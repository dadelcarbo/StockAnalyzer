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
    public enum DataProvider
    {
        All = 0,
        ABC,
        SaxoTurbo,
        Vontobel,
        SocGen,
        Yahoo,
        Cnn
    }
    public abstract class DataProviderBase : IDataProvider
    {
        static SortedDictionary<DataProvider, IDataProvider> DataProviders { get; } = new SortedDictionary<DataProvider, IDataProvider>()
        {
            {DataProvider.ABC, new AbcBourse.AbcDataProvider() },
            {DataProvider.SaxoTurbo, new SaxoTurbos.SaxoTurboDataProvider()},
            {DataProvider.Vontobel, new Vontobel.VontobelDataProvider()},
            {DataProvider.SocGen, new SocGen.SocGenDataProvider()},
            {DataProvider.Yahoo, new Yahoo.YahooDataProvider()},
            {DataProvider.Cnn, new Cnn.CnnDataProvider()}
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

        public void AddSplit(StockInstrument instrument, DateTime date, float before, float after)
        {
            var split = new StockSplit() { Instrument = instrument.Id, Date = date, Before = before, After = after };
            StockSplit.Splits.Add(split);
            StockSplit.Save();
        }

        protected string GetInstrumentFilePath(StockInstrument instrument)
        {
            // Use ID to prevent discrepencies between different market/currency price. (Isin could be ull for some data Provider).
            return string.IsNullOrEmpty(instrument.Symbol) ? Path.Combine(DataFolder, $"{instrument.Id}.dat") : Path.Combine(DataFolder, $"{instrument.Id}_{instrument.Symbol}.dat");
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

        protected abstract void PreInitDictionary(bool download);

        protected abstract void PostInitDictionary(bool download);

        protected abstract StockInstrument CreateInstrumentFromConfigLine(string line);

        public abstract void OpenInDataProvider(StockInstrument stockInstrument);

        public virtual void Remove(IEnumerable<StockInstrument> instruments)
        {
            if (File.Exists(ConfigFile))
            {
                var lines = File.ReadAllLines(ConfigFile).ToList();

                foreach (var instrument in instruments)
                {
                    lines.RemoveAll(l => l.Contains(instrument.Id));
                    StockDictionary.Instruments.Remove(instrument.Id);
                }

                File.WriteAllLines(ConfigFile, lines);
            }
        }

        public void KeepOnlyBars(StockInstrument instrument, Func<StockDailyValue, bool> predicate)
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
            StockLog.Write($"ForceDownloadData{instrument.DisplayName}");

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

                DataSerie dataSerie = LoadData(instrument, DefaultDuration);
                DateTime startDate = dataSerie?.LastValue != null ? dataSerie.LastValue.DATE.Date : DateTime.MinValue;

                // Improvement check if last day is complete
                var newBars = this.dataClient.GetData(instrument, startDate);

                if (newBars != null && newBars.Length > 0)
                {
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

        TimeSpan closeTime = new TimeSpan(22, 00, 0);
        TimeSpan openTime = new TimeSpan(08, 0, 0);
        TimeSpan longDelay = new TimeSpan(2, 0, 0);

        public virtual bool NeedDownload(StockInstrument instrument)
        {
            var history = GetDownloadHistory(instrument);
            if (history.LastDate == DateTime.MinValue)
                return true;
            if (history.DownloadDate.Add(longDelay) > DateTime.Now)
                return false;

            var now = DateTime.Now;
            var isLate = now.TimeOfDay > closeTime;
            var isEarly = now.TimeOfDay < openTime;

            if ((now.DayOfWeek == DayOfWeek.Friday && isLate) || // Check if week-end
                now.DayOfWeek == DayOfWeek.Saturday ||
                now.DayOfWeek == DayOfWeek.Sunday ||
                (now.DayOfWeek == DayOfWeek.Monday && isEarly))
            {
                if ((now.Date - history.LastDate.Date).TotalDays > 3)
                    return true;
                if (history.LastDate.DayOfWeek == DayOfWeek.Friday && history.LastDate.AddHours(1).TimeOfDay == closeTime)
                {
                    return false;
                }
            }
            else if (isEarly) // 
            {
                return history.LastDate.AddHours(1) != now.Date.AddDays(-1).Add(closeTime);
            }
            else if (isLate)
            {
                return history.LastDate.AddHours(1) != now.Date.Add(closeTime);
            }
            return true;
        }

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
            if (MarketHours.MarketHoursTable.TryGetValue(instrument.Market, out var marketHours))
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

        /// <summary>
        /// Default version is adapted intraday product such as SaxoTurbo, SocGen...
        /// </summary>
        /// <param name="instrument"></param>
        /// <returns></returns>
        protected virtual bool UpdateIntradayDataSpecific(StockInstrument instrument)
        {
            if (GetDownloadHistory(instrument).DownloadDate.Add(shortDelay) > DateTime.Now)
                return false;

            return DownloadData(instrument) != null;
        }
    }
}
