using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;


namespace StockAnalyzer.StockData.DataProviders
{
    [DebuggerDisplay("{Name}-{LastDate.ToString(\"dd/MM/yy\")}")]
    public class InstrumentDownloadHistory
    {
        public string Id { get; set; }
        public DateTime LastDate { get; set; }
        public DateTime DownloadDate { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public string Market { get; set; }


        static CultureInfo frenchCulture = CultureInfo.GetCultureInfo("fr-FR");

        public InstrumentDownloadHistory(StockInstrument instrument, DateTime lastDate, DateTime downloadDate)
        {
            Id = instrument.Id;
            LastDate = lastDate;
            DownloadDate = downloadDate;
            Name = instrument.DisplayName;
            Group = instrument.Group.ToString();
            Market = instrument.Market.ToString();
        }

        private InstrumentDownloadHistory(string id, DateTime lastDate, DateTime downloadDate, string name, string group, string market)
        {
            Id = id;
            LastDate = lastDate;
            DownloadDate = downloadDate;
            Name = name;
            Group = group;
            Market = market;
        }

        static public List<InstrumentDownloadHistory> Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                var lines = File.ReadAllLines(fileName);
                return lines.Select(l => l.Split(','))
                    .Select(f => new InstrumentDownloadHistory(
                        f[0],
                        DateTime.Parse(f[1], frenchCulture),
                        DateTime.Parse(f[2], frenchCulture),
                        f[3],
                        f[4],
                        f[5])).ToList();
            }
            else
            {
                return new List<InstrumentDownloadHistory>();
            }
        }

        static public void Save(string fileName, List<InstrumentDownloadHistory> history)
        {
            File.WriteAllLines(fileName, history.Select(h => $"{h.Id},{h.LastDate.ToString(frenchCulture)}, {h.DownloadDate.ToString(frenchCulture)},{h.Name},{h.Group},{h.Market}"));
        }
    }
}