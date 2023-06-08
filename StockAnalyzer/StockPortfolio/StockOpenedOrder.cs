using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockPortfolio
{
    public class StockOpenedOrder : StockOrderBase
    {
        public StockOpenedOrder()
        {
            this.IsActive = true;
        }

        public float StopValue { get; set; }

        public BarDuration BarDuration { get; set; } = BarDuration.Daily;
        public string EntryComment { get; set; }
        public string Theme { get; set; }
    }
}