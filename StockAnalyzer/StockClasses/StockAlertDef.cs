﻿using System;

namespace StockAnalyzer.StockClasses
{
    public class StockAlertDef
    {
        public StockAlertDef()
        {
            this.BarDuration = StockBarDuration.Daily;
            this.CreationDate = DateTime.MinValue;
        }

        public DateTime CreationDate { get; set; }
        public string StockName { get; set; }
        public float PriceTrigger { get; set; }
        public bool TriggerBrokenUp { get; set; }
        public StockBarDuration BarDuration { get; set; }
        public string IndicatorType { get; set; }
        public string IndicatorName { get; set; }
        public string EventName { get; set; }

        public string FilterType { get; set; }
        public string FilterName { get; set; }
        public string FilterEventName { get; set; }

        public string EventFullName
        {
            get { return IndicatorFullName + "=>" + EventName; }
        }

        public string IndicatorFullName { get { return IndicatorType + "|" + IndicatorName; } }
        public string FilterFullName { get { return FilterType == null || FilterName == null ? null : FilterType + "|" + FilterName; } }

        public override string ToString()
        {
            return this.BarDuration.ToString() + ";" + EventFullName;
        }
    }
}
