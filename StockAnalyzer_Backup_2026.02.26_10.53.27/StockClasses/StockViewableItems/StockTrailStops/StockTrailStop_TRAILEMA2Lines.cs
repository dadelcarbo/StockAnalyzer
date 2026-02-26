using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILEMA2Lines : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draw a trail stop that is based on a EMA 2 Lines Cloud.";

        public override string[] ParameterNames => new string[] { "FastPeriod", "SlowPeriod" };
        public override Object[] ParameterDefaultValues => new Object[] { 6, 35 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, "TRAILEMA2Lines.LS", float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, "TRAILEMA2Lines.SS", float.NaN);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var fastEma = stockSerie.GetIndicator($"EMA({(int)this.Parameters[0]})").Series[0];
            var slowEma = stockSerie.GetIndicator($"EMA({(int)this.Parameters[1]})").Series[0];

            bool bull = false;
            for (int i = 1; i < stockSerie.Count; i++)
            {
                if (bull)
                {
                    if (fastEma[i] > slowEma[i])
                    {
                        longStopSerie[i] = slowEma[i];
                    }
                    else
                    {
                        bull = false;
                    }
                }
                else
                {
                    if (fastEma[i] > slowEma[i])
                    {
                        bull = true;
                        longStopSerie[i] = slowEma[i];
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
