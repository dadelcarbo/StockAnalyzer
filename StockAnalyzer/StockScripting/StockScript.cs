using StockAnalyzer.StockHelpers;

namespace StockAnalyzer.StockScripting
{
    public class StockScript : IPersistable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
    }
}
