using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILROR : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draw a trail stop that Starts Up Trend when price reaches ROR trigger, then trail using a ratio from current ROR";

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;
        public override string[] ParameterNames => new string[] { "Period", "Trigger", "DrawdownRatio" };

        public override Object[] ParameterDefaultValues => new Object[] { 30, 0.2f, 0.681f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0, 5), new ParamRangeFloat(0, 1) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, float.NaN);

            var rorPeriod = (int)this.parameters[0];
            var trigger = (float)this.parameters[1];
            var ratio = (float)this.parameters[2];
            var rorSerie = stockSerie.GetIndicator($"ROR({rorPeriod})").Series[0];

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            float trailStop = float.NaN;
            for (int i = 1; i < stockSerie.Count; i++)
            {
                if (float.IsNaN(trailStop))
                {
                    if (rorSerie[i] > trigger)
                    {
                        trailStop = closeSerie[i] * (1 - rorSerie[i] * ratio);
                        longStopSerie[i] = trailStop;
                    }
                }
                else
                {
                    trailStop = Math.Max(trailStop, closeSerie[i] * (1 - rorSerie[i] * ratio));
                    if (closeSerie[i] > trailStop)
                    {
                        longStopSerie[i] = trailStop;
                    }
                    else
                    {
                        trailStop = float.NaN;
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
