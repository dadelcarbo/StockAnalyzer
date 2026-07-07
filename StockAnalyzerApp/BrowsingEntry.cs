using StockAnalyzer.StockClasses;
using StockAnalyzerApp.StockData;

namespace StockAnalyzerApp
{
    public class BrowsingEntry
    {
        public StockInstrument Instrument { get; set; }
        public BarDuration BarDuration { get; set; }
        public string Theme { get; set; }
    }
}
