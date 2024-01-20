using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs
{
    public class InvestingConfigEntry
    {
        public InvestingConfigEntry(long ticker)
        {
            this.Ticker = ticker;
        }
        public long Ticker { get; private set; }
        public string ShortName { get; set; }
        public string StockName { get; set; }
        public string Group { get; set; }

        public static List<InvestingConfigEntry> LoadFromFile(string fileName)
        {
            string line;
            var entries = new List<InvestingConfigEntry>();
            if (File.Exists(fileName))
            {
                using var sr = new StreamReader(fileName, true);
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                    var row = line.Split(',');

                    entries.Add(new InvestingConfigEntry(long.Parse(row[0])) // 8894,CC,FUT_COM_COCOA,FUTURE
                    {
                        ShortName = row[1],
                        StockName = row[2],
                        Group = row[3]
                    });
                }
            }
            return entries;
        }

        public static void SaveToFile(IList<InvestingConfigEntry> entries, string fileName)
        {
            using var sr = new StreamWriter(fileName, false);
            foreach (var entry in entries.OrderBy(e => e.Group).ThenBy(e => e.StockName))
            {
                sr.WriteLine(
                    entry.Ticker + "," +
                    entry.ShortName + "," +
                    entry.StockName + "," +
                    entry.Group
                    );
            }
        }
    }
}
