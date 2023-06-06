using StockAnalyzer.Saxo.OpenAPI.TradingServices;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace StockAnalyzer.StockPortfolio
{
    public class StockNetPosition : StockPositionBase
    {
        public List<long> EntryOrderIds { get; set; } = new List<long>();
        public List<long> ExitOrderIds { get; set; } = new List<long>();
    }

    public class SaxoPositionChange
    {
        public long OrderId { get; set; }
        public float Value { get; set; }
        public int Qty { get; set; }
        public DateTime Date { get; set; }
    }
}