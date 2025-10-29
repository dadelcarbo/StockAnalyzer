using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class AbcDownloadHistory
    {
        public string Id { get; set; }
        public DateTime LastDate { get; set; }

        public AbcDownloadHistory(string id, DateTime lastDate)
        {
            Id = id;
            LastDate = lastDate;
        }

        static public List<AbcDownloadHistory> Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                var lines = File.ReadAllLines(fileName);
                return lines.Select(l => l.Split(','))
                    .Select(f => new AbcDownloadHistory(
                        f[0],
                        DateTime.ParseExact(f[1], "yy/MM/dd", CultureInfo.InvariantCulture))).ToList();
            }
            else
            {
                return new List<AbcDownloadHistory>();
            }
        }

        static public void Save(string fileName, List<AbcDownloadHistory> history)
        {
            File.WriteAllLines(fileName, history.Select(h => $"{h.Id},{h.LastDate:yy/MM/dd}"));
        }
    }
}
