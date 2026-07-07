using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockData;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using StockAnalyzerApp.StockData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace StockAnalyzer.StockClasses
{
    public class StockDictionary : SortedDictionary<string, StockSerie>, IStockPriceProvider
    {
        static private SortedDictionary<string, StockInstrument> instruments;
        static public SortedDictionary<string, StockInstrument> Instruments => instruments ??= InitializeInstruments();

        static private SortedDictionary<string, StockInstrument> InitializeInstruments()
        {
            var instruments = new SortedDictionary<string, StockInstrument>();
            foreach (var serie in Instance.Values)
            {
                if (serie.DataProvider == StockDataProvider.ABC)
                {
                    continue;
                }
                var instrument = new StockInstrument(serie);
                instruments[instrument.Id] = instrument;
            }
            return instruments;
        }



        public DateTime ArchiveEndDate { get; private set; }

        public delegate void OnSerieEventDetectionDone();

        public static StockDictionary Instance { get; set; }

        public delegate void ReportProgressHandler(string progress);
        public event ReportProgressHandler ReportProgress;


        static public void Initialize(DateTime archiveEndDate)
        {
            if (Instance == null)
            {
                Instance = new StockDictionary(archiveEndDate);
            }
        }
        private StockDictionary(DateTime archiveEndDate)
        {
            this.ArchiveEndDate = archiveEndDate;

            StockPortfolio.StockPortfolio.PriceProvider = this;
        }

        private static List<Groups> validGroups = null;
        public List<Groups> GetValidGroups()
        {
            if (validGroups == null)
            {
                validGroups = new List<Groups>();
                foreach (Groups group in Enum.GetValues(typeof(Groups)))
                {
                    if (!Instruments.Values.Any(s => s.BelongsToGroup(group)) || group == Groups.ALL ||
                        group == Groups.NONE)
                    {
                        continue;
                    }
                    validGroups.Add(group);
                }
            }
            return validGroups;
        }

        #region BREADTH INDICATOR GENERATION

        //public bool GenerateAdvDeclSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        //{
        //    var refSerieName = indexName == "USA" ? "S&P 500" : "CAC40";
        //    if (!Instruments.TryGetValue(refSerieName, out var indiceSerie))
        //    {
        //        return false;
        //    }
        //    var indiceDataSerie = indiceSerie.GetDataSerie(BarDuration.Daily);
        //    if (indiceDataSerie == null || indiceDataSerie.Count == 0)
        //    {
        //        return false;
        //    }

        //    var indexComponents = Instruments.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
        //    if (indexComponents.Length == 0)
        //        return false;

        //    #region Load component series
        //    var componentDataSeries = new List<DataSerie>();
        //    foreach (var serie in indexComponents)
        //    {
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress("Loading data for " + serie.DisplayName);
        //        }
        //        var dataSerie = serie.GetDataSerie(BarDuration.Daily);
        //        if (dataSerie != null && dataSerie.Count > 50)
        //        {
        //            componentDataSeries.Add(dataSerie);
        //        }
        //    }
        //    #endregion


        //    DateTime lastIndiceDate = indiceDataSerie.LastValue.DATE;
        //    DateTime lastBreadthDate = DateTime.MinValue;

        //    // Check if serie has been already generated
        //    if (breadthSerie.Count > 0)
        //    {
        //        lastBreadthDate = breadthSerie.LastValue.DATE;
        //        if (lastIndiceDate <= lastBreadthDate)
        //        {
        //            // The breadth serie is up to date
        //            return true;
        //        }
        //        // Check if latest value is intraday data
        //        if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
        //        {
        //            // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
        //            if (lastIndiceDate.Date == lastBreadthDate.Date)
        //            {
        //                breadthSerie.RemoveLast();
        //                lastBreadthDate = breadthSerie.LastValue.DATE;
        //            }
        //        }
        //    }

        //    long vol;
        //    float val, count;
        //    foreach (var dailyBar in indiceDataSerie.Values)
        //    {
        //        if (dailyBar.DATE <= lastBreadthDate)
        //        {
        //            continue;
        //        }
        //        vol = 0; val = 0; count = 0;
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress(dailyBar.DATE.ToShortDateString());
        //        }

        //        int index = -1;
        //        foreach (var serie in componentDataSeries)
        //        {
        //            index = serie.IndexOf(dailyBar.DATE);
        //            if (index != -1)
        //            {
        //                vol += dailyBar.VOLUME;
        //                if (serie.Values[index].VARIATION >= 0)
        //                {
        //                    val++;
        //                }
        //                else
        //                {
        //                    val--;
        //                }
        //                count++;
        //            }
        //        }
        //        if (count != 0)
        //        {
        //            val /= count;
        //            breadthSerie.Add(dailyBar.DATE, new StockDailyValue(val, val, val, val, vol, dailyBar.DATE));
        //        }
        //    }
        //    if (breadthSerie.Count == 0)
        //    {
        //        //   this.Remove(breadthSerie.StockName);
        //    }
        //    else
        //    {
        //        if (!string.IsNullOrEmpty(destinationFolder))
        //        {
        //            breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
        //        }
        //        if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
        //        {
        //            breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
        //        }
        //    }
        //    return true;
        //}
        //public bool GenerateMcClellanSumSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        //{
        //    if (!this.ContainsKey($"AD.{indexName}"))
        //    {
        //        return false;
        //    }
        //    var adSerie = this[$"AD.{indexName}"];
        //    if (!adSerie.Initialise())
        //    {
        //        return false;
        //    }

        //    var ema19 = adSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(19);
        //    var ema39 = adSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(39);
        //    var val = ema19 - ema39;
        //    int i = 0;
        //    float sum = 0f;
        //    foreach (StockDailyValue value in adSerie.Values)
        //    {
        //        sum += val[i];
        //        breadthSerie.Add(value.DATE, new StockDailyValue(sum, sum, sum, sum, 0, value.DATE));
        //        i++;
        //    }
        //    return true;
        //}
        //public bool GenerateMcClellanSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        //{
        //    if (!this.ContainsKey($"AD.{indexName}"))
        //    {
        //        return false;
        //    }
        //    var adSerie = this[$"AD.{indexName}"];
        //    if (!adSerie.Initialise())
        //    {
        //        return false;
        //    }

        //    var ema19 = adSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(19);
        //    var ema39 = adSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(39);
        //    var val = ema19 - ema39;
        //    int i = 0;
        //    foreach (StockDailyValue value in adSerie.Values)
        //    {
        //        var mcClellan = val[i];
        //        breadthSerie.Add(value.DATE, new StockDailyValue(mcClellan, mcClellan, mcClellan, mcClellan, 0, value.DATE));
        //        i++;
        //    }
        //    return true;
        //}

        //public bool GenerateSTOKFBreadthSerie(StockSerie breadthSerie, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        //{
        //    int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);

        //    StockSerie indiceSerie = null;
        //    var refSerieName = indexName == "USA" ? "S&P 500" : "CAC40";
        //    if (this.ContainsKey(refSerieName))
        //    {
        //        indiceSerie = this[refSerieName];
        //        if (!indiceSerie.Initialise())
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //    StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
        //    if (indexComponents.Length == 0)
        //        return false;

        //    DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
        //    DateTime lastBreadthDate = DateTime.MinValue;

        //    // Check if serie has been already generated
        //    if (breadthSerie.Count > 0)
        //    {
        //        lastBreadthDate = breadthSerie.LastValue.DATE;
        //        if (lastIndiceDate <= lastBreadthDate)
        //        {
        //            // The breadth serie is up to date
        //            return true;
        //        }
        //        // Check if latest value is intraday data
        //        if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
        //        {
        //            // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
        //            if (lastIndiceDate.Date == lastBreadthDate.Date)
        //            {
        //                breadthSerie.RemoveLast();
        //                lastBreadthDate = breadthSerie.LastValue.DATE;
        //            }
        //        }
        //    }
        //    #region Load component series
        //    foreach (StockSerie serie in indexComponents)
        //    {
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress("Loading data for " + serie.StockName);
        //        }
        //        serie.Initialise();
        //        serie.BarDuration = barDuration;
        //    }
        //    #endregion
        //    long vol;
        //    float val, count;
        //    foreach (StockDailyValue value in indiceSerie.Values)
        //    {
        //        if (value.DATE <= lastBreadthDate)
        //        {
        //            continue;
        //        }
        //        vol = 0; val = 0; count = 0;
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress(value.DATE.ToShortDateString());
        //        }

        //        int index = -1;
        //        foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
        //        {
        //            index = serie.IndexOf(value.DATE);
        //            if (index != -1)
        //            {
        //                IStockIndicator indicator = serie.GetIndicator("STOKF(" + period + ",1,25,75)");
        //                val += indicator.Series[0][index];
        //                count++;
        //            }
        //        }
        //        if (count != 0)
        //        {
        //            val /= count;
        //            val = (val - 50f) * 0.02f;
        //            breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
        //        }
        //    }
        //    if (breadthSerie.Count == 0)
        //    {
        //        this.Remove(breadthSerie.StockName);
        //    }
        //    else
        //    {
        //        if (!string.IsNullOrEmpty(destinationFolder))
        //        {
        //            breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
        //        }
        //        if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
        //        {
        //            breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
        //        }
        //    }
        //    return true;
        //}
        //public bool GenerateSTOKBreadthSerie(StockSerie breadthSerie, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        //{
        //    int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);

        //    StockSerie indiceSerie = null;
        //    var refSerieName = indexName == "USA" ? "S&P 500" : "CAC40";
        //    if (this.ContainsKey(refSerieName))
        //    {
        //        indiceSerie = this[refSerieName];
        //        if (!indiceSerie.Initialise())
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //    StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
        //    if (indexComponents.Length == 0)
        //        return false;

        //    DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
        //    DateTime lastBreadthDate = DateTime.MinValue;

        //    // Check if serie has been already generated
        //    if (breadthSerie.Count > 0)
        //    {
        //        lastBreadthDate = breadthSerie.LastValue.DATE;
        //        if (lastIndiceDate <= lastBreadthDate)
        //        {
        //            // The breadth serie is up to date
        //            return true;
        //        }
        //        // Check if latest value is intraday data
        //        if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
        //        {
        //            // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
        //            if (lastIndiceDate.Date == lastBreadthDate.Date)
        //            {
        //                breadthSerie.RemoveLast();
        //                lastBreadthDate = breadthSerie.LastValue.DATE;
        //            }
        //        }
        //    }
        //    #region Load component series
        //    foreach (StockSerie serie in indexComponents)
        //    {
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress("Loading data for " + serie.StockName);
        //        }
        //        serie.Initialise();
        //        serie.BarDuration = barDuration;
        //    }
        //    #endregion
        //    long vol;
        //    float val, count;
        //    foreach (StockDailyValue value in indiceSerie.Values)
        //    {
        //        if (value.DATE <= lastBreadthDate)
        //        {
        //            continue;
        //        }
        //        vol = 0; val = 0; count = 0;
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress(value.DATE.ToShortDateString());
        //        }

        //        int index = -1;
        //        foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
        //        {
        //            index = serie.IndexOf(value.DATE);
        //            if (index != -1)
        //            {
        //                IStockIndicator indicator = serie.GetIndicator($"STOK({period})");
        //                val += indicator.Series[0][index];
        //                count++;
        //            }
        //        }
        //        if (count != 0)
        //        {
        //            val /= count;
        //            val = (val - 50f) * 0.02f;
        //            breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
        //        }
        //    }
        //    if (breadthSerie.Count == 0)
        //    {
        //        this.Remove(breadthSerie.StockName);
        //    }
        //    else
        //    {
        //        if (!string.IsNullOrEmpty(destinationFolder))
        //        {
        //            breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
        //        }
        //        if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
        //        {
        //            breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
        //        }
        //    }
        //    return true;
        //}
        //public bool GenerateRSIBreadthSerie(StockSerie breadthSerie, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        //{
        //    int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);

        //    StockSerie indiceSerie = null;
        //    var refSerieName = indexName == "USA" ? "S&P 500" : "CAC40";
        //    if (this.ContainsKey(refSerieName))
        //    {
        //        indiceSerie = this[refSerieName];
        //        if (!indiceSerie.Initialise())
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //    StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
        //    if (indexComponents.Length == 0)
        //        return false;

        //    DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
        //    DateTime lastBreadthDate = DateTime.MinValue;

        //    // Check if serie has been already generated
        //    if (breadthSerie.Count > 0)
        //    {
        //        lastBreadthDate = breadthSerie.LastValue.DATE;
        //        if (lastIndiceDate <= lastBreadthDate)
        //        {
        //            // The breadth serie is up to date
        //            return true;
        //        }
        //        // Check if latest value is intraday data
        //        if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
        //        {
        //            // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
        //            if (lastIndiceDate.Date == lastBreadthDate.Date)
        //            {
        //                breadthSerie.RemoveLast();
        //                lastBreadthDate = breadthSerie.LastValue.DATE;
        //            }
        //        }
        //    }
        //    #region Load component series
        //    foreach (StockSerie serie in indexComponents)
        //    {
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress("Loading data for " + serie.StockName);
        //        }
        //        serie.Initialise();
        //        serie.BarDuration = barDuration;
        //    }
        //    #endregion
        //    long vol;
        //    float val, count;
        //    foreach (StockDailyValue value in indiceSerie.Values)
        //    {
        //        if (value.DATE <= lastBreadthDate)
        //        {
        //            continue;
        //        }
        //        vol = 0; val = 0; count = 0;
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress(value.DATE.ToShortDateString());
        //        }

        //        int index = -1;
        //        foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
        //        {
        //            index = serie.IndexOf(value.DATE);
        //            if (index != -1)
        //            {
        //                IStockIndicator indicator = serie.GetIndicator($"RSI({period})");
        //                val += indicator.Series[0][index];
        //                count++;
        //            }
        //        }
        //        if (count != 0)
        //        {
        //            val /= count;
        //            val = (val - 50f) * 0.02f;
        //            breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
        //        }
        //    }
        //    if (breadthSerie.Count == 0)
        //    {
        //        this.Remove(breadthSerie.StockName);
        //    }
        //    else
        //    {
        //        if (!string.IsNullOrEmpty(destinationFolder))
        //        {
        //            breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
        //        }
        //        if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
        //        {
        //            breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
        //        }
        //    }
        //    return true;
        //}
        //public bool GenerateSTOKSBreadthSerie(StockSerie breadthSerie, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        //{
        //    int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);

        //    StockSerie indiceSerie = null;
        //    var refSerieName = indexName == "USA" ? "S&P 500" : "CAC40";
        //    if (this.ContainsKey(refSerieName))
        //    {
        //        indiceSerie = this[refSerieName];
        //        if (!indiceSerie.Initialise())
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //    StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
        //    if (indexComponents.Length == 0)
        //        return false;

        //    DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
        //    DateTime lastBreadthDate = DateTime.MinValue;

        //    // Check if serie has been already generated
        //    if (breadthSerie.Count > 0)
        //    {
        //        lastBreadthDate = breadthSerie.LastValue.DATE;
        //        if (lastIndiceDate <= lastBreadthDate)
        //        {
        //            // The breadth serie is up to date
        //            return true;
        //        }
        //        // Check if latest value is intraday data
        //        if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
        //        {
        //            // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
        //            if (lastIndiceDate.Date == lastBreadthDate.Date)
        //            {
        //                breadthSerie.RemoveLast();
        //                lastBreadthDate = breadthSerie.LastValue.DATE;
        //            }
        //        }
        //    }
        //    #region Load component series
        //    foreach (StockSerie serie in indexComponents)
        //    {
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress("Loading data for " + serie.StockName);
        //        }
        //        serie.Initialise();
        //        serie.BarDuration = barDuration;
        //    }
        //    #endregion
        //    long vol;
        //    float val, count;
        //    foreach (StockDailyValue value in indiceSerie.Values)
        //    {
        //        if (value.DATE <= lastBreadthDate)
        //        {
        //            continue;
        //        }
        //        vol = 0; val = 0; count = 0;
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress(value.DATE.ToShortDateString());
        //        }

        //        int index = -1;
        //        foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
        //        {
        //            index = serie.IndexOf(value.DATE);
        //            if (index != -1)
        //            {
        //                IStockIndicator indicator = serie.GetIndicator("STOKS(" + period + ",3,3)");
        //                val += indicator.Series[0][index];
        //                count++;
        //            }
        //        }
        //        if (count != 0)
        //        {
        //            val /= count;
        //            val = (val - 50f) * 0.02f;
        //            breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
        //        }
        //    }
        //    if (breadthSerie.Count == 0)
        //    {
        //        this.Remove(breadthSerie.StockName);
        //    }
        //    else
        //    {
        //        if (!string.IsNullOrEmpty(destinationFolder))
        //        {
        //            breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
        //        }
        //        if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
        //        {
        //            breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
        //        }
        //    }
        //    return true;
        //}
        //public bool GenerateEMABreadthSerie(StockSerie breadthSerie, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        //{
        //    int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);

        //    StockSerie indiceSerie = null;
        //    var refSerieName = indexName == "USA" ? "S&P 500" : "CAC40";
        //    if (this.ContainsKey(refSerieName))
        //    {
        //        indiceSerie = this[refSerieName];
        //        if (!indiceSerie.Initialise())
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //    StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
        //    if (indexComponents.Length == 0)
        //        return false;

        //    DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
        //    DateTime lastBreadthDate = DateTime.MinValue;

        //    // Check if serie has been already generated
        //    if (breadthSerie.Count > 0)
        //    {
        //        lastBreadthDate = breadthSerie.LastValue.DATE;
        //        if (lastIndiceDate <= lastBreadthDate)
        //        {
        //            // The breadth serie is up to date
        //            return true;
        //        }
        //        // Check if latest value is intraday data
        //        if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
        //        {
        //            // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
        //            if (lastIndiceDate.Date == lastBreadthDate.Date)
        //            {
        //                breadthSerie.RemoveLast();
        //                lastBreadthDate = breadthSerie.LastValue.DATE;
        //            }
        //        }
        //    }
        //    #region Load component series
        //    foreach (StockSerie serie in indexComponents)
        //    {
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress("Loading data for " + serie.StockName);
        //        }
        //        serie.Initialise();
        //        serie.BarDuration = barDuration;
        //    }
        //    #endregion
        //    long vol;
        //    float val, count;
        //    foreach (StockDailyValue value in indiceSerie.Values)
        //    {
        //        if (value.DATE <= lastBreadthDate)
        //        {
        //            continue;
        //        }
        //        vol = 0; val = 0; count = 0;
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress(value.DATE.ToShortDateString());
        //        }
        //        int index = -1;
        //        foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
        //        {
        //            index = serie.IndexOf(value.DATE);
        //            if (index != -1)
        //            {
        //                IStockEvent emaIndicator = serie.GetTrailStop("TRAILEMA(" + period + ",1)");
        //                if (emaIndicator != null && emaIndicator.Events[0].Count > 0)
        //                {
        //                    if (emaIndicator.Events[6][index])
        //                    {
        //                        val++;
        //                    }
        //                    count++;
        //                }
        //            }
        //        }
        //        if (count != 0)
        //        {
        //            val /= count;
        //            val = (val - 0.5f) * 2.0f;
        //            breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
        //        }
        //    }
        //    if (breadthSerie.Count == 0)
        //    {
        //        this.Remove(breadthSerie.StockName);
        //    }
        //    else
        //    {
        //        if (!string.IsNullOrEmpty(destinationFolder))
        //        {
        //            breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
        //        }
        //        if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
        //        {
        //            breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
        //        }
        //    }
        //    return true;
        //}
        //public bool GenerateHigherThanHLTrailSerie(StockSerie breadthSerie, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        //{
        //    int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);

        //    StockSerie indiceSerie = null;
        //    var refSerieName = indexName == "USA" ? "S&P 500" : "CAC40";
        //    if (this.ContainsKey(refSerieName))
        //    {
        //        indiceSerie = this[refSerieName];
        //        if (!indiceSerie.Initialise())
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //    StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
        //    if (indexComponents.Length == 0)
        //        return false;

        //    DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
        //    DateTime lastBreadthDate = DateTime.MinValue;

        //    // Check if serie has been already generated
        //    if (breadthSerie.Count > 0)
        //    {
        //        lastBreadthDate = breadthSerie.LastValue.DATE;
        //        if (lastIndiceDate <= lastBreadthDate)
        //        {
        //            // The breadth serie is up to date
        //            return true;
        //        }
        //        // Check if latest value is intraday data
        //        if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
        //        {
        //            // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
        //            if (lastIndiceDate.Date == lastBreadthDate.Date)
        //            {
        //                breadthSerie.RemoveLast();
        //                lastBreadthDate = breadthSerie.LastValue.DATE;
        //            }
        //        }
        //    }
        //    #region Load component series
        //    foreach (StockSerie serie in indexComponents)
        //    {
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress("Loading data for " + serie.StockName);
        //        }
        //        serie.Initialise();
        //        serie.BarDuration = barDuration;
        //    }
        //    #endregion
        //    long vol;
        //    float val, count;
        //    foreach (StockDailyValue value in indiceSerie.Values)
        //    {
        //        if (value.DATE <= lastBreadthDate)
        //        {
        //            continue;
        //        }
        //        vol = 0; val = 0; count = 0;
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress(value.DATE.ToShortDateString());
        //        }
        //        int index = -1;
        //        foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
        //        {
        //            index = serie.IndexOf(value.DATE);
        //            if (index != -1)
        //            {
        //                IStockTrailStop trailStop = serie.GetTrailStop("TRAILHL(" + period + ")");
        //                if (trailStop != null && trailStop.Series[0].Count > 0)
        //                {
        //                    if (float.IsNaN(trailStop.Series[1][index]))
        //                    {
        //                        val++;
        //                    }
        //                    count++;
        //                }
        //            }
        //        }
        //        if (count != 0)
        //        {
        //            val /= count;
        //            val = (val - 0.5f) * 2.0f;
        //            breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
        //        }
        //    }
        //    if (breadthSerie.Count == 0)
        //    {
        //        this.Remove(breadthSerie.StockName);
        //    }
        //    else
        //    {
        //        if (!string.IsNullOrEmpty(destinationFolder))
        //        {
        //            breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
        //        }
        //        if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
        //        {
        //            breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
        //        }
        //    }
        //    return true;
        //}
        //public bool GenerateIndiceEqualWeight(StockSerie breadthSerie, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        //{
        //    StockSerie indiceSerie = null;
        //    var refSerieName = indexName == "USA" ? "S&P 500" : "CAC40";
        //    if (this.ContainsKey(refSerieName))
        //    {
        //        indiceSerie = this[refSerieName];
        //        if (!indiceSerie.Initialise())
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //    StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
        //    if (indexComponents.Length == 0)
        //        return false;

        //    DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
        //    DateTime lastBreadthDate = DateTime.MinValue;

        //    float val = 1000, var;

        //    // Check if serie has been already generated
        //    if (breadthSerie.Count > 0)
        //    {
        //        lastBreadthDate = breadthSerie.LastValue.DATE;
        //        val = breadthSerie.LastValue.CLOSE;
        //        if (lastIndiceDate <= lastBreadthDate)
        //        {
        //            // The breadth serie is up to date
        //            return true;
        //        }
        //        // Check if latest value is intraday data
        //        if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
        //        {
        //            // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
        //            if (lastIndiceDate.Date == lastBreadthDate.Date)
        //            {
        //                breadthSerie.RemoveLast();
        //                lastBreadthDate = breadthSerie.LastValue.DATE;
        //            }
        //        }
        //    }
        //    #region Load component series
        //    foreach (StockSerie serie in indexComponents)
        //    {
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress("Loading data for " + serie.StockName);
        //        }
        //        serie.Initialise();
        //        serie.BarDuration = barDuration;
        //    }
        //    #endregion
        //    long vol;

        //    foreach (StockDailyValue value in indiceSerie.Values)
        //    {
        //        if (value.DATE <= lastBreadthDate)
        //        {
        //            continue;
        //        }
        //        vol = 0; var = 0;
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress(value.DATE.ToShortDateString());
        //        }

        //        int index = -1;
        //        foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50 && s.Keys.First() <= value.DATE))
        //        {
        //            index = serie.IndexOf(value.DATE);
        //            if (index != -1)
        //            {
        //                float dailyVar = serie.GetValue(StockDataType.VARIATION, index);
        //                var += dailyVar;
        //                vol++;
        //            }
        //        }
        //        if (vol != 0)
        //        {
        //            val *= (1 + var / vol);
        //            breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
        //        }
        //    }
        //    if (breadthSerie.Count == 0)
        //    {
        //        this.Remove(breadthSerie.StockName);
        //    }
        //    else
        //    {
        //        if (!string.IsNullOrEmpty(destinationFolder))
        //        {
        //            breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_" + breadthSerie.StockGroup + ".csv", ArchiveEndDate, false);
        //        }
        //        if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
        //        {
        //            breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_" + breadthSerie.StockGroup + ".csv", ArchiveEndDate, true);
        //        }
        //    }
        //    return true;
        //}
        //public bool GenerateIndiceBest(string speedIndicator, StockSerie breadthSerie, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        //{
        //    int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);
        //    int nbStocks = 10;

        //    StockSerie indiceSerie = null;
        //    var refSerieName = indexName == "USA" ? "S&P 500" : "CAC40";
        //    if (this.ContainsKey(refSerieName))
        //    {
        //        indiceSerie = this[refSerieName];
        //        if (!indiceSerie.Initialise())
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //    DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
        //    DateTime lastBreadthDate = DateTime.MinValue;

        //    float val = 1000, var;

        //    // Check if serie has been already generated
        //    if (breadthSerie.Count > 0)
        //    {
        //        lastBreadthDate = breadthSerie.LastValue.DATE;
        //        val = breadthSerie.LastValue.CLOSE;
        //        if (lastIndiceDate <= lastBreadthDate)
        //        {
        //            // The breadth serie is up to date
        //            return true;
        //        }
        //        // Check if latest value is intraday data
        //        if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
        //        {
        //            // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
        //            if (lastIndiceDate.Date == lastBreadthDate.Date)
        //            {
        //                breadthSerie.RemoveLast();
        //                lastBreadthDate = breadthSerie.LastValue.DATE;
        //            }
        //        }
        //    }

        //    StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName) && s.Initialise() && s.Count > 50).ToArray();
        //    if (indexComponents.Length == 0)
        //        return false;

        //    #region Load component series
        //    foreach (StockSerie serie in indexComponents)
        //    {
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress("Loading data for " + serie.StockName);
        //        }
        //        serie.Initialise();
        //        serie.BarDuration = barDuration;
        //        serie.GetIndicator($"{speedIndicator}({period})");
        //    }
        //    #endregion
        //    long vol;

        //    int index;
        //    DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
        //    Calendar cal = dfi.Calendar;

        //    List<StockSerie> bestSeries = new List<StockSerie>();
        //    int previousWeekNumber = 52;
        //    foreach (StockDailyValue value in indiceSerie.Values)
        //    {
        //        if (value.DATE <= lastBreadthDate)
        //        {
        //            continue;
        //        }
        //        vol = 0; var = 0;
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress(value.DATE.ToShortDateString());
        //        }
        //        int weekNumber = cal.GetWeekOfYear(value.DATE, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
        //        if (weekNumber != previousWeekNumber)
        //        {
        //            bestSeries.Clear();
        //            List<Tuple<StockSerie, float>> tuples = new List<Tuple<StockSerie, float>>();

        //            // Reselect list of stock at the beging of the month
        //            foreach (StockSerie serie in indexComponents.Where(s => s.Keys.First() <= value.DATE))
        //            {
        //                index = serie.IndexOf(value.DATE);
        //                if (index >= 1)
        //                {
        //                    if (serie.GetSerie(StockDataType.EXCHANGED)[index - 1] > 250000f)
        //                    {
        //                        float speed = serie.GetIndicator($"{speedIndicator}({period})").Series[0][index - 1];
        //                        if (speed > 0)
        //                        {
        //                            tuples.Add(new Tuple<StockSerie, float>(serie, speed));
        //                        }
        //                    }
        //                }
        //            }
        //            bestSeries = tuples.OrderByDescending(t => t.Item2).Select(t => t.Item1).Take(nbStocks).ToList();
        //            previousWeekNumber = weekNumber;
        //        }
        //        foreach (StockSerie serie in bestSeries)
        //        {
        //            index = serie.IndexOf(value.DATE);
        //            if (index != -1)
        //            {
        //                float dailyVar = serie.GetValue(StockDataType.VARIATION, index);
        //                var += dailyVar;
        //                vol++;
        //            }
        //        }
        //        if (vol != 0)
        //        {
        //            val *= (1 + var / nbStocks);
        //        }
        //        breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
        //    }
        //    if (breadthSerie.Count == 0)
        //    {
        //        this.Remove(breadthSerie.StockName);
        //    }
        //    else
        //    {
        //        if (!string.IsNullOrEmpty(destinationFolder))
        //        {
        //            breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_" + breadthSerie.StockGroup + ".csv", ArchiveEndDate, false);
        //        }
        //        if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
        //        {
        //            breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_" + breadthSerie.StockGroup + ".csv", ArchiveEndDate, true);
        //        }
        //    }
        //    return true;
        //}

        //public bool GenerateHigherThanMMSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        //{
        //    int period = int.Parse(breadthSerie.StockName.Split('.')[0].Replace("MM", ""));

        //    StockSerie indiceSerie = null;
        //    var refSerieName = indexName == "USA" ? "S&P 500" : "CAC40";
        //    if (this.ContainsKey(refSerieName))
        //    {
        //        indiceSerie = this[refSerieName];
        //        if (!indiceSerie.Initialise())
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //    StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
        //    if (indexComponents.Length == 0)
        //        return false;

        //    DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
        //    DateTime lastBreadthDate = DateTime.MinValue;

        //    // Check if serie has been already generated
        //    if (breadthSerie.Count > 0)
        //    {
        //        lastBreadthDate = breadthSerie.LastValue.DATE;
        //        if (lastIndiceDate <= lastBreadthDate)
        //        {
        //            // The breadth serie is up to date
        //            return true;
        //        }
        //        // Check if latest value is intraday data
        //        if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
        //        {
        //            // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
        //            if (lastIndiceDate.Date == lastBreadthDate.Date)
        //            {
        //                breadthSerie.RemoveLast();
        //                lastBreadthDate = breadthSerie.LastValue.DATE;
        //            }
        //        }
        //    }
        //    #region Load component series
        //    foreach (StockSerie serie in indexComponents)
        //    {
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress("Loading data for " + serie.StockName);
        //        }
        //        serie.Initialise();
        //    }
        //    #endregion
        //    long vol;
        //    float val, count;
        //    foreach (StockDailyValue value in indiceSerie.Values)
        //    {
        //        if (value.DATE <= lastBreadthDate)
        //        {
        //            continue;
        //        }
        //        vol = 0; val = 0; count = 0;
        //        if (this.ReportProgress != null)
        //        {
        //            this.ReportProgress(value.DATE.ToShortDateString());
        //        }
        //        int index = -1;
        //        foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
        //        {
        //            index = serie.IndexOf(value.DATE);
        //            if (index != -1)
        //            {
        //                FloatSerie mmSerie = serie.GetIndicator("EMA(" + period.ToString() + ")").Series[0];
        //                if (serie.GetValue(StockDataType.CLOSE, index) >= mmSerie[index])
        //                {
        //                    val++;
        //                }
        //                count++;
        //            }
        //        }
        //        if (count != 0)
        //        {
        //            val /= count;
        //            breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
        //        }
        //    }
        //    if (breadthSerie.Count == 0)
        //    {
        //        this.Remove(breadthSerie.StockName);
        //    }
        //    else
        //    {
        //        if (!string.IsNullOrEmpty(destinationFolder))
        //        {
        //            breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
        //        }
        //        if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
        //        {
        //            breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
        //        }
        //    }
        //    return true;
        //}

        #endregion
        #region ANALYSIS SERIALISATION
        static public void ReadAnalysisFromXml(System.Xml.XmlReader reader)
        {
            reader.Read(); // Skip Header
            reader.Read(); // Skip StockAnalysisList
            reader.ReadStartElement(); // Start StockAnalysisItem
            while (reader.Name == "StockAnalysisItem")
            {
                string stockName = reader.GetAttribute("StockName");
                if (stockName != null)
                {
                    if (Instruments.TryGetValue(stockName, out var instrument))
                    {
                        instrument.ReadAnalysisFromXml(reader);
                    }
                    else
                    {
                        reader.ReadToFollowing("StockAnalysisItem");
                    }
                }
                else
                {
                    reader.ReadToFollowing("StockAnalysisItem");
                }
            }
        }
        static public void WriteAnalysisToXml(System.Xml.XmlWriter writer)
        {
            // Serialize Flat Attributes
            writer.WriteStartElement("StockAnalysisList");

            foreach (var instrument in Instruments.Values.Where(s => !s.StockAnalysis.IsEmpty()).OrderBy(s => s.Group))
            {
                writer.WriteStartElement("StockAnalysisItem");
                writer.WriteAttributeString("StockName", instrument.Id);

                instrument.WriteAnalysisToXml(writer);

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.Flush();
        }
        #endregion

        public float GetClosingPrice(string instrumentName, DateTime date, BarDuration duration)
        {
            if (Instruments.TryGetValue(instrumentName, out var instrument))
            {
                var dataSerie = instrument.GetDataSerie(duration);

                var index = dataSerie.IndexOfFirstLowerOrEquals(date);
                if (index != -1)
                {
                    return dataSerie.Values[index].CLOSE;
                }

            }
            return 0f;
        }
        public float GetLastClosingPrice(string instrumentName)
        {
            if (Instruments.TryGetValue(instrumentName, out var instrument))
            {
                var dataSerie = instrument.GetDefaultDataSerie();

                if (dataSerie != null && dataSerie.Count > 0)
                {
                    return dataSerie.LastValue.CLOSE;
                }
            }
            return 0f;
        }

        public static StockInstrument GetInstrument(string id, string isin = null)
        {
            if (Instruments.ContainsKey(id))
            {
                return Instruments[id];
            }
            return string.IsNullOrEmpty(isin) ? null : Instruments.Values.FirstOrDefault(s => s.Isin == isin);
        }
        Stopwatch sw;
        public List<StockAlert> MatchAlert(StockAlertDef alertDef)
        {
            using MethodLogger ml = new MethodLogger(this, true, $"AlertDef: {alertDef.Title}");
            sw = Stopwatch.StartNew();

            var alerts = new List<StockAlert>();
            try
            {
                foreach (var instrument in Instruments.Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(alertDef.Group)))
                {
                    var alert = instrument.MatchAlertDef(alertDef);
                    if (alert != null)
                    {
                        alerts.Add(alert);
                    }
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            sw.Stop();
            StockLog.Write($"MatchAlert Duration: {sw.Elapsed} for {alertDef.BarDuration} {alertDef.Group} {alertDef.Title}");

            return alerts;
        }

        public static DataSerie GetDataSerie(string instrumentId, BarDuration duration)
        {
            var instrument = StockDictionary.Instruments[instrumentId];
            if (instrument == null)
            {
                return null;
            }
            return instrument.GetDataSerie(duration);

        }
    }
}
