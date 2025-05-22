using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILMCCLELLAN : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draw a trail stop that is calculated based on the lower line of the McClellan convergence indicator";

        public override string[] ParameterNames => new string[] { "FastPeriod", "SlowPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 19, 39 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var mcClellanIndicator = stockSerie.GetIndicator($"MCCLELLANCONV({this.Parameters[0]},{this.Parameters[1]})");

            var lowSerie = mcClellanIndicator.Series[3];
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var longStopSerie = new FloatSerie(stockSerie.Count, "TRAILMCCLELLAN.LS", float.NaN);
            var shortStopSerie = new FloatSerie(stockSerie.Count, "TRAILMCCLELLAN.SS", float.NaN);
            var isBull = closeSerie[1] > lowSerie[1];
            if (isBull) { longStopSerie[1] = lowSerie[1]; } else { shortStopSerie[1] = lowSerie[1]; }

            for (int i = 2; i < stockSerie.Count; i++)
            {
                if (isBull)
                {
                    if (closeSerie[i] > lowSerie[i])
                    {
                        longStopSerie[i] = lowSerie[i];
                    }
                    else
                    {
                        isBull = false;
                    }
                }
                else
                {
                    if (lowSerie[i - 1] <= lowSerie[i] && closeSerie[i] > lowSerie[i])
                    {
                        longStopSerie[i] = lowSerie[i];
                        isBull = true;
                    }
                }
            }

            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }
    }
}
