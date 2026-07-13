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
    public abstract class DataProviderBase : IDataProvider
    {
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
            {DataProvider.SaxoTurbo, new SaxoTurbos.SaxoTurboDataProvider()}
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
                    var history = GetDownloadHistory(instrument);
                    if (RefSerie == null)
                    {
                        if (NeedDownload(instrument, history))
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
                                history.LastDate = RefSerie.LastValue.DATE;
                                history.DownloadDate = DateTime.Now;
                            }
                        }
                        else
                        {
                            needDownload = false;
                        }
                    }
                    else
                    {
                        if (NeedDownload(instrument, history))
                        {
                            var dataSerie = DownloadData(instrument);
                            if (dataSerie != null)
                            {
                                instrument.SetDataSerie(DefaultDuration, dataSerie);

                                history.LastDate = dataSerie.LastValue.DATE;
                                history.DownloadDate = DateTime.Now;
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

        public virtual bool RemoveEntry(StockInstrument instrument)
        {
            throw new NotImplementedException();
        }

        public void ApplyTrimBefore(StockInstrument instrument, DateTime upToDate)
        {
            throw new NotImplementedException();
        }

        public abstract void ForceDownloadData(StockInstrument instrument);

        public abstract DataSerie DownloadData(StockInstrument instrument);

        public abstract bool NeedDownload(StockInstrument instrument, InstrumentDownloadHistory history = null);

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
    }
}
