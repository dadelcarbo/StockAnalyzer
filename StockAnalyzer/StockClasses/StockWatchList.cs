using StockAnalyzer.StockHelpers;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace StockAnalyzer.StockClasses
{
    public class StockWatchList : IPersistable
    {
        public string Name { get; set; }
        public bool Report { get; set; }
        public SortedSet<string> StockList { get; set; }

        public StockWatchList()
        {
            this.Name = string.Empty;
            this.StockList = new SortedSet<string>();
        }
        public StockWatchList(string name)
        {
            this.Name = name;
            this.StockList = new SortedSet<string>();
        }

        static public List<StockWatchList> WatchLists { get; private set; }

        public static void Load(string fileName)
        {
            if (!File.Exists(fileName))
                WatchLists = new List<StockWatchList>();
            else
                WatchLists = JsonSerializer.Deserialize<List<StockWatchList>>(File.ReadAllText(fileName));
        }

        public static void Save(string fileName)
        {
            var jsonData = JsonSerializer.Serialize(WatchLists, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(fileName, jsonData);
        }

    }
}
