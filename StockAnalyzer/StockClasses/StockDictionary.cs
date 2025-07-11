﻿using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace StockAnalyzer.StockClasses
{
    public class StockDictionary : SortedDictionary<string, StockSerie>, IStockPriceProvider
    {
        public DateTime ArchiveEndDate { get; private set; }

        public delegate void OnSerieEventDetectionDone();

        public static StockDictionary Instance { get; set; }

        public delegate void ReportProgressHandler(string progress);
        public event ReportProgressHandler ReportProgress;

        public StockDictionary(DateTime archiveEndDate)
        {
            Instance = this;
            this.ArchiveEndDate = archiveEndDate;

            StockPortfolio.StockPortfolio.PriceProvider = this;
        }

        private static List<StockSerie.Groups> validGroups = null;
        public List<StockSerie.Groups> GetValidGroups()
        {
            if (validGroups == null)
            {
                validGroups = new List<StockSerie.Groups>();
                foreach (StockSerie.Groups group in Enum.GetValues(typeof(StockSerie.Groups)))
                {
                    if (!this.Values.Any(s => s.BelongsToGroup(group)) || group == StockSerie.Groups.ALL ||
                        group == StockSerie.Groups.NONE)
                    {
                        continue;
                    }
                    validGroups.Add(group);
                }
            }
            return validGroups;
        }

        #region RANK Calculation
        public void CalculateRank(StockSerie.Groups group, string indicatorName, BarDuration duration, string destinationFolder)
        {
            var groupsSeries = StockDictionary.Instance.Values.Where(s => s.BelongsToGroup(group) && s.Initialise()).ToList();

            var dico = new SortedDictionary<DateTime, List<Tuple<StockSerie, float>>>();

            foreach (var serie in groupsSeries)
            {
                serie.BarDuration = duration;

                var indicatorSerie = serie.GetIndicator(indicatorName).Series[0];
                int i = 0;
                foreach (var date in serie.Keys)
                {
                    if (!dico.ContainsKey(date))
                    {
                        dico.Add(date, new List<Tuple<StockSerie, float>>());
                    }
                    dico[date].Add(new Tuple<StockSerie, float>(serie, indicatorSerie[i++]));
                }
            }

            var ranks = new SortedDictionary<DateTime, List<Tuple<StockSerie, float>>>();
            foreach (var item in dico)
            {
                float rank = 0;
                var rankList = new List<Tuple<StockSerie, float>>();
                ranks.Add(item.Key, rankList);
                foreach (var ranked in item.Value.OrderBy(r => r.Item2).Select(r => r.Item1))
                {
                    rankList.Add(new Tuple<StockSerie, float>(ranked, rank * 100f / (item.Value.Count - 1f)));
                    rank++;
                }
            }

            // Persist
            string fileName = Path.Combine(destinationFolder, $"{group}_{indicatorName}_{duration}.txt");
            using StreamWriter sw = new StreamWriter(fileName, false);
            foreach (var serie in groupsSeries)
            {
                var serieRanks = ranks.Select(r => r.Value.FirstOrDefault(l => l.Item1 == serie)?.Item2).Where(r => r != null).Select(r => r.Value.ToString("G4"));
                sw.WriteLine(serie.StockName + "|" + serieRanks.Aggregate((i, j) => i + "|" + j));
            }
            //foreach (var dailyValue in stockSerie.Values.Reverse().Take(1))
            //{
            //    var rank = new List<Tuple<string, float>>();
            //    foreach (var serie in groupsSeries)
            //    {
            //        var index = serie.IndexOf(dailyValue.DATE);
            //        if (index != -1)
            //        {
            //            var indicatorValue = serie.GetIndicator(indicatorName).Series[0][index];
            //            rank.Add(new Tuple<string, float>(serie.StockName, indicatorValue));
            //        }
            //    }
            //    var orderedRank = rank.OrderBy(r => r.Item2).Select(r => r.Item1).ToList().IndexOf(stockSerie.StockName) + 1f;
            //    rankSerie[count++] = orderedRank / rank.Count;
            //}


        }
        #endregion
        #region BREADTH INDICATOR GENERATION
        public bool GenerateAdvDeclSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            StockSerie indiceSerie = null;
            var refSerieName = indexName == "USA" ? "SP500" : "CAC40";
            if (this.ContainsKey(refSerieName))
            {
                indiceSerie = this[refSerieName];
                if (!indiceSerie.Initialise())
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
            if (indexComponents.Length == 0)
                return false;

            DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
            DateTime lastBreadthDate = DateTime.MinValue;

            // Check if serie has been already generated
            if (breadthSerie.Count > 0)
            {
                lastBreadthDate = breadthSerie.LastValue.DATE;
                if (lastIndiceDate <= lastBreadthDate)
                {
                    // The breadth serie is up to date
                    return true;
                }
                // Check if latest value is intraday data
                if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
                {
                    // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
                    if (lastIndiceDate.Date == lastBreadthDate.Date)
                    {
                        breadthSerie.RemoveLast();
                        lastBreadthDate = breadthSerie.LastValue.DATE;
                    }
                }
            }
            #region Load component series
            foreach (StockSerie serie in indexComponents)
            {
                if (this.ReportProgress != null)
                {
                    this.ReportProgress("Loading data for " + serie.StockName);
                }
                serie.Initialise();
            }
            #endregion
            long vol;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = serie.IndexOf(value.DATE);
                    if (index != -1)
                    {
                        vol += value.VOLUME;
                        if (serie.ValueArray[index].VARIATION >= 0)
                        {
                            val++;
                        }
                        else
                        {
                            val--;
                        }
                        count++;
                    }
                }
                if (count != 0)
                {
                    val /= count;
                    breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
                }
            }
            if (breadthSerie.Count == 0)
            {
                //   this.Remove(breadthSerie.StockName);
            }
            else
            {
                if (!string.IsNullOrEmpty(destinationFolder))
                {
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateMcClellanSumSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            if (!this.ContainsKey($"AD.{indexName}"))
            {
                return false;
            }
            var adSerie = this[$"AD.{indexName}"];
            if (!adSerie.Initialise())
            {
                return false;
            }

            var ema19 = adSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(19);
            var ema39 = adSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(39);
            var val = ema19 - ema39;
            int i = 0;
            float sum = 0f;
            foreach (StockDailyValue value in adSerie.Values)
            {
                sum += val[i];
                breadthSerie.Add(value.DATE, new StockDailyValue(sum, sum, sum, sum, 0, value.DATE));
                i++;
            }
            return true;
        }
        public bool GenerateMcClellanSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            if (!this.ContainsKey($"AD.{indexName}"))
            {
                return false;
            }
            var adSerie = this[$"AD.{indexName}"];
            if (!adSerie.Initialise())
            {
                return false;
            }

            var ema19 = adSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(19);
            var ema39 = adSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(39);
            var val = ema19 - ema39;
            int i = 0;
            foreach (StockDailyValue value in adSerie.Values)
            {
                var mcClellan = val[i];
                breadthSerie.Add(value.DATE, new StockDailyValue(mcClellan, mcClellan, mcClellan, mcClellan, 0, value.DATE));
                i++;
            }
            return true;
        }

        public bool GenerateSTOKFBreadthSerie(StockSerie breadthSerie, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);

            StockSerie indiceSerie = null;
            var refSerieName = indexName == "USA" ? "SP500" : "CAC40";
            if (this.ContainsKey(refSerieName))
            {
                indiceSerie = this[refSerieName];
                if (!indiceSerie.Initialise())
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
            if (indexComponents.Length == 0)
                return false;

            DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
            DateTime lastBreadthDate = DateTime.MinValue;

            // Check if serie has been already generated
            if (breadthSerie.Count > 0)
            {
                lastBreadthDate = breadthSerie.LastValue.DATE;
                if (lastIndiceDate <= lastBreadthDate)
                {
                    // The breadth serie is up to date
                    return true;
                }
                // Check if latest value is intraday data
                if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
                {
                    // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
                    if (lastIndiceDate.Date == lastBreadthDate.Date)
                    {
                        breadthSerie.RemoveLast();
                        lastBreadthDate = breadthSerie.LastValue.DATE;
                    }
                }
            }
            #region Load component series
            foreach (StockSerie serie in indexComponents)
            {
                if (this.ReportProgress != null)
                {
                    this.ReportProgress("Loading data for " + serie.StockName);
                }
                serie.Initialise();
                serie.BarDuration = barDuration;
            }
            #endregion
            long vol;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = serie.IndexOf(value.DATE);
                    if (index != -1)
                    {
                        IStockIndicator indicator = serie.GetIndicator("STOKF(" + period + ",1,25,75)");
                        val += indicator.Series[0][index];
                        count++;
                    }
                }
                if (count != 0)
                {
                    val /= count;
                    val = (val - 50f) * 0.02f;
                    breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
                }
            }
            if (breadthSerie.Count == 0)
            {
                this.Remove(breadthSerie.StockName);
            }
            else
            {
                if (!string.IsNullOrEmpty(destinationFolder))
                {
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateSTOKBreadthSerie(StockSerie breadthSerie, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);

            StockSerie indiceSerie = null;
            var refSerieName = indexName == "USA" ? "SP500" : "CAC40";
            if (this.ContainsKey(refSerieName))
            {
                indiceSerie = this[refSerieName];
                if (!indiceSerie.Initialise())
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
            if (indexComponents.Length == 0)
                return false;

            DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
            DateTime lastBreadthDate = DateTime.MinValue;

            // Check if serie has been already generated
            if (breadthSerie.Count > 0)
            {
                lastBreadthDate = breadthSerie.LastValue.DATE;
                if (lastIndiceDate <= lastBreadthDate)
                {
                    // The breadth serie is up to date
                    return true;
                }
                // Check if latest value is intraday data
                if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
                {
                    // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
                    if (lastIndiceDate.Date == lastBreadthDate.Date)
                    {
                        breadthSerie.RemoveLast();
                        lastBreadthDate = breadthSerie.LastValue.DATE;
                    }
                }
            }
            #region Load component series
            foreach (StockSerie serie in indexComponents)
            {
                if (this.ReportProgress != null)
                {
                    this.ReportProgress("Loading data for " + serie.StockName);
                }
                serie.Initialise();
                serie.BarDuration = barDuration;
            }
            #endregion
            long vol;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = serie.IndexOf(value.DATE);
                    if (index != -1)
                    {
                        IStockIndicator indicator = serie.GetIndicator($"STOK({period})");
                        val += indicator.Series[0][index];
                        count++;
                    }
                }
                if (count != 0)
                {
                    val /= count;
                    val = (val - 50f) * 0.02f;
                    breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
                }
            }
            if (breadthSerie.Count == 0)
            {
                this.Remove(breadthSerie.StockName);
            }
            else
            {
                if (!string.IsNullOrEmpty(destinationFolder))
                {
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateRSIBreadthSerie(StockSerie breadthSerie, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);

            StockSerie indiceSerie = null;
            var refSerieName = indexName == "USA" ? "SP500" : "CAC40";
            if (this.ContainsKey(refSerieName))
            {
                indiceSerie = this[refSerieName];
                if (!indiceSerie.Initialise())
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
            if (indexComponents.Length == 0)
                return false;

            DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
            DateTime lastBreadthDate = DateTime.MinValue;

            // Check if serie has been already generated
            if (breadthSerie.Count > 0)
            {
                lastBreadthDate = breadthSerie.LastValue.DATE;
                if (lastIndiceDate <= lastBreadthDate)
                {
                    // The breadth serie is up to date
                    return true;
                }
                // Check if latest value is intraday data
                if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
                {
                    // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
                    if (lastIndiceDate.Date == lastBreadthDate.Date)
                    {
                        breadthSerie.RemoveLast();
                        lastBreadthDate = breadthSerie.LastValue.DATE;
                    }
                }
            }
            #region Load component series
            foreach (StockSerie serie in indexComponents)
            {
                if (this.ReportProgress != null)
                {
                    this.ReportProgress("Loading data for " + serie.StockName);
                }
                serie.Initialise();
                serie.BarDuration = barDuration;
            }
            #endregion
            long vol;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = serie.IndexOf(value.DATE);
                    if (index != -1)
                    {
                        IStockIndicator indicator = serie.GetIndicator($"RSI({period})");
                        val += indicator.Series[0][index];
                        count++;
                    }
                }
                if (count != 0)
                {
                    val /= count;
                    val = (val - 50f) * 0.02f;
                    breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
                }
            }
            if (breadthSerie.Count == 0)
            {
                this.Remove(breadthSerie.StockName);
            }
            else
            {
                if (!string.IsNullOrEmpty(destinationFolder))
                {
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateSTOKSBreadthSerie(StockSerie breadthSerie, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);

            StockSerie indiceSerie = null;
            var refSerieName = indexName == "USA" ? "SP500" : "CAC40";
            if (this.ContainsKey(refSerieName))
            {
                indiceSerie = this[refSerieName];
                if (!indiceSerie.Initialise())
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
            if (indexComponents.Length == 0)
                return false;

            DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
            DateTime lastBreadthDate = DateTime.MinValue;

            // Check if serie has been already generated
            if (breadthSerie.Count > 0)
            {
                lastBreadthDate = breadthSerie.LastValue.DATE;
                if (lastIndiceDate <= lastBreadthDate)
                {
                    // The breadth serie is up to date
                    return true;
                }
                // Check if latest value is intraday data
                if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
                {
                    // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
                    if (lastIndiceDate.Date == lastBreadthDate.Date)
                    {
                        breadthSerie.RemoveLast();
                        lastBreadthDate = breadthSerie.LastValue.DATE;
                    }
                }
            }
            #region Load component series
            foreach (StockSerie serie in indexComponents)
            {
                if (this.ReportProgress != null)
                {
                    this.ReportProgress("Loading data for " + serie.StockName);
                }
                serie.Initialise();
                serie.BarDuration = barDuration;
            }
            #endregion
            long vol;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = serie.IndexOf(value.DATE);
                    if (index != -1)
                    {
                        IStockIndicator indicator = serie.GetIndicator("STOKS(" + period + ",3,3)");
                        val += indicator.Series[0][index];
                        count++;
                    }
                }
                if (count != 0)
                {
                    val /= count;
                    val = (val - 50f) * 0.02f;
                    breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
                }
            }
            if (breadthSerie.Count == 0)
            {
                this.Remove(breadthSerie.StockName);
            }
            else
            {
                if (!string.IsNullOrEmpty(destinationFolder))
                {
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateEMABreadthSerie(StockSerie breadthSerie, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);

            StockSerie indiceSerie = null;
            var refSerieName = indexName == "USA" ? "SP500" : "CAC40";
            if (this.ContainsKey(refSerieName))
            {
                indiceSerie = this[refSerieName];
                if (!indiceSerie.Initialise())
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
            if (indexComponents.Length == 0)
                return false;

            DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
            DateTime lastBreadthDate = DateTime.MinValue;

            // Check if serie has been already generated
            if (breadthSerie.Count > 0)
            {
                lastBreadthDate = breadthSerie.LastValue.DATE;
                if (lastIndiceDate <= lastBreadthDate)
                {
                    // The breadth serie is up to date
                    return true;
                }
                // Check if latest value is intraday data
                if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
                {
                    // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
                    if (lastIndiceDate.Date == lastBreadthDate.Date)
                    {
                        breadthSerie.RemoveLast();
                        lastBreadthDate = breadthSerie.LastValue.DATE;
                    }
                }
            }
            #region Load component series
            foreach (StockSerie serie in indexComponents)
            {
                if (this.ReportProgress != null)
                {
                    this.ReportProgress("Loading data for " + serie.StockName);
                }
                serie.Initialise();
                serie.BarDuration = barDuration;
            }
            #endregion
            long vol;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = serie.IndexOf(value.DATE);
                    if (index != -1)
                    {
                        IStockEvent emaIndicator = serie.GetTrailStop("TRAILEMA(" + period + ",1)");
                        if (emaIndicator != null && emaIndicator.Events[0].Count > 0)
                        {
                            if (emaIndicator.Events[6][index])
                            {
                                val++;
                            }
                            count++;
                        }
                    }
                }
                if (count != 0)
                {
                    val /= count;
                    val = (val - 0.5f) * 2.0f;
                    breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
                }
            }
            if (breadthSerie.Count == 0)
            {
                this.Remove(breadthSerie.StockName);
            }
            else
            {
                if (!string.IsNullOrEmpty(destinationFolder))
                {
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateHigherThanHLTrailSerie(StockSerie breadthSerie, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);

            StockSerie indiceSerie = null;
            var refSerieName = indexName == "USA" ? "SP500" : "CAC40";
            if (this.ContainsKey(refSerieName))
            {
                indiceSerie = this[refSerieName];
                if (!indiceSerie.Initialise())
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
            if (indexComponents.Length == 0)
                return false;

            DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
            DateTime lastBreadthDate = DateTime.MinValue;

            // Check if serie has been already generated
            if (breadthSerie.Count > 0)
            {
                lastBreadthDate = breadthSerie.LastValue.DATE;
                if (lastIndiceDate <= lastBreadthDate)
                {
                    // The breadth serie is up to date
                    return true;
                }
                // Check if latest value is intraday data
                if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
                {
                    // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
                    if (lastIndiceDate.Date == lastBreadthDate.Date)
                    {
                        breadthSerie.RemoveLast();
                        lastBreadthDate = breadthSerie.LastValue.DATE;
                    }
                }
            }
            #region Load component series
            foreach (StockSerie serie in indexComponents)
            {
                if (this.ReportProgress != null)
                {
                    this.ReportProgress("Loading data for " + serie.StockName);
                }
                serie.Initialise();
                serie.BarDuration = barDuration;
            }
            #endregion
            long vol;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = serie.IndexOf(value.DATE);
                    if (index != -1)
                    {
                        IStockTrailStop trailStop = serie.GetTrailStop("TRAILHL(" + period + ")");
                        if (trailStop != null && trailStop.Series[0].Count > 0)
                        {
                            if (float.IsNaN(trailStop.Series[1][index]))
                            {
                                val++;
                            }
                            count++;
                        }
                    }
                }
                if (count != 0)
                {
                    val /= count;
                    val = (val - 0.5f) * 2.0f;
                    breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
                }
            }
            if (breadthSerie.Count == 0)
            {
                this.Remove(breadthSerie.StockName);
            }
            else
            {
                if (!string.IsNullOrEmpty(destinationFolder))
                {
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateIndiceEqualWeight(StockSerie breadthSerie, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            StockSerie indiceSerie = null;
            var refSerieName = indexName == "USA" ? "SP500" : "CAC40";
            if (this.ContainsKey(refSerieName))
            {
                indiceSerie = this[refSerieName];
                if (!indiceSerie.Initialise())
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
            if (indexComponents.Length == 0)
                return false;

            DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
            DateTime lastBreadthDate = DateTime.MinValue;

            float val = 1000, var;

            // Check if serie has been already generated
            if (breadthSerie.Count > 0)
            {
                lastBreadthDate = breadthSerie.LastValue.DATE;
                val = breadthSerie.LastValue.CLOSE;
                if (lastIndiceDate <= lastBreadthDate)
                {
                    // The breadth serie is up to date
                    return true;
                }
                // Check if latest value is intraday data
                if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
                {
                    // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
                    if (lastIndiceDate.Date == lastBreadthDate.Date)
                    {
                        breadthSerie.RemoveLast();
                        lastBreadthDate = breadthSerie.LastValue.DATE;
                    }
                }
            }
            #region Load component series
            foreach (StockSerie serie in indexComponents)
            {
                if (this.ReportProgress != null)
                {
                    this.ReportProgress("Loading data for " + serie.StockName);
                }
                serie.Initialise();
                serie.BarDuration = barDuration;
            }
            #endregion
            long vol;

            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; var = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50 && s.Keys.First() <= value.DATE))
                {
                    index = serie.IndexOf(value.DATE);
                    if (index != -1)
                    {
                        float dailyVar = serie.GetValue(StockDataType.VARIATION, index);
                        var += dailyVar;
                        vol++;
                    }
                }
                if (vol != 0)
                {
                    val *= (1 + var / vol);
                    breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
                }
            }
            if (breadthSerie.Count == 0)
            {
                this.Remove(breadthSerie.StockName);
            }
            else
            {
                if (!string.IsNullOrEmpty(destinationFolder))
                {
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_" + breadthSerie.StockGroup + ".csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_" + breadthSerie.StockGroup + ".csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateIndiceBest(string speedIndicator, StockSerie breadthSerie, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);
            int nbStocks = 10;

            StockSerie indiceSerie = null;
            var refSerieName = indexName == "USA" ? "SP500" : "CAC40";
            if (this.ContainsKey(refSerieName))
            {
                indiceSerie = this[refSerieName];
                if (!indiceSerie.Initialise())
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
            DateTime lastBreadthDate = DateTime.MinValue;

            float val = 1000, var;

            // Check if serie has been already generated
            if (breadthSerie.Count > 0)
            {
                lastBreadthDate = breadthSerie.LastValue.DATE;
                val = breadthSerie.LastValue.CLOSE;
                if (lastIndiceDate <= lastBreadthDate)
                {
                    // The breadth serie is up to date
                    return true;
                }
                // Check if latest value is intraday data
                if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
                {
                    // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
                    if (lastIndiceDate.Date == lastBreadthDate.Date)
                    {
                        breadthSerie.RemoveLast();
                        lastBreadthDate = breadthSerie.LastValue.DATE;
                    }
                }
            }

            StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName) && s.Initialise() && s.Count > 50).ToArray();
            if (indexComponents.Length == 0)
                return false;

            #region Load component series
            foreach (StockSerie serie in indexComponents)
            {
                if (this.ReportProgress != null)
                {
                    this.ReportProgress("Loading data for " + serie.StockName);
                }
                serie.Initialise();
                serie.BarDuration = barDuration;
                serie.GetIndicator($"{speedIndicator}({period})");
            }
            #endregion
            long vol;

            int index;
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;

            List<StockSerie> bestSeries = new List<StockSerie>();
            int previousWeekNumber = 52;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; var = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }
                int weekNumber = cal.GetWeekOfYear(value.DATE, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
                if (weekNumber != previousWeekNumber)
                {
                    bestSeries.Clear();
                    List<Tuple<StockSerie, float>> tuples = new List<Tuple<StockSerie, float>>();

                    // Reselect list of stock at the beging of the month
                    foreach (StockSerie serie in indexComponents.Where(s => s.Keys.First() <= value.DATE))
                    {
                        index = serie.IndexOf(value.DATE);
                        if (index >= 1)
                        {
                            if (serie.GetSerie(StockDataType.EXCHANGED)[index - 1] > 250000f)
                            {
                                float speed = serie.GetIndicator($"{speedIndicator}({period})").Series[0][index - 1];
                                if (speed > 0)
                                {
                                    tuples.Add(new Tuple<StockSerie, float>(serie, speed));
                                }
                            }
                        }
                    }
                    bestSeries = tuples.OrderByDescending(t => t.Item2).Select(t => t.Item1).Take(nbStocks).ToList();
                    previousWeekNumber = weekNumber;
                }
                foreach (StockSerie serie in bestSeries)
                {
                    index = serie.IndexOf(value.DATE);
                    if (index != -1)
                    {
                        float dailyVar = serie.GetValue(StockDataType.VARIATION, index);
                        var += dailyVar;
                        vol++;
                    }
                }
                if (vol != 0)
                {
                    val *= (1 + var / nbStocks);
                }
                breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
            }
            if (breadthSerie.Count == 0)
            {
                this.Remove(breadthSerie.StockName);
            }
            else
            {
                if (!string.IsNullOrEmpty(destinationFolder))
                {
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_" + breadthSerie.StockGroup + ".csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_" + breadthSerie.StockGroup + ".csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateIndiceBestOSC(StockSerie breadthSerie, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);
            int nbStocks = 10;

            StockSerie indiceSerie = null;
            var refSerieName = indexName == "USA" ? "SP500" : "CAC40";
            if (this.ContainsKey(refSerieName))
            {
                indiceSerie = this[refSerieName];
                if (!indiceSerie.Initialise())
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
            DateTime lastBreadthDate = DateTime.MinValue;

            float val = 1000, var;

            // Check if serie has been already generated
            if (breadthSerie.Count > 0)
            {
                lastBreadthDate = breadthSerie.LastValue.DATE;
                val = breadthSerie.LastValue.CLOSE;
                if (lastIndiceDate <= lastBreadthDate)
                {
                    // The breadth serie is up to date
                    return true;
                }
                // Check if latest value is intraday data
                if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
                {
                    // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
                    if (lastIndiceDate.Date == lastBreadthDate.Date)
                    {
                        breadthSerie.RemoveLast();
                        lastBreadthDate = breadthSerie.LastValue.DATE;
                    }
                }
            }

            StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName) && s.Initialise() && s.Count > 50).ToArray();
            if (indexComponents.Length == 0)
                return false;

            #region Load component series
            foreach (StockSerie serie in indexComponents)
            {
                if (this.ReportProgress != null)
                {
                    this.ReportProgress("Loading data for " + serie.StockName);
                }
                serie.Initialise();
                serie.BarDuration = barDuration;
                serie.GetIndicator($"OSC({period},{period * 2},True,EMA)");
            }
            #endregion
            long vol;

            int index;
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;

            List<StockSerie> bestSeries = new List<StockSerie>();
            int previousWeekNumber = 52;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; var = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }
                int weekNumber = cal.GetWeekOfYear(value.DATE, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
                if (weekNumber != previousWeekNumber)
                {
                    bestSeries.Clear();
                    List<Tuple<StockSerie, float>> tuples = new List<Tuple<StockSerie, float>>();
                    // Reselect list of stock at the beging of the month
                    foreach (StockSerie serie in indexComponents.Where(s => s.Keys.First() <= value.DATE))
                    {
                        index = serie.IndexOf(value.DATE);
                        if (index >= 1)
                        {

                            float osc = serie.GetIndicator($"OSC({period},{period * 2},True,EMA)").Series[0][index - 1];
                            if (osc > 0)
                            {
                                tuples.Add(new Tuple<StockSerie, float>(serie, osc));
                            }
                        }
                    }
                    bestSeries = tuples.OrderByDescending(t => t.Item2).Select(t => t.Item1).Take(nbStocks).ToList();
                    previousWeekNumber = weekNumber;
                }
                foreach (StockSerie serie in bestSeries)
                {
                    index = serie.IndexOf(value.DATE);
                    if (index != -1)
                    {
                        float dailyVar = serie.GetValue(StockDataType.VARIATION, index);
                        var += dailyVar;
                        vol++;
                    }
                }
                if (vol != 0)
                {
                    val *= (1 + var / nbStocks);
                }
                breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
            }
            if (breadthSerie.Count == 0)
            {
                this.Remove(breadthSerie.StockName);
            }
            else
            {
                if (!string.IsNullOrEmpty(destinationFolder))
                {
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_" + breadthSerie.StockGroup + ".csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_" + breadthSerie.StockGroup + ".csv", ArchiveEndDate, true);
                }
            }
            return true;
        }

        public float GeneratePTFBestFilter(StockSerie.Groups stockGroup, BarDuration barDuration, string filter, int nbStocks)
        {
            StockSerie indiceSerie = this["CAC40"]; // Use CAC40 as a reference serie
            if (!indiceSerie.Initialise())
            {
                return float.NaN;
            }
            indiceSerie.BarDuration = barDuration;

            float val = 1000, var;

            var indexComponents = this.Values.Where(s => s.BelongsToGroup(stockGroup) && s.Initialise() && s.Count > 50).ToArray();
            #region Load component series
            foreach (StockSerie serie in indexComponents)
            {
                serie.BarDuration = barDuration;
                serie.GetIndicator(filter);
            }
            #endregion
            long vol;

            int index;
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;

            List<StockSerie> bestSeries = new List<StockSerie>();
            int previousWeekNumber = 52;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                vol = 0; var = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }
                int weekNumber = cal.GetWeekOfYear(value.DATE, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
                if (weekNumber != previousWeekNumber)
                {
                    bestSeries.Clear();
                    List<Tuple<StockSerie, float>> tuples = new List<Tuple<StockSerie, float>>();
                    // Reselect list of stock at the beging of the month
                    foreach (StockSerie serie in indexComponents.Where(s => s.Keys.First() <= value.DATE))
                    {
                        index = serie.IndexOf(value.DATE);
                        if (index >= 1)
                        {

                            float osc = serie.GetIndicator(filter).Series[0][index - 1];
                            if (osc > 0)
                            {
                                tuples.Add(new Tuple<StockSerie, float>(serie, osc));
                            }
                        }
                    }
                    bestSeries = tuples.OrderByDescending(t => t.Item2).Select(t => t.Item1).Take(nbStocks).ToList();
                    previousWeekNumber = weekNumber;
                }
                foreach (StockSerie serie in bestSeries)
                {
                    index = serie.IndexOf(value.DATE);
                    if (index != -1)
                    {
                        float dailyVar = serie.GetValue(StockDataType.VARIATION, index);
                        var += dailyVar;
                        vol++;
                    }
                }
                if (vol != 0)
                {
                    val *= (1.0f + var / (float)nbStocks);
                }
            }
            return val;
        }
        public bool GenerateABCSectorEqualWeight(StockSerie breadthSerie, string destinationFolder, string archiveFolder)
        {
            StockSerie indiceSerie = this["CAC40"]; // Use CAC40 as a reference serie
            if (!indiceSerie.Initialise())
            {
                return false;
            }
            int sectorId = int.Parse(breadthSerie.Symbol);
            StockSerie[] indexComponents = this.Values.Where(s => s.SectorId == sectorId).ToArray();
            if (indexComponents.Length == 0)
                return false;

            DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
            DateTime lastBreadthDate = DateTime.MinValue;

            float val = 1000, var;

            // Check if serie has been already generated
            if (breadthSerie.Count > 0)
            {
                lastBreadthDate = breadthSerie.LastValue.DATE;
                val = breadthSerie.LastValue.CLOSE;
                if (lastIndiceDate <= lastBreadthDate)
                {
                    // The breadth serie is up to date
                    return true;
                }
                // Check if latest value is intraday data
                if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
                {
                    // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
                    if (lastIndiceDate.Date == lastBreadthDate.Date)
                    {
                        breadthSerie.RemoveLast();
                        lastBreadthDate = breadthSerie.LastValue.DATE;
                    }
                }
            }
            #region Load component series
            foreach (StockSerie serie in indexComponents)
            {
                if (this.ReportProgress != null)
                {
                    this.ReportProgress("Loading data for " + serie.StockName);
                }
                serie.Initialise();
                serie.BarDuration = BarDuration.Daily;
            }
            #endregion
            long vol;

            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; var = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50 && s.Keys.First() <= value.DATE))
                {
                    index = serie.IndexOf(value.DATE);
                    if (index != -1)
                    {
                        float dailyVar = serie.GetValue(StockDataType.VARIATION, index);
                        var += dailyVar;
                        vol++;
                    }
                }
                if (vol != 0)
                {
                    val *= (1 + var / vol);
                    breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
                }
            }
            if (breadthSerie.Count == 0)
            {
                return false;
                //this.Remove(breadthSerie.StockName);
            }
            else
            {
                if (!string.IsNullOrEmpty(destinationFolder))
                {
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_" + breadthSerie.StockGroup + ".csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_" + breadthSerie.StockGroup + ".csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateHigherThanMMSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Replace("MM", ""));

            StockSerie indiceSerie = null;
            var refSerieName = indexName == "USA" ? "SP500" : "CAC40";
            if (this.ContainsKey(refSerieName))
            {
                indiceSerie = this[refSerieName];
                if (!indiceSerie.Initialise())
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
            if (indexComponents.Length == 0)
                return false;

            DateTime lastIndiceDate = indiceSerie.Keys.Last(d => d.Date == d);
            DateTime lastBreadthDate = DateTime.MinValue;

            // Check if serie has been already generated
            if (breadthSerie.Count > 0)
            {
                lastBreadthDate = breadthSerie.LastValue.DATE;
                if (lastIndiceDate <= lastBreadthDate)
                {
                    // The breadth serie is up to date
                    return true;
                }
                // Check if latest value is intraday data
                if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
                {
                    // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
                    if (lastIndiceDate.Date == lastBreadthDate.Date)
                    {
                        breadthSerie.RemoveLast();
                        lastBreadthDate = breadthSerie.LastValue.DATE;
                    }
                }
            }
            #region Load component series
            foreach (StockSerie serie in indexComponents)
            {
                if (this.ReportProgress != null)
                {
                    this.ReportProgress("Loading data for " + serie.StockName);
                }
                serie.Initialise();
            }
            #endregion
            long vol;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = serie.IndexOf(value.DATE);
                    if (index != -1)
                    {
                        FloatSerie mmSerie = serie.GetIndicator("EMA(" + period.ToString() + ")").Series[0];
                        if (serie.GetValue(StockDataType.CLOSE, index) >= mmSerie[index])
                        {
                            val++;
                        }
                        count++;
                    }
                }
                if (count != 0)
                {
                    val /= count;
                    breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
                }
            }
            if (breadthSerie.Count == 0)
            {
                this.Remove(breadthSerie.StockName);
            }
            else
            {
                if (!string.IsNullOrEmpty(destinationFolder))
                {
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        #endregion
        #region ANALYSIS SERIALISATION
        public void ReadAnalysisFromXml(System.Xml.XmlReader reader)
        {
            reader.Read(); // Skip Header
            reader.Read(); // Skip StockAnalysisList
            reader.ReadStartElement(); // Start StockAnalysisItem
            while (reader.Name == "StockAnalysisItem")
            {
                string stockName = reader.GetAttribute("StockName");
                if (stockName != null)
                {
                    if (this.Keys.Contains(stockName))
                    {
                        this[stockName].ReadAnalysisFromXml(reader);
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
        public void WriteAnalysisToXml(System.Xml.XmlWriter writer)
        {
            // Serialize Flat Attributes
            writer.WriteStartElement("StockAnalysisList");

            foreach (StockSerie stockSerie in Values.Where(s => !s.StockAnalysis.IsEmpty()).OrderBy(s => s.StockGroup))
            {
                writer.WriteStartElement("StockAnalysisItem");
                writer.WriteAttributeString("StockName", stockSerie.StockName);

                stockSerie.WriteAnalysisToXml(writer);

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.Flush();
        }
        #endregion

        public float GetClosingPrice(string stockName, DateTime date, BarDuration duration)
        {
            if (this.ContainsKey(stockName))
            {
                var stockSerie = this[stockName];
                if (stockSerie.Initialise())
                {
                    stockSerie.BarDuration = duration;
                    var index = stockSerie.IndexOfFirstLowerOrEquals(date);
                    if (index != -1)
                    {
                        return stockSerie.ValueArray[index].CLOSE;
                    }
                }
            }
            return 0f;
        }
        public float GetLastClosingPrice(string stockName)
        {
            if (this.ContainsKey(stockName))
            {
                var stockSerie = this[stockName];
                if (stockSerie.Initialise() && stockSerie.Count > 0)
                {
                    return stockSerie.LastValue.CLOSE;
                }
            }
            return 0f;
        }

        public static StockSerie GetSerie(string stockName, string isin = null)
        {
            if (Instance.ContainsKey(stockName))
            {
                return Instance[stockName];
            }
            return Instance.Values.FirstOrDefault(s => s.ISIN == isin);
        }
        Stopwatch sw;
        public List<StockAlert> MatchAlert(StockAlertDef alertDef)
        {
            using MethodLogger ml = new MethodLogger(this, true, $"AlertDef: {alertDef.Title}");
            sw = Stopwatch.StartNew();
            var alerts = new List<StockAlert>();
            try
            {
                foreach (StockSerie stockSerie in Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(alertDef.Group)))
                {
                    if (alertDef.BarDuration > BarDuration.Monthly && stockSerie.BelongsToGroup(StockSerie.Groups.PEA) && !stockSerie.Intraday) // if intraday
                    {
                        continue;
                    }
                    if (stockSerie.Initialise())
                    {
                        var previousBarDuration = stockSerie.BarDuration;
                        try
                        {
                            stockSerie.BarDuration = alertDef.BarDuration;
                            if (stockSerie.Count < 30)
                                continue;

                            if (alertDef.MinLiquidity > 0 && stockSerie.HasVolume)
                            {
                                if (!stockSerie.HasLiquidity(alertDef.MinLiquidity, 10))
                                {
                                    continue;
                                }
                            }

                            var values = stockSerie.ValueArray;
                            var lastIndex = alertDef.CompleteBar ? stockSerie.LastCompleteIndex : stockSerie.LastIndex;
                            var dailyValue = values.ElementAt(lastIndex);
                            if (stockSerie.MatchEvent(alertDef))
                            {
                                alerts.Add(new StockAlert
                                {
                                    AlertDef = alertDef,
                                    Date = dailyValue.DATE,
                                    StockSerie = stockSerie
                                });
                            }
                        }
                        finally
                        {
                            stockSerie.BarDuration = previousBarDuration;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            sw.Stop();
            StockLog.Write($"MatchAlert Duration: {sw.Elapsed}");
            return alerts;
        }
    }
}
