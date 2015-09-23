using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;

namespace StockAnalyzer.StockClasses
{
   public class StockDictionary : SortedDictionary<string, StockSerie>
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
      }
      public void DetectEvents(OnSerieEventDetectionDone onSerieEventDetectionDone, StockPortofolioList stockPortofolioList, string selectedEvents)
      {
         // Detect Events according to the new selected list
         foreach (StockSerie stockSerie in Values)
         {
            if (!stockSerie.StockAnalysis.Excluded)
            {
               stockSerie.DetectEventsForGui(stockSerie.Count - 1, StockEvent.GetEventFilterMode(), stockPortofolioList, selectedEvents);
               // Notifiy one serie has been analysed
               onSerieEventDetectionDone();
            }
         }
      }
      public void CreatePortofolioSerie(StockPortofolio portofolio)
      {
         string referenceStockName = string.Empty;
         if ((portofolio.OrderList != null) && (portofolio.OrderList.Count != 0))
         {
            referenceStockName = portofolio.OrderList.First().StockName;
         }
         else
         {
            referenceStockName = this.Values.First().StockName;
         }
         // Refresh portofolio generated stock
         if (this.Keys.Contains(portofolio.Name))
         {
            this.Remove(portofolio.Name);
         }
         if (this.Keys.Contains(referenceStockName))
         {
            portofolio.Initialize(this);
            this.Add(portofolio.Name, portofolio.GeneratePortfolioStockSerie(portofolio.Name, this[referenceStockName], portofolio.Group));
            // this[portofolio.Name].Initialise();
         }
      }

      public List<string> GetValidGroupNames()
      {
         List<string> validGroups = new List<string>();
         foreach (StockSerie.Groups group in Enum.GetValues(typeof(StockSerie.Groups)))
         {
            if (this.Values.Count(s => s.BelongsToGroup(group)) == 0 || group == StockSerie.Groups.ALL || group == StockSerie.Groups.NONE)
            {
               continue;
            }
            validGroups.Add(group.ToString());
         }
         return validGroups;
      }

      #region BREADTH INDICATOR GENERATION
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

         DateTime lastIndiceDate = indiceSerie.Keys.Last();
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
            foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised))
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
               breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
            }
            if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
            {
               breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
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

         DateTime lastIndiceDate = indiceSerie.Keys.Last();
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
            foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised))
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
               breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
            }
            if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
            {
               breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
            }
         }
         return true;
      }
      public bool GenerateHigherThanHLTrailSerie(StockSerie breadthSerie, string indexName, StockSerie.StockBarDuration barDuration, string destinationFolder, string archiveFolder)
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

         DateTime lastIndiceDate = indiceSerie.Keys.Last();
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
            foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised))
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
               breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
            }
            if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
            {
               breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
            }
         }
         return true;
      }

      public bool GenerateHigherThanTrailVolSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
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

         DateTime lastIndiceDate = indiceSerie.Keys.Last();
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
            foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised))
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
                  IStockTrailStop trailStop = serie.GetTrailStop("TRAILVOL(True)");
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
               breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
            }
            if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
            {
               breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
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

         DateTime lastIndiceDate = indiceSerie.Keys.Last();
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
            foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised))
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
               breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
            }
            if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
            {
               breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
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

         DateTime lastIndiceDate = indiceSerie.Keys.Last();
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
            foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised))
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
               breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
            }
            if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
            {
               breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
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

         DateTime lastIndiceDate = indiceSerie.Keys.Last();
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
            foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised))
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
               breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
            }
            if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
            {
               breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
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

         DateTime lastIndiceDate = indiceSerie.Keys.Last();
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
            foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised))
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
               breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
            }
            if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
            {
               breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
            }
         }
         return true;
      }
      public bool GenerateCorrelationSerie(StockSerie breadthSerie, string indexName, string destinationFolder, string archiveFolder)
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

         DateTime lastIndiceDate = indiceSerie.Keys.Last();
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
         foreach (StockDailyValue indexDailyValue in indiceSerie.Values)
         {
            if (indexDailyValue.DATE <= lastBreadthDate)
            {
               continue;
            }
            vol = 0; upVol = 0; tick = 0; upTick = 0; val = 0; count = 0;
            if (this.ReportProgress != null)
            {
               this.ReportProgress(indexDailyValue.DATE.ToShortDateString());
            }

            bool isIntraday = false;
            if (indexDailyValue.DATE.TimeOfDay != TimeSpan.Zero)
            {
               isIntraday = true;
            }
            int index = -1;
            foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised))
            {
               index = -1;
               if (isIntraday && serie.Keys.Last().Date == lastIndiceDate.Date)
               {
                  index = serie.Keys.Count - 1;
               }
               else
               {
                  index = serie.IndexOf(indexDailyValue.DATE);
               }
               if (index != -1)
               {
                  if (serie.GetSerie(StockIndicatorType.VARIATION_REL)[index] * indexDailyValue.VARIATION >= 0)
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
               breadthSerie.Add(indexDailyValue.DATE, new StockDailyValue(breadthSerie.StockName, val, val, val, val, vol, upVol, tick, upTick, indexDailyValue.DATE));
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
               breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_" + breadthSerie.StockGroup.ToString() + ".csv", ArchiveEndDate, false);
            }
            if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
            {
               breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_" + breadthSerie.StockGroup.ToString() + ".csv", ArchiveEndDate, true);
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

         DateTime lastIndiceDate = indiceSerie.Keys.Last();
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
            foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised))
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
               breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
            }
            if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
            {
               breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.StockName + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
            }
         }
         return true;
      }
   }
}
