using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockScreeners
{
    public class StockScreener_TRAILATR : StockScreenerBase
    {
        public override string Definition
        {
            get { return "Detect opportunities in bullish in TRAILATR for a short period of time"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "ATRPeriod", "NbUpDev", "NbDownDev", "MAType", "Duration" }; }
        }
        public override object[] ParameterDefaultValues
        {
            get { return new object[] { 20, 10, 3.0f, -3.0f, "EMA", 10 }; }
        }
        static List<string> emaTypes = new List<string>() { "EMA", "MA", "EA" };
        public override ParamRange[] ParameterRanges
        {
            get
            {
                return new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(-5.0f, 20.0f),
                new ParamRangeFloat(-20.0f, 5.0f),
                new ParamRangeMA(),
                new ParamRangeInt(1, 500)
                };
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // "TRAILATR(175,60,4,1,EMA)"

            var duration = (int)parameters[5];

            var longSerie = stockSerie.GetTrailStop(this.Name).Series[0];

            // Detecting events
            CreateEventSeries(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            Match[stockSerie.LastIndex] = !float.IsNaN(longSerie[stockSerie.LastIndex]) && float.IsNaN(longSerie[stockSerie.LastIndex - duration]);
        }

    }
}
