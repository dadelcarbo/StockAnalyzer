using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockDataProviders.AbcDataProvider
{
    [DebuggerDisplay("{Name}-{LastDate.ToString(\"dd/MM/yy\")}")]
    public class AbcDownloadHistory
    {
        public string Id { get; set; }
        public DateTime LastDate { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }

        public AbcDownloadHistory(string id, DateTime lastDate, string name, string group)
        {
            Id = id;
            LastDate = lastDate;
            Name = name;
            Group = group;
        }

        static public List<AbcDownloadHistory> Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                var lines = File.ReadAllLines(fileName);
                return lines.Select(l => l.Split(','))
                    .Select(f => new AbcDownloadHistory(
                        f[0],
                        DateTime.ParseExact(f[1], "yyyy/MM/dd", CultureInfo.InvariantCulture),
                        f[2],
                        f[3])).ToList();
            }
            else
            {
                return new List<AbcDownloadHistory>();
            }
        }

        static public void Save(string fileName, List<AbcDownloadHistory> history)
        {
            File.WriteAllLines(fileName, history.Select(h => $"{h.Id},{h.LastDate:yyyy/MM/dd},{h.Name},{h.Group}"));
        }
    }
}
