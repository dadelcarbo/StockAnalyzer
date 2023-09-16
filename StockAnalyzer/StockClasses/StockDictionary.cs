using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace StockAnalyzer.StockClasses
{
    public class StockDictionary : SortedDictionary<string, StockSerie>, IStockPriceProvider
    {
        public System.DateTime ArchiveEndDate { get; private set; }

        public delegate void OnSerieEventDetectionDone();

        public static StockDictionary Instance { get; set; }

        public delegate void ReportProgressHandler(string progress);
        public event ReportProgressHandler ReportProgress;

        public StockDictionary(System.DateTime archiveEndDate)
        {
            Instance = this;
            this.ArchiveEndDate = archiveEndDate;

            StockPortfolio.StockPortfolio.PriceProvider = this;
        }

        private static List<string> validGroups = null;
        public List<string> GetValidGroupNames()
        {
            if (validGroups == null)
            {
                validGroups = new List<string>();
                foreach (StockSerie.Groups group in Enum.GetValues(typeof(StockSerie.Groups)))
                {
                    if (!this.Values.Any(s => s.BelongsToGroup(group)) || group == StockSerie.Groups.ALL ||
                        group == StockSerie.Groups.NONE)
                    {
                        continue;
                    }
                    validGroups.Add(group.ToString());
                }
            }
            return validGroups;
        }
        #region RANK Calculation
        public void CalculateRank(StockSerie.Groups group, string indicatorName, StockBarDuration duration, string destinationFolder)
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
            using (StreamWriter sw = new StreamWriter(fileName, false))
            {
                foreach (var serie in groupsSeries)
                {
                    var serieRanks = ranks.Select(r => r.Value.FirstOrDefault(l => l.Item1 == serie)?.Item2).Where(r => r != null).Select(r => r.Value.ToString("G4"));
                    sw.WriteLine(serie.StockName + "|" + serieRanks.Aggregate((i, j) => i + "|" + j));
                }
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
            if (this.ContainsKey("CAC40"))
            {
                indiceSerie = this["CAC40"];
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
        public bool GenerateSTOKFBreadthSerie(StockSerie breadthSerie, string indexName, StockBarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);
            StockSerie indiceSerie = null;
            if (this.ContainsKey("CAC40"))
            {
                indiceSerie = this["CAC40"];
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
        public bool GenerateSTOKSBreadthSerie(StockSerie breadthSerie, string indexName, StockBarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);
            StockSerie indiceSerie = null;
            if (this.ContainsKey("CAC40"))
            {
                indiceSerie = this["CAC40"];
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
        public bool GenerateEMABreadthSerie(StockSerie breadthSerie, string indexName, StockBarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);
            StockSerie indiceSerie = null;
            if (this.ContainsKey("CAC40"))
            {
                indiceSerie = this["CAC40"];
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

        public bool GenerateERBreadthSerie(StockSerie breadthSerie, string indexName, StockBarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);
            StockSerie indiceSerie = null;
            if (this.ContainsKey("CAC40"))
            {
                indiceSerie = this["CAC40"];
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
                        IStockIndicator erIndicator = serie.GetIndicator("ER(" + period + ",6,6,0.7)");
                        if (erIndicator != null && erIndicator.Series[0].Count > 0)
                        {
                            if (erIndicator.Series[0][index] >= 0)
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
        public bool GenerateHigherThanHLTrailSerie(StockSerie breadthSerie, string indexName, StockBarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);
            StockSerie indiceSerie = null;
            if (this.ContainsKey("CAC40"))
            {
                indiceSerie = this["CAC40"];
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
        public bool GenerateIndiceEqualWeight(StockSerie breadthSerie, string indexName, StockBarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            StockSerie indiceSerie = this["CAC40"]; // Use CAC40 as a reference serie

            if (!indiceSerie.Initialise())
            {
                return false;
            }

            StockSerie[] indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();

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
        public bool GenerateIndiceBestROC(StockSerie breadthSerie, string indexName, StockBarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);
            int nbStocks = 10;
            StockSerie indiceSerie = this["CAC40"]; // Use CAC40 as a reference serie

            if (!indiceSerie.Initialise())
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
            #region Load component series
            foreach (StockSerie serie in indexComponents)
            {
                if (this.ReportProgress != null)
                {
                    this.ReportProgress("Loading data for " + serie.StockName);
                }
                serie.Initialise();
                serie.BarDuration = barDuration;
                serie.GetIndicator($"ROC({period})");
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
                            float ROC = serie.GetIndicator($"ROC({period})").Series[0][index - 1];
                            if (ROC > 0)
                            {
                                tuples.Add(new Tuple<StockSerie, float>(serie, ROC));
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
        public bool GenerateIndiceBestOSC(StockSerie breadthSerie, string indexName, StockBarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);
            int nbStocks = 10;
            StockSerie indiceSerie = this["CAC40"]; // Use CAC40 as a reference serie

            if (!indiceSerie.Initialise())
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

        public float GeneratePTFBestFilter(StockSerie.Groups stockGroup, StockBarDuration barDuration, string filter, int nbStocks)
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

        public bool GenerateIndiceBBTF(StockSerie breadthSerie, string indexName, StockBarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);
            int nbStocks = 10;
            StockSerie indiceSerie = this["CAC40"]; // Use CAC40 as a reference serie

            if (!indiceSerie.Initialise())
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
            #region Load component series
            foreach (StockSerie serie in indexComponents)
            {
                if (this.ReportProgress != null)
                {
                    this.ReportProgress("Loading data for " + serie.StockName);
                }
                serie.Initialise();
                serie.BarDuration = barDuration;
                serie.GetTrailStop($"TRAILBBTF({period},1,-1,MA)");
            }
            #endregion
            long vol;

            int index;
            List<StockSerie> bestSeries = new List<StockSerie>();
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
                bestSeries.Clear();
                List<Tuple<StockSerie, float>> tuples = new List<Tuple<StockSerie, float>>();
                // Reselect list of stock at the beging of the month
                foreach (StockSerie serie in indexComponents.Where(s => s.Keys.First() <= value.DATE))
                {
                    index = serie.IndexOf(value.DATE);
                    if (index > 1)
                    {
                        bool bullish = serie.GetTrailStop($"TRAILBBTF({period},1,-1,MA)").Events[6][index - 1];

                        float ROC = serie.GetIndicator($"ROC({period})").Series[0][index - 1];
                        if (bullish && ROC > 0)
                        {
                            tuples.Add(new Tuple<StockSerie, float>(serie, ROC));
                        }
                    }
                }
                bestSeries = tuples.OrderByDescending(t => t.Item2).Select(t => t.Item1).Take(nbStocks).ToList();
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
        public bool GenerateABCSectorEqualWeight(StockSerie breadthSerie, string destinationFolder, string archiveFolder)
        {
            StockSerie indiceSerie = this["CAC40"]; // Use CAC40 as a reference serie
            if (!indiceSerie.Initialise())
            {
                return false;
            }
            int sectorId = int.Parse(breadthSerie.Symbol);
            StockSerie[] indexComponents = this.Values.Where(s => s.SectorId == sectorId).ToArray();

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
                serie.BarDuration = StockBarDuration.Daily;
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
        public bool GenerateMyOscBreadth(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Replace("MYOSC_", ""));
            StockSerie indiceSerie = null;
            if (this.ContainsKey("CAC40"))
            {
                indiceSerie = this["CAC40"];
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
                        FloatSerie mmSerie = serie.GetIndicator("MYOSC(" + period.ToString() + ",6)").Series[0];
                        if (serie.GetValue(StockDataType.CLOSE, index) >= mmSerie[index])
                        {
                            val += mmSerie[index];
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
        public bool GenerateBBWidthBreadth(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Replace("BBWIDTH_", ""));
            StockSerie indiceSerie = null;
            if (this.ContainsKey("CAC40"))
            {
                indiceSerie = this["CAC40"];
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
                        FloatSerie mmSerie = serie.GetIndicator("BBWIDTH(" + period.ToString() + ",MA)").Series[0];
                        if (serie.GetValue(StockDataType.CLOSE, index) >= mmSerie[index])
                        {
                            val += mmSerie[index];
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
        public bool GenerateMcClellanSumSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            StockSerie indiceSerie = null;
            if (this.ContainsKey("CAC40"))
            {
                indiceSerie = this["CAC40"];
                if (!indiceSerie.Initialise())
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            StockSerie[] indexComponents;
            if (indexName.EndsWith("_SI"))
            {
                int sectorId = int.Parse(breadthSerie.Symbol);
                indexComponents = this.Values.Where(s => !s.StockAnalysis.Excluded && s.SectorId == sectorId).ToArray();
            }
            else
            {
                indexComponents = this.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
            }
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
            float alpha19 = 2.0f / 20.0f;
            float alpha39 = 2.0f / 40.0f;
            float ema19 = 0.0f;
            float ema39 = 0.0f;
            float sum = 0;
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
                        if (serie.GetValue(StockDataType.VARIATION, index) >= 0)
                        {
                            val++;
                        }
                        count++;
                    }
                }
                if (count != 0)
                {
                    val = (2 * val - count) / count;
                    ema19 = ema19 + alpha19 * (val - ema19);
                    ema39 = ema39 + alpha39 * (val - ema39);
                    val = ema19 - ema39;
                    sum += val;
                    breadthSerie.Add(value.DATE, new StockDailyValue(sum, sum, sum, sum, vol, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + $"_{breadthSerie.StockGroup}.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + $"_{breadthSerie.StockGroup}.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateMcClellanSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            StockSerie indiceSerie = null;
            if (this.ContainsKey("CAC40"))
            {
                indiceSerie = this["CAC40"];
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
            float alpha19 = 2.0f / 20.0f;
            float alpha39 = 2.0f / 40.0f;
            float ema19 = 0.0f;
            float ema39 = 0.0f;
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
                        if (serie.GetValue(StockDataType.VARIATION, index) >= 0)
                        {
                            val++;
                        }
                        count++;
                    }
                }
                if (count != 0)
                {
                    val = (2 * val - count) / count;
                    ema19 = ema19 + alpha19 * (val - ema19);
                    ema39 = ema39 + alpha39 * (val - ema39);
                    val = ema19 - ema39;
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
        public bool GenerateHigherThanMMSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Replace("MM", ""));
            StockSerie indiceSerie = null;
            if (this.ContainsKey("CAC40"))
            {
                indiceSerie = this["CAC40"];
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

        public float GetClosingPrice(string stockName, DateTime date, StockClasses.BarDuration duration)
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
                if (stockSerie.Initialise())
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
    }
}
