using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (!Settings.Default.GenerateBreadth)
                return null;
            var fields = instrument.Id.Split('.');
            var breadthFields = fields[0].Split('_');

            switch (breadthFields[0])
            {
                case "AD":
                    {
                        return GenerateCountIfBreadthSerie(instrument, fields[1], startDate, (serie, d) =>
                        {
                            var index = serie.IndexOf(d);
                            if (index == -1)
                                return false;

                            return serie.Values[index].VARIATION > 0;

                        });
                    }
                case "ADSUM":
                    {
                        return GenerateSumIfBreadthSerie(instrument, fields[1], startDate, (serie, d) =>
                        {
                            var index = serie.IndexOf(d);
                            if (index == -1)
                                return false;

                            return serie.Values[index].VARIATION > 0;
                        });
                    }
                case "MC":
                    {
                        var adInstrument = StockDictionary.GetInstrumentByName("AD." + fields[1]);
                        if (adInstrument == null)
                            return null;
                        var adSerie = adInstrument.GetDefaultDataSerie();
                        var ema19 = adSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(19);
                        var ema39 = adSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(39);
                        var val = ema19 - ema39;
                        int i = 0;
                        return adSerie.Values.Select(v => new StockDailyValue(val[i], val[i], val[i], val[i++], 0, v.DATE)).ToArray();
                    }
                case "MCSUM":
                    {
                        var mcInstrument = StockDictionary.GetInstrumentByName("MC." + fields[1]);
                        if (mcInstrument == null)
                            return null;
                        var val = mcInstrument.GetDefaultDataSerie();
                        if (val == null)
                            return null;

                        float sum = 0f;
                        return val.Values.Select(v => new StockDailyValue(sum += v.CLOSE, sum, sum, sum, 0, v.DATE)).ToArray();
                    }
                case "EMA":
                    {
                        var period = int.Parse(breadthFields[1]);
                        return GenerateCountIfBreadthSerie(instrument, fields[1], startDate, (serie, d) =>
                        {
                            var index = serie.IndexOf(d);
                            if (index == -1)
                                return false;

                            var emaIndicator = serie.GetIndicator($"EMA({period})")?.Series[0];
                            return emaIndicator != null && serie.Values[index].CLOSE > emaIndicator[index];

                        });
                    }
                case "STOK":
                    {
                        var period = int.Parse(breadthFields[1]);
                        return GenerateAverageBreadthSerie(instrument, fields[1], startDate, (serie, d) =>
                        {
                            var index = serie.IndexOf(d);
                            if (index == -1)
                                return float.NaN;

                            var emaIndicator = serie.GetIndicator($"STOKTURTLE({period},{period},6)")?.Series[0];
                            return emaIndicator != null ? (emaIndicator[index] - 50f) / 50f : float.NaN;

                        });
                    }
                default:
                    break;
            }

            return null;
        }

        /// <summary>
        /// Generate breadth serie to count number of instruments matching a predicate (e.g. number of instrument clse > EMA200)
        /// </summary>
        /// <param name="instrument"></param>
        /// <param name="groupName"></param>
        /// <param name="startDate"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public StockDailyValue[] GenerateCountIfBreadthSerie(StockInstrument instrument, string groupName, DateTime startDate, Func<DataSerie, DateTime, bool> predicate)
        {
            var indexName = groupName == "USA" ? "S&P 500" : "CAC40";
            var indiceInstrument = StockDictionary.GetInstrumentByName(indexName);
            if (indiceInstrument == null)
            {
                return null;
            }

            DataSerie indiceSerie = indiceInstrument.GetDefaultDataSerie();
            if (indiceSerie == null || indiceSerie.Values.Length == 0)
                return null;

            if (indiceSerie.LastCompleteValue.DATE == startDate)
                return null;

            var indexComponents = StockDictionary.Instruments.Values
                .Where(s => s.BelongsToGroup(groupName))
                .Select(s => s.GetDataSerie(indiceSerie.BarDuration))
                .Where(d => d != null & d.Count > 0).ToArray();

            if (indexComponents.Length == 0)
                return null;

            DateTime lastIndiceDate = indiceSerie.LastValue.DATE;
            DateTime lastBreadthDate = DateTime.MinValue;

            var bars = new List<StockDailyValue>();

            float val;
            int count;
            foreach (var bar in indiceSerie.Values.Where(v => v.DATE > lastBreadthDate))
            {
                val = 0; count = 0;
                this.NotifyProgress($"Generating {instrument.DisplayName} {bar.DATE.ToShortDateString()}");

                foreach (var serie in indexComponents)
                {
                    if (predicate(serie, bar.DATE))
                        val++;
                    count++;
                }
                if (count != 0)
                {
                    val /= (float)count;
                    bars.Add(new StockDailyValue(val, val, val, val, count, bar.DATE));
                }
            }
            return bars.ToArray();
        }


        /// <summary>
        /// Generate breadth serie to sum number of instruments matching a predicate (1 if matching, -1 if not matching) (e.g. number of instrument clse > EMA200)
        /// </summary>
        /// <param name="instrument"></param>
        /// <param name="groupName"></param>
        /// <param name="startDate"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public StockDailyValue[] GenerateSumIfBreadthSerie(StockInstrument instrument, string groupName, DateTime startDate, Func<DataSerie, DateTime, bool> predicate)
        {
            var indexName = groupName == "USA" ? "S&P 500" : "CAC40";
            var indiceInstrument = StockDictionary.GetInstrumentByName(indexName);
            if (indiceInstrument == null)
            {
                return null;
            }

            DataSerie indiceSerie = indiceInstrument.GetDefaultDataSerie();
            if (indiceSerie == null || indiceSerie.Values.Length == 0)
                return null;

            if (indiceSerie.LastCompleteValue.DATE == startDate)
                return null;

            var indexComponents = StockDictionary.Instruments.Values
                .Where(s => s.BelongsToGroup(groupName))
                .Select(s => s.GetDataSerie(indiceSerie.BarDuration))
                .Where(d => d != null & d.Count > 0).ToArray();

            if (indexComponents.Length == 0)
                return null;

            DateTime lastIndiceDate = indiceSerie.LastValue.DATE;
            DateTime lastBreadthDate = DateTime.MinValue;

            var bars = new List<StockDailyValue>();

            float val, summation = 0;
            int count;
            foreach (var bar in indiceSerie.Values.Where(v => v.DATE > lastBreadthDate))
            {
                val = 0; count = 0;
                this.NotifyProgress($"Generating {instrument.DisplayName} {bar.DATE.ToShortDateString()}");

                foreach (var serie in indexComponents)
                {
                    if (predicate(serie, bar.DATE))
                        val++;
                    else
                        val--;
                    count++;
                }
                if (count != 0)
                {
                    summation += val / (float)count;
                    bars.Add(new StockDailyValue(summation, summation, summation, summation, count, bar.DATE));
                }
            }
            return bars.ToArray();
        }

        /// <summary>
        /// Generate breadth serie as an average value for an insdicator (e.g. average on RSI value)
        /// </summary>
        /// <param name="instrument"></param>
        /// <param name="groupName"></param>
        /// <param name="startDate"></param>
        /// <param name="indicator"></param>
        /// <returns></returns>
        public StockDailyValue[] GenerateAverageBreadthSerie(StockInstrument instrument, string groupName, DateTime startDate, Func<DataSerie, DateTime, float> indicator)
        {
            var indexName = groupName == "USA" ? "S&P 500" : "CAC40";
            var indiceInstrument = StockDictionary.GetInstrumentByName(indexName);
            if (indiceInstrument == null)
            {
                return null;
            }

            DataSerie indiceSerie = indiceInstrument.GetDefaultDataSerie();
            if (indiceSerie == null || indiceSerie.Values.Length == 0)
                return null;

            if (indiceSerie.LastCompleteValue.DATE == startDate)
                return null;

            var indexComponents = StockDictionary.Instruments.Values
                .Where(s => s.BelongsToGroup(groupName))
                .Select(s => s.GetDataSerie(indiceSerie.BarDuration))
                .Where(d => d != null & d.Count > 0).ToArray();

            if (indexComponents.Length == 0)
                return null;

            DateTime lastIndiceDate = indiceSerie.LastValue.DATE;
            DateTime lastBreadthDate = DateTime.MinValue;

            var bars = new List<StockDailyValue>();

            float val;
            int count;
            foreach (var bar in indiceSerie.Values.Where(v => v.DATE > lastBreadthDate))
            {
                val = 0; count = 0;
                this.NotifyProgress($"Generating {instrument.DisplayName} {bar.DATE.ToShortDateString()}");

                foreach (var serie in indexComponents)
                {
                    var v = indicator(serie, bar.DATE);
                    if (float.IsNaN(v))
                        continue;
                    val += v;
                    count++;
                }
                if (count != 0)
                {
                    val /= (float)count;
                    bars.Add(new StockDailyValue(val, val, val, val, count, bar.DATE));
                }
            }
            return bars.ToArray();
        }
    }
}
