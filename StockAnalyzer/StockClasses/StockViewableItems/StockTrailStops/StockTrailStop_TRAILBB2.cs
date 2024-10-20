using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILBB2 : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string[] ParameterNames => new string[] { "Period", "NbUpDev", "NbDownDev", "MAType" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 2.0f, -2.0f, "MA" };
        static readonly List<string> emaTypes = StockIndicatorMovingAvgBase.MaTypes;
        public override ParamRange[] ParameterRanges => new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(-5.0f, 20.0f),
                new ParamRangeFloat(-20.0f, 5.0f),
                new ParamRangeMA()
                };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;

            IStockIndicator bbIndicator = stockSerie.GetIndicator(this.Name.Replace("TRAILBB2", "BB"));
            stockSerie.CalculateBandTrailStop2(bbIndicator.Series[1], bbIndicator.Series[0], out longStopSerie, out shortStopSerie);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }

    }
}
