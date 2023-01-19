using System.Collections.Generic;

namespace StockAnalyzer.StockPortfolio
{
    public class StockNetPosition : StockPositionBase
    {
        public List<long> EntryOrderIds { get; set; } = new List<long>();
        public List<long> ExitOrderIds { get; set; } = new List<long>();
    }
}