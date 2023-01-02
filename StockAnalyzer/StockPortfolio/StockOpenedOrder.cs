using Newtonsoft.Json;
using StockAnalyzer.StockClasses;
using System;

namespace StockAnalyzer.StockPortfolio
{
    public class StockOpenedOrder
    {
        public StockOpenedOrder()
        {
        }

        public long Id { get; set; }
        public string StockName { get; set; }
        public string ISIN { get; set; }
        public long Uic { get; set; }

        public string BuySell { get; set; }
        public string OrderType { get; set; }

        public int Qty { get; set; }
        public float Value { get; set; }
        [JsonIgnore]
        public float Amount => Qty * Value;
        public float StopValue { get; set; }
        public DateTime CreationDate { get; set; }
        public string Status { get; set; }

        public BarDuration BarDuration { get; set; } = BarDuration.Daily;
        public string EntryComment { get; set; }
        public string Theme { get; set; }
    }
}