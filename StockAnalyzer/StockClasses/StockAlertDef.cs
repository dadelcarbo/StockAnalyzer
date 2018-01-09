using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockAnalyzer.StockClasses
{
    public class StockAlertDef
    {
        public StockAlertDef()
        {

        }
        public StockAlertDef(StockSerie.StockBarDuration barDuration, string indicatorType, string indicatorName, string eventName)
        {
            this.BarDuration = barDuration;
            this.IndicatorType = indicatorType;
            this.IndicatorName = indicatorName;
            this.EventName = eventName;
        }
        public StockSerie.StockBarDuration BarDuration { get; set; }
        public string IndicatorType { get; set; }
        public string IndicatorName { get; set; }
        public string EventName { get; set; }

        public string EventFullName
        {
            get { return IndicatorFullName + "=>" + EventName; }
        }

        public string IndicatorFullName { get { return IndicatorType + "|" + IndicatorName; } }

        public override string ToString()
        {
            return this.BarDuration + ";" + EventFullName;
        }
    }

}
