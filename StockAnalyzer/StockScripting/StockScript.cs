using StockAnalyzer.StockHelpers;

namespace StockAnalyzer.StockScripting
{
    public class StockScript : IPersistable
    {
        private static StockScript empty = new StockScript();
        public static StockScript Empty => empty;
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
    }
}
