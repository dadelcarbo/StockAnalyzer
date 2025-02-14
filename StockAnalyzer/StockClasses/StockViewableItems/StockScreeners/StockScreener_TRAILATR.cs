using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;
using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockScreeners
{
    public class StockScreener_TRAILATR : StockScreenerBase
    {
        public override string Definition => "Detect opportunities in bullish in TRAILATR for a short period of time";
        public override string[] ParameterNames => new string[] { "Period", "ATRPeriod", "NbUpDev", "NbDownDev", "MAType", "Duration" };
        public override object[] ParameterDefaultValues => new object[] { 20, 10, 3.0f, -3.0f, "EMA", 10 };
        static readonly List<string> emaTypes = StockIndicatorMovingAvgBase.MaTypes;
        public override ParamRange[] ParameterRanges => new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(-5.0f, 20.0f),
                new ParamRangeFloat(-20.0f, 5.0f),
                new ParamRangeMA(),
                new ParamRangeInt(1, 500)
                };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var duration = (int)parameters[5];

            var longSerie = stockSerie.GetTrailStop(this.Name).Series[0];

            // Detecting events
            CreateEventSeries(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            Match[stockSerie.LastIndex] = !float.IsNaN(longSerie[stockSerie.LastIndex]) && float.IsNaN(longSerie[stockSerie.LastIndex - duration]);
        }

    }
}
