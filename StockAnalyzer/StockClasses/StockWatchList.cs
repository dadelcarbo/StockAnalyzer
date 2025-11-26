using StockAnalyzer.StockHelpers;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace StockAnalyzer.StockClasses
{
    public class StockWatchList : IPersistable
    {
        public string Name { get; set; }
        public List<string> StockList { get; set; }

        public StockWatchList()
        {
            this.Name = string.Empty;
            this.StockList = new List<string>();
        }
        public StockWatchList(string name)
        {
            this.Name = name;
            this.StockList = new List<string>();
        }

        public static List<StockWatchList> Load(string fileName)
        {
            if (!File.Exists(fileName))
                return new List<StockWatchList>();
            return JsonSerializer.Deserialize<List<StockWatchList>>(System.IO.File.ReadAllText(fileName));
        }

        public static void Save(string fileName, List<StockWatchList> watchLists)
        {
            var jsonData = JsonSerializer.Serialize(watchLists, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(fileName, jsonData);
        }

    }
}
