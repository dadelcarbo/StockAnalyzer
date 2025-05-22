using System;

namespace StockAnalyzer.StockPortfolio
{
    public class SaxoPositionChange
    {
        public long OrderId { get; set; }
        public float Value { get; set; }
        public int Qty { get; set; }
        public DateTime Date { get; set; }
    }
}