using System;
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
            this.BarDuration = StockBarDuration.Daily;
            this.CreationDate = DateTime.MinValue;
            this.Active = true;
        }

        public int Id { get; set; }
        public AlertType Type { get; set; }
        public bool Active { get; set; }

        public string Theme { get; set; }
        public string Title { get; set; }
        public Groups Group { get; set; }
        public DateTime CreationDate { get; set; }
        public string StockName { get; set; }
        public float PriceTrigger { get; set; }
        public bool TriggerBrokenUp { get; set; }
        public StockBarDuration BarDuration { get; set; }

        public string IndicatorType { get; set; }
        public string IndicatorName { get; set; }
        public string IndicatorFullName { get { return IndicatorType + "|" + IndicatorName; } }
        public string EventName { get; set; }
        public string EventFullName
        {
            get { return EventName + "=>" + IndicatorFullName; }
        }

        public string FilterType { get; set; }
        public string FilterName { get; set; }
        public string FilterEventName { get; set; }
        public string FilterFullName { get { return FilterType == null || FilterName == null ? null : FilterType + "|" + FilterName; } }

        public string Stop { get; set; }

        public override string ToString()
        {
            return this.BarDuration.ToString() + ";" + EventFullName;
        }
    }
}
