using System;
using System.Collections.Generic;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILATRBAND : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "NbUpDev", "NbDownDev", "MAType" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 3.0f, -3.0f, "MA" }; }
        }
        static List<string> emaTypes = new List<string>() { "EMA", "HMA", "MA", "EA" };
        public override ParamRange[] ParameterRanges
        {
            get
            {
                return new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(0f, 20.0f),
                new ParamRangeFloat(-20.0f, 0.0f),
                new ParamRangeMA()
                };
            }
        }
        public override string[] SerieNames { get { return new string[] { "TRAILATRBAND.S", "TRAILATRBAND.R" }; } }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;

            IStockIndicator bbIndicator = stockSerie.GetIndicator(this.Name.Replace("TRAIL", ""));
            stockSerie.CalculateBBTrailStop(bbIndicator.Series[1], bbIndicator.Series[0], out longStopSerie, out shortStopSerie);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }

    }
}
