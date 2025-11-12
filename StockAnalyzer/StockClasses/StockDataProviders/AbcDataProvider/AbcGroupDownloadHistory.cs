using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockDataProviders.AbcDataProvider
{
    [DebuggerDisplay("{Name}-{LastDownload.ToString(\"dd/MM/yy hh:mm:ss\")}")]
    public class AbcGroupDownloadHistory
    {
        public string Name { get; set; }
        public DateTime LastDownload { get; set; }
        public DateTime NextDownload { get; set; }

        public AbcGroupDownloadHistory(string name, DateTime lastDownload, DateTime nextDownload)
        {
            Name = name;
            LastDownload = lastDownload;
            NextDownload = nextDownload;
        }

        static public List<AbcGroupDownloadHistory> Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                var lines = File.ReadAllLines(fileName);
                return lines.Select(l => l.Split(','))
                    .Select(f => new AbcGroupDownloadHistory(
                        f[0],
                        DateTime.Parse(f[1]),
                        DateTime.Today
                        )).ToList();
            }
            else
            {
                return new List<AbcGroupDownloadHistory>();
            }
        }

        static public void Save(string fileName, List<AbcGroupDownloadHistory> history)
        {
            File.WriteAllLines(fileName, history.Select(h => $"{h.Name},{h.LastDownload},{h.NextDownload}"));
        }
    }
}
