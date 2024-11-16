using System;
using System.Text.Json.Serialization;

namespace StockAnalyzer.StockPortfolio
{
    public class StockOrderBase
    {
        public long Id { get; set; }

        public string StockName { get; set; }
        public string ISIN { get; set; }
        public long Uic { get; set; }

        public string BuySell { get; set; }
        public string OrderType { get; set; }

        public string Status { get; set; }

        public int Qty { get; set; }
        public float Value { get; set; }

        [JsonIgnore]
        public float Amount => Qty * Value;

        public DateTime CreationDate { get; set; }
    }
}