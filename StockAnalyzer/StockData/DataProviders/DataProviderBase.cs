using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using StockAnalyzerApp.StockData;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
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

        protected string DataFolder { get; private set; }
        protected string ConfigFile { get; private set; }

        public static event DownloadingEventHandler DownloadStarted;

        public void AddSplit(StockInstrument instrument, DateTime date, float before, float after)
        {
            throw new NotImplementedException();
        }

        private string GetInstrumentFilePath(StockInstrument instrument)
        {
            return string.IsNullOrEmpty(instrument.Symbol) ? Path.Combine(DataFolder, $"{instrument.Isin}.dat") : Path.Combine(DataFolder, $"{instrument.Isin}_{instrument.Symbol}.dat");
        }

        public DataSerie LoadData(StockInstrument instrument, BarDuration barDuration)
        {
            if (barDuration != DefaultDuration)
            {
                return instrument.GetDefaultDataSerie()?.ConvertToDurationFromDaily(barDuration);
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

        public void InitDictionary(bool download)
        {
            this.DataFolder = Path.Combine(Folders.DataFolder, Provider.ToString());
            this.ConfigFile = Path.Combine(Folders.PersonalFolder, $"Provider_{Provider}.cfg");

            if (!Directory.Exists(DataFolder))
            {
                Directory.CreateDirectory(DataFolder);
            }
            if (!File.Exists(ConfigFile))
            {
                File.Create(ConfigFile).Dispose();
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
            }

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

        public bool ForceDownloadData(StockInstrument instrument)
        {
            throw new NotImplementedException();
        }
    }
}
