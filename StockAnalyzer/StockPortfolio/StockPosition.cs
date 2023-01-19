using Newtonsoft.Json;
using StockAnalyzer.StockClasses;
using System.Collections.Generic;

namespace StockAnalyzer.StockPortfolio
{
    public class StockPosition : StockPositionBase
    {
        public string LimitOrderId { get; set; }
    }
}