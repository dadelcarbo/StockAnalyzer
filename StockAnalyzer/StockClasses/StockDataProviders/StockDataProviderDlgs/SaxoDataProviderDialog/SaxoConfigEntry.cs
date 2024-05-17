using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog
{
    public class SaxoConfigEntry
    {
        public SaxoConfigEntry()
        {
        }
        public string ISIN { get; set; }
        public string StockName { get; set; }

        public static IEnumerable<SaxoConfigEntry> LoadFromFile(string fileName)
        {
            string line;
            var entries = new List<SaxoConfigEntry>();
            if (File.Exists(fileName))
            {
                using var sr = new StreamReader(fileName, true);
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                    var row = line.Split(',');

                    entries.Add(new SaxoConfigEntry() // 8894,CC,FUT_COM_COCOA,FUTURE
                    {
                        ISIN = row[0],
                        StockName = row[1]
                    });
                }
            }
            return entries;
        }

        public static void SaveToFile(IEnumerable<SaxoConfigEntry> entries, string fileName)
        {
            using var sr = new StreamWriter(fileName, false);
            foreach (var entry in entries.OrderBy(e => e.StockName))
            {
                sr.WriteLine(
                    entry.ISIN + "," +
                    entry.StockName
                    );
            }
        }

        public static void RemoveEntry(string Isin, string fileName)
        {
            try
            {
                var entries = LoadFromFile(fileName);
                SaveToFile(entries.Where(e => e.ISIN != Isin), fileName);
            }
            catch (Exception ex)
            {
                StockLogging.StockLog.Write(ex);
            }
        }
    }

}
