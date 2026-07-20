using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockPortfolio
{
    public class StockOrder
    {
        private SaxoOrder saxoOrder;
        public StockOrder(SaxoOrder saxoOrder)
        {
            this.saxoOrder = saxoOrder;
        }

        public int Qty { get; set; }
        public float? Price;

        public BarDuration BarDuration { get; set; } = BarDuration.Daily;
        public string EntryComment { get; set; }
        public string Theme { get; set; }
        public string StockName { get; set; }
        public string Isin { get; set; }
    }
}