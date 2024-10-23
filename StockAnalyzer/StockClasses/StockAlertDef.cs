using System;
using System.Text.Json.Serialization;
using static StockAnalyzer.StockClasses.StockSerie;

namespace StockAnalyzer.StockClasses
{
    public enum AlertType
    {
        Group,
        Stock,
        Price
    }
    public class StockAlertDef
    {
        public StockAlertDef()
        {
            this.BarDuration = BarDuration.Daily;
            this.CreationDate = DateTime.MinValue;
            this.InReport = true;
        }

        public int Id { get; set; }
        public int Rank { get; set; }
        public AlertType Type { get; set; }
        public bool InReport { get; set; }
        public bool InAlert { get; set; }
        public bool CompleteBar { get; set; }

        public string Theme { get; set; }
        public string Title { get; set; }
        public Groups Group { get; set; }
        public DateTime CreationDate { get; set; }
        public string StockName { get; set; }
        public float PriceTrigger { get; set; }
        /// <summary>
        /// Minimum liquidity in M€. Ignored if equals to zero
        /// </summary>
        public float MinLiquidity { get; set; }
        public bool TriggerBrokenUp { get; set; }
        public BarDuration BarDuration { get; set; }

        public string IndicatorType { get; set; }
        public string IndicatorName { get; set; }
        [JsonIgnore]
        public string IndicatorFullName => IndicatorType + "|" + IndicatorName;
        public string EventName { get; set; }
        [JsonIgnore]
        public string EventFullName => EventName + "=>" + IndicatorFullName;

        public string FilterType { get; set; }
        public string FilterName { get; set; }
        public string FilterEventName { get; set; }
        [JsonIgnore]
        public string FilterFullName => FilterType == null || FilterName == null ? null : FilterType + "|" + FilterName;
        public BarDuration FilterDuration { get; set; } = new BarDuration();

        public string Stop { get; set; }
        public string Speed { get; set; }
        public int Stok { get; set; }

        public override string ToString()
        {
            return this.BarDuration.ToString() + ";" + EventFullName;
        }
    }
}
