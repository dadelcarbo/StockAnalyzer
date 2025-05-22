using StockAnalyzer.StockClasses;
using System;
using System.Text.Json.Serialization;

namespace StockAnalyzer.StockPortfolio.AutoTrade
{
    public class TradePosition
    {
        public long Id { get; set; }
        public DateTime OpenDate { get; set; }
        [JsonIgnore]
        public StockSerie StockSerie { get; set; }
        public int Qty { get; set; }
        public float TheoriticalOpenValue { get; set; }
        public float ActualOpenValue { get; set; }
        public float? Stop { get; set; }

        // For closed positions
        public DateTime? CloseDate { get; set; }
        public float? TheoriticalCloseValue { get; set; }
        public float? ActualCloseValue { get; set; }

        public override string ToString()
        {
            return $"Open: {OpenDate}-{Qty}@{ActualOpenValue}" + CloseDate == null ? string.Empty : $" Close: {CloseDate}-{Qty}@{ActualOpenValue}";
        }
    }
}