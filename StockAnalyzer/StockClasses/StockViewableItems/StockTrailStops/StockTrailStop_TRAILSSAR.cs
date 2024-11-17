using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILSSAR : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Calculate a trail that increases based on each bar with the percentage given in parameter";
        public override string[] ParameterNames => new string[] { "Step" };

        public override Object[] ParameterDefaultValues => new Object[] { 0.1f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeFloat(0.001f, 20f) };
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;

            float step = (float)this.parameters[0] / 100f;

            stockSerie.CalculateSSAR(step, out longStopSerie, out shortStopSerie, 1);

            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }
    }
}
