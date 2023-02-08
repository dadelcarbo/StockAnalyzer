using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace UltimateChartist.DataModels
{
    public class StockBar
    {
        public DateTime Date { get; }
        public double Open { get; }
        public double High { get; }
        public double Low { get; }
        public double Close { get; }
        public long Volume { get; }

        public StockBar(DateTime date, double open, double high, double low, double close, long volume)
        {
            Date = date;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
        }

        #region CSV FILE IO

        static protected CultureInfo usCulture = CultureInfo.GetCultureInfo("en-US");
        const char SEPARATOR = ';';
        const string Header = "Date;Open;High;Low;Close;Volume";

        public static StockBar Parse(string csvLine, DateTime? startDate = null)
        {
            try
            {
                var row = csvLine.Split(SEPARATOR);
                DateTime date = DateTime.Parse(row[0]);
                if (startDate != null && date < startDate)
                    return null;
                return new StockBar(date,
                   double.Parse(row[1], usCulture),
                   double.Parse(row[2], usCulture),
                   double.Parse(row[3], usCulture),
                   double.Parse(row[4], usCulture),
                   long.Parse(row[5], usCulture));
            }
            catch (Exception) { return null; }
        }

        /// <summary>
        /// Load a csv file containg bar data. First line is a header
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public static List<StockBar> Load(string fileName, DateTime? startDate = null)
        {
            if (File.Exists(fileName))
            {
                List<StockBar> bars = new List<StockBar>();
                foreach (var line in File.ReadAllLines(fileName).Skip(1))
                {
                    var readValue = StockBar.Parse(line, startDate);
                    if (readValue != null)
                    {
                        bars.Add(readValue);
                    }
                }
                return bars;
            }
            else
            {
                return null;
            }
        }

        public static void SaveCsv(List<StockBar> bars, string fileName, DateTime? startDate = null, DateTime? endDate = null)
        {
            var selectedBars = bars.AsEnumerable();
            if (startDate != null)
            {
                selectedBars = selectedBars.Where(b => b.Date >= startDate);
            }
            if (endDate != null)
            {
                selectedBars = selectedBars.Where(b => b.Date <= endDate);
            }
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.WriteLine(StockBar.Header);
                if (selectedBars.Any())
                {
                    sw.Write(selectedBars.Select(b => b.ToCsvString()).Aggregate((i, j) => i + Environment.NewLine + j));
                }
            }
        }

        public string ToCsvString()
        {
            return $"{this.Date}{SEPARATOR}{this.Open.ToString(usCulture)}{SEPARATOR}{this.High.ToString(usCulture)}{SEPARATOR}{this.Low.ToString(usCulture)}{SEPARATOR}{this.Close.ToString(usCulture)}{SEPARATOR}{this.Volume}";
        }

        #endregion

    }
}
