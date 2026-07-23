using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using StockAnalyzer.UltimatePortfolio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StockAnalyzer.StockData.DataProviders.Breadth
{
    public class BreadthDataProvider : DataProviderBase
    {
        public override string DisplayName => "Breadth";

        public override BarDuration[] SupportedDurations => new BarDuration[] { BarDuration.Daily, BarDuration.Weekly, BarDuration.Monthly };
        public override BarDuration DefaultDuration => BarDuration.Daily;
        public override DataProvider Provider => DataProvider.Breadth;

        public override void OpenInDataProvider(StockInstrument stockInstrument) { }

        protected override StockInstrument CreateInstrumentFromConfigLine(string line)
        {
            string[] row = line.Split(',');
            if (row.Length != 2)
            {
                MessageBox.Show($"Invalid breadth definition: {line}");
                return null;
            }

            var id = row[0];
            var group = row[1];

            var instrument = new StockInstrument()
            {
                Id = id,
                Name = id,
                Provider = DataProvider.Breadth,
                Group = (Groups)Enum.Parse(typeof(Groups), group),
                Market = Market.NYSE
            };

            return instrument;
        }

        protected override void PostInitDictionary(bool download)
        {
        }

        protected override void PreInitDictionary(bool download)
        {
        }

        public override DataSerie DownloadData(StockInstrument instrument)
        {
            try
            {
                DataSerie dataSerie = LoadData(instrument, DefaultDuration);
                DateTime startDate = dataSerie?.LastValue != null ? dataSerie.LastValue.DATE.Date : DateTime.MinValue;

                var newBars = GenerateBreath(instrument, startDate);

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

        private StockDailyValue[] GenerateBreath(StockInstrument instrument, DateTime startDate)
        {
            var bars = new List<StockDailyValue>();

            return bars.ToArray();
        }

        public StockDailyValue[] GenerateEMABreadthSerie(StockInstrument instrument, string indexName, BarDuration barDuration, string destinationFolder, string archiveFolder)
        {
            int period = int.Parse(instrument.DisplayName.Split('.')[0].Split('_')[1]);

            indexName = indexName == "USA" ? "S&P 500" : "CAC40";
            if (!StockDictionary.Instruments.TryGetValue(indexName, out var indiceInstrument))
            {
                return null;
            }

            DataSerie indiceSerie = indiceInstrument.GetDefaultDataSerie();
            if (indiceSerie == null || indiceSerie.Values.Length == 0)
                return null;

            var indexComponents = StockDictionary.Instruments.Values.Where(s => s.BelongsToGroup(indexName)).ToArray();
            if (indexComponents.Length == 0)
                return null;

            DateTime lastIndiceDate = indiceSerie.LastValue.DATE;
            DateTime lastBreadthDate = DateTime.MinValue;

            return null;
            // Check if serie has been already generated
            //if (breadthSerie.Count > 0)
            //{
            //    lastBreadthDate = breadthSerie.LastValue.DATE;
            //    if (lastIndiceDate <= lastBreadthDate)
            //    {
            //        // The breadth serie is up to date
            //        return true;
            //    }
            //    // Check if latest value is intraday data
            //    if (lastIndiceDate.TimeOfDay > TimeSpan.Zero)
            //    {
            //        // this are intraday data, remove the breadth latest data to avoid creating multiple bars on the same day
            //        if (lastIndiceDate.Date == lastBreadthDate.Date)
            //        {
            //            breadthSerie.RemoveLast();
            //            lastBreadthDate = breadthSerie.LastValue.DATE;
            //        }
            //    }
            //}
            //#region Load component series
            //foreach (StockSerie serie in indexComponents)
            //{
            //    if (this.ReportProgress != null)
            //    {
            //        this.ReportProgress("Loading data for " + serie.StockName);
            //    }
            //    serie.Initialise();
            //    serie.BarDuration = barDuration;
            //}
            //#endregion
            //long vol;
            //float val, count;
            //foreach (StockDailyValue value in indiceSerie.Values)
            //{
            //    if (value.DATE <= lastBreadthDate)
            //    {
            //        continue;
            //    }
            //    vol = 0; val = 0; count = 0;
            //    if (this.ReportProgress != null)
            //    {
            //        this.ReportProgress(value.DATE.ToShortDateString());
            //    }
            //    int index = -1;
            //    foreach (StockSerie serie in indexComponents.Where(s => s.IsInitialised && s.Count > 50))
            //    {
            //        index = serie.IndexOf(value.DATE);
            //        if (index != -1)
            //        {
            //            IStockEvent emaIndicator = serie.GetTrailStop("TRAILEMA(" + period + ",1)");
            //            if (emaIndicator != null && emaIndicator.Events[0].Count > 0)
            //            {
            //                if (emaIndicator.Events[6][index])
            //                {
            //                    val++;
            //                }
            //                count++;
            //            }
            //        }
            //    }
            //    if (count != 0)
            //    {
            //        val /= count;
            //        val = (val - 0.5f) * 2.0f;
            //        breadthSerie.Add(value.DATE, new StockDailyValue(val, val, val, val, vol, value.DATE));
            //    }
            //}
            //if (breadthSerie.Count == 0)
            //{
            //    this.Remove(breadthSerie.StockName);
            //}
            //else
            //{
            //    if (!string.IsNullOrEmpty(destinationFolder))
            //    {
            //        breadthSerie.SaveToCSV(destinationFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, false);
            //    }
            //    if (!string.IsNullOrEmpty(archiveFolder) && lastBreadthDate < ArchiveEndDate)
            //    {
            //        breadthSerie.SaveToCSV(archiveFolder + "\\" + breadthSerie.Symbol + "_" + breadthSerie.StockName + "_BREADTH.csv", ArchiveEndDate, true);
            //    }
            //}
            //return true;
        }
    }
}
