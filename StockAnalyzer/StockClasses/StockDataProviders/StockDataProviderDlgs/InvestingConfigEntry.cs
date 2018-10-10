using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs
{
    public class InvestingConfigEntry : INotifyPropertyChanged
    {
        public long Ticker { get; set; }
        public string ShortName { get; set; }
        public string StockName { get; set; }
        public string Group { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public static List<InvestingConfigEntry> LoadFromFile(string fileName)
        {
            string line;
            var entries = new List<InvestingConfigEntry>();
            if (File.Exists(fileName))
            {
                using (var sr = new StreamReader(fileName, true))
                {
                    sr.ReadLine(); // Skip first line
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                        var row = line.Split(',');

                        entries.Add(new InvestingConfigEntry() { Ticker = long.Parse(row[0]), ShortName = row[1], StockName = row[2], Group = row[3] });
                        // 8894,CC,FUT_COM_COCOA,FUTURE
                    }
                }
            }
            return entries;
        }
    }
}
