using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILATR2 : StockTrailStopBase
    {
        public override string Definition => "Draws Trail Stop based ATR Bands";

        public override string[] ParameterNames => new string[] { "Period", "ATRPeriod", "NbUpDev", "NbDownDev", "MAType", "SignalPeriod" };
                public override Object[] ParameterDefaultValues => new Object[] { 30, 10, 2f, -2f, "EMA",3 };
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
            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;

            var bandIndicator = stockSerie.GetIndicator($"ATRBAND({(int)this.parameters[0]},{(int)this.parameters[1]},{(float)this.parameters[2]},{(float)this.parameters[3]},{this.parameters[4]},{this.parameters[5]})");
            stockSerie.CalculateBandTrailStop2(bandIndicator.Series[1], bandIndicator.Series[0], out longStopSerie, out shortStopSerie);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }

    }
}
