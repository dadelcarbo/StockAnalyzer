using StockAnalyzer.StockClasses;
using System;

namespace StockAnalyzer.StockPortfolio.AutoTrade
{
    public class TradePosition
    {
        public long Id { get; set; }
        public DateTime OpenDate { get; set; }
        public StockSerie StockSerie { get; set; }
        public int Qty { get; set; }
        public float TheoriticalOpenValue { get; set; }
        public float ActualOpenValue { get; set; }
        public float? Stop { get; set; }

        // For closed positions
        public DateTime? CloseDate { get; set; }
        public float? TheoriticalCloseValue { get; set; }
        public float? ActualCloseValue { get; set; }
    }
}