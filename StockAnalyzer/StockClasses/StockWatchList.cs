using System.Collections.Generic;

namespace StockAnalyzer.StockClasses
{
    public class StockWatchList
    {
        public string Name { get; set; }
        public List<string> StockList{ get; set; }

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
    }
}
