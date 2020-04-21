using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses
{
    public class StockDictionary : SortedDictionary<string, StockSerie>, IStockPriceProvider
    {
        public System.DateTime ArchiveEndDate { get; private set; }

        public delegate void OnSerieEventDetectionDone();

        public static StockDictionary StockDictionarySingleton { get; set; }

        public delegate void ReportProgressHandler(string progress);
        public event ReportProgressHandler ReportProgress;

        public StockDictionary(System.DateTime archiveEndDate)
        {
            StockDictionarySingleton = this;
            this.ArchiveEndDate = archiveEndDate;

            StockBinckPortfolio.StockPortfolio.PriceProvider = this;
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

        #region BREADTH INDICATOR GENERATION
        public bool GenerateAdvDeclCumulSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                indiceSerie = this[indexName];
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

            float val = 0f;

            // Check if serie has been already generated
            if (breadthSerie.Count > 0)
            {
                lastBreadthDate = breadthSerie.Keys.Last();
                if (lastIndiceDate <= lastBreadthDate)
                {
                    // The breadth serie is up to date
                    return true;
                }
                val = breadthSerie.Values.Last().CLOSE;
                // Check if latest value is intraday data
                if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
                {
                    // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
                    if (lastIndiceDate.Date == lastBreadthDate.Date)
                    {
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;
            float dailyVal, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; upVol = 0; tick = 0; upTick = 0; dailyVal = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = -1;
                    if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
                    if (index != -1)
                    {
                        vol += value.VOLUME;
                        upVol += value.UPVOLUME;
                        tick += value.TICK;
                        upTick += value.UPTICK;
                        if (serie.ValueArray[index].VARIATION >= 0)
                        {
                            dailyVal++;
                        }
                        else
                        {
                            dailyVal--;
                        }
                        count++;
                    }
                }
                if (count != 0)
                {
                    dailyVal /= count;
                    val += dailyVal;
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }

        public bool GenerateAdvDeclSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                indiceSerie = this[indexName];
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
                lastBreadthDate = breadthSerie.Keys.Last();
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
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; upVol = 0; tick = 0; upTick = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = -1;
                    if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
                    if (index != -1)
                    {
                        vol += value.VOLUME;
                        upVol += value.UPVOLUME;
                        tick += value.TICK;
                        upTick += value.UPTICK;
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
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateBullishROCEXSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                indiceSerie = this[indexName];
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
                lastBreadthDate = breadthSerie.Keys.Last();
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
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; upVol = 0; tick = 0; upTick = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = -1;
                    if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
                    if (index != -1)
                    {
                        IStockIndicator oscSerie = serie.GetIndicator("ROCEX3(50,25,10,10,20)");
                        if (oscSerie != null && oscSerie.Series[0].Count > 0)
                        {
                            if (oscSerie.Events[2][index])
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
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateSTOKFBreadthSerie(StockSerie breadthSerie, string indexName, StockBarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);
            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                indiceSerie = this[indexName];
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
                lastBreadthDate = breadthSerie.Keys.Last();
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
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; upVol = 0; tick = 0; upTick = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = -1;
                    if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
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
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }

        public bool GenerateEMABreadthSerie(StockSerie breadthSerie, string indexName, StockBarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);
            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                indiceSerie = this[indexName];
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
                lastBreadthDate = breadthSerie.Keys.Last();
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
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; upVol = 0; tick = 0; upTick = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = -1;
                    if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
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
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }

        public bool GenerateERBreadthSerie(StockSerie breadthSerie, string indexName, StockBarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);
            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                indiceSerie = this[indexName];
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
                lastBreadthDate = breadthSerie.Keys.Last();
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
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; upVol = 0; tick = 0; upTick = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = -1;
                    if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
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
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateVarBreadthSerie(StockSerie breadthSerie, string indexName, StockBarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);
            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                indiceSerie = this[indexName];
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
                lastBreadthDate = breadthSerie.Keys.Last();
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
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; upVol = 0; tick = 0; upTick = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = -1;
                    if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
                    if (index != -1)
                    {
                        IStockIndicator trailStop = serie.GetIndicator("VAR(" + period + ")");
                        if (trailStop != null && trailStop.Series[0].Count > 0)
                        {
                            val += trailStop.Series[0][index];
                            count++;
                        }
                    }
                }
                if (count != 0)
                {
                    val /= count;
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateHigherThanHLTrailSerie(StockSerie breadthSerie, string indexName, StockBarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);
            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                indiceSerie = this[indexName];
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
                lastBreadthDate = breadthSerie.Keys.Last();
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
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; upVol = 0; tick = 0; upTick = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = -1;
                    if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
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
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateBullishOverboughtSerie(StockSerie breadthSerie, string indexName, StockBarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);
            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                indiceSerie = this[indexName];
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
                lastBreadthDate = breadthSerie.Keys.Last();
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
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; upVol = 0; tick = 0; upTick = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = -1;
                    if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
                    if (index != -1)
                    {
                        IStockIndicator indicator = serie.GetIndicator("OVERBOUGHTSR(STOKS(" + period + "_3_3),75,25)");
                        if (indicator != null && indicator.Series[0].Count > 0)
                        {
                            if (indicator.Events[8][index])
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
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateTOPEMASerie(StockSerie breadthSerie, string indexName, StockBarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                indiceSerie = this[indexName];
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
                lastBreadthDate = breadthSerie.Keys.Last();
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
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; upVol = 0; tick = 0; upTick = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = -1;
                    if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
                    if (index != -1)
                    {
                        serie.BarDuration = barDuration;
                        IStockIndicator trailStop = serie.GetIndicator("TOPEMA(0,30,1)");
                        if (trailStop != null && trailStop.Events[0].Count > 0)
                        {
                            if (trailStop.Events[8][index])
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
                }
                if (count != 0)
                {
                    val /= count;
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateIndiceEqualWeight(StockSerie breadthSerie, string indexName, StockBarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                if (indexName == "SRD" || indexName == "CACALL") indiceSerie = this["CAC40"];
                else indiceSerie = this[indexName];

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

            float val = 1000, var;

            // Check if serie has been already generated
            if (breadthSerie.Count > 0)
            {
                lastBreadthDate = breadthSerie.Keys.Last();
                val = breadthSerie.Values.Last().CLOSE;
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
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;

            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; upVol = 0; tick = 0; upTick = 0; var = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50 && s.Keys.First() <= value.DATE))
                {
                    index = -1;
                    if (isIntraday && serie.Values.Last().DATE == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
                    if (index != -1)
                    {
                        float dailyVar = serie.GetValue(StockDataType.VARIATION, index);
                        if (dailyVar >= 0)
                        {
                            upVol++;
                        }
                        var += dailyVar;
                        vol++;
                    }
                }
                if (vol != 0)
                {
                    val *= (1 + var / vol);
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_INDICES.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_INDICES.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateMyOscBreadth(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Replace("MYOSC_", ""));
            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                indiceSerie = this[indexName];
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
                lastBreadthDate = breadthSerie.Keys.Last();
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
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; upVol = 0; tick = 0; upTick = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = -1;
                    if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
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
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateMcClellanSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                indiceSerie = this[indexName];
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
                lastBreadthDate = breadthSerie.Keys.Last();
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
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;
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
                vol = 0; upVol = 0; tick = 0; upTick = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = -1;
                    if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
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
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateHigherThanMMSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Replace("MM", ""));
            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                indiceSerie = this[indexName];
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
                lastBreadthDate = breadthSerie.Keys.Last();
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
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; upVol = 0; tick = 0; upTick = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = -1;
                    if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
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
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateHighestInDays(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Replace("HD", ""));

            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                indiceSerie = this[indexName];
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
                lastBreadthDate = breadthSerie.Keys.Last();
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
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; upVol = 0; tick = 0; upTick = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = -1;
                    if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
                    if (index != -1)
                    {
                        FloatSerie closeSerie = serie.GetSerie(StockDataType.CLOSE);
                        if (closeSerie[index] > closeSerie[Math.Max(0, index - period)])
                        {
                            val++;
                        }
                        count++;
                    }
                }
                if (count != 0)
                {
                    val /= count;
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateLowestInDays(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Replace("LD", ""));

            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                indiceSerie = this[indexName];
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
                lastBreadthDate = breadthSerie.Keys.Last();
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
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; upVol = 0; tick = 0; upTick = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = -1;
                    if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
                    if (index != -1)
                    {
                        FloatSerie closeSerie = serie.GetSerie(StockDataType.CLOSE);
                        if (closeSerie[index] <= closeSerie[Math.Max(0, index - period)])
                        {
                            val++;
                        }
                        count++;
                    }
                }
                if (count != 0)
                {
                    val /= count;
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }
        public bool GenerateRecordHighIndexInDays(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Replace("RHI", ""));

            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                indiceSerie = this[indexName];
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
                lastBreadthDate = breadthSerie.Keys.Last();
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
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;
            float hdVal, ldVal, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; upVol = 0; tick = 0; upTick = 0; hdVal = 0; ldVal = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = -1;
                    if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
                    if (index != -1)
                    {
                        FloatSerie closeSerie = serie.GetSerie(StockDataType.CLOSE);
                        if (closeSerie.IsHighestInDays(index, period))
                        {
                            hdVal++;
                            count++;
                        }
                        else if (closeSerie.IsLowestInDays(index, period))
                        {
                            ldVal++;
                            count++;
                        }
                    }
                }
                if (count != 0)
                {
                    hdVal /= count;
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, hdVal, hdVal, hdVal, hdVal, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
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

        public bool GenerateTrinSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(breadthSerie.StockName.Split('.')[0].Split('_')[1]);

            StockSerie indiceSerie = null;
            if (this.ContainsKey(indexName))
            {
                indiceSerie = this[indexName];
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
                lastBreadthDate = breadthSerie.Keys.Last();
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
                        breadthSerie.Remove(lastBreadthDate);
                        lastBreadthDate = breadthSerie.Keys.Last();
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
            long vol, upVol;
            int tick, upTick;
            float val, count;
            foreach (StockDailyValue value in indiceSerie.Values)
            {
                if (value.DATE <= lastBreadthDate)
                {
                    continue;
                }
                vol = 0; upVol = 0; tick = 0; upTick = 0; val = 0; count = 0;
                if (this.ReportProgress != null)
                {
                    this.ReportProgress(value.DATE.ToShortDateString());
                }

                bool isIntraday = false;
                if (value.DATE.TimeOfDay != TimeSpan.Zero)
                {
                    isIntraday = true;
                }
                int index = -1;
                foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
                {
                    index = -1;
                    if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
                    {
                        index = serie.Keys.Count - 1;
                    }
                    else
                    {
                        index = serie.IndexOf(value.DATE);
                    }
                    if (index != -1)
                    {
                        FloatSerie closeSerie = serie.GetSerie(StockDataType.CLOSE);
                        if (closeSerie[index] <= closeSerie[Math.Max(0, index - period)])
                        {
                            val++;
                        }
                        count++;
                    }
                }
                if (count != 0)
                {
                    val /= count;
                    breadthSerie.Add(value.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, value.DATE));
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
                    breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
                }
                if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
                {
                    breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.ShortName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
                }
            }
            return true;
        }

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

        public StockSerie GeneratePortfolioSerie(StockBinckPortfolio.StockPortfolio binckPortfolio)
        {
            var refStock = this["CAC40"];
            if (!refStock.Initialise())
                return null;

            refStock.BarDuration = BarDuration.Daily;
            var startDate = binckPortfolio.Operations.OrderBy(op => op.Id).First().Date;

            StockSerie portfolioSerie = new StockSerie(binckPortfolio.Name, binckPortfolio.Name, refStock.StockGroup, StockDataProvider.BinckPortfolio);
            portfolioSerie.IsPortofolioSerie = true;

            float value;
            foreach (var date in refStock.Keys.Where(d => d >= startDate))
            {
                long volume;
                value = binckPortfolio.EvaluateAt(date, BarDuration.Daily, out volume);
                portfolioSerie.Add(date, new StockDailyValue(binckPortfolio.Name, value, value, value, value, volume, date));
            }

            // Preinitialise the serie
            portfolioSerie.PreInitialise();
            portfolioSerie.IsInitialised = true;
            return portfolioSerie;
        }
    }
}
