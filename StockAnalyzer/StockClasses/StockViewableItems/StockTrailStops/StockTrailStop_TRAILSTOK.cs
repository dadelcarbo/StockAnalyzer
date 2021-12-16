using System;
using System.Linq;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILSTOK : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draw a trail stop that starts when making a new high and hold while it remains above overbought level";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string[] ParameterNames => new string[] { "Period", "Overbought" };

        public override Object[] ParameterDefaultValues => new Object[] { 35, 0.75f };

        public override ParamRange[] ParameterRanges =>
            new ParamRange[] {
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(0f, 100.0f),
            };

        public override string[] SerieNames => new string[] { "TRAILSTOK.LS", "TRAILSTOK.SS" };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, "TRAILSTOK.LS", float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, "TRAILSTOK.SS", float.NaN);

            int period = (int)this.parameters[0];
            float threeshold = (float)this.parameters[1];

            if (stockSerie.Count <= period)
            {
                // Generate events
                this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
                return;
            }

            var bodyHighSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);
            var bodyLowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            FloatSerie highestSerie = stockSerie.GetIndicator($"MDH({period})").Series[0];
            FloatSerie lowestSerie = stockSerie.GetIndicator($"MDH({period})").Series[2];
            FloatSerie threesholdSerie = (1.0f - threeshold) * lowestSerie + threeshold * highestSerie;

            float longTrail = float.NaN;
            float shortTrail = float.NaN;
            for (int i = period; i < stockSerie.Count; i++)
            {
                if (float.IsNaN(longTrail))
                {
                    if (threesholdSerie[i] > threesholdSerie[i - 1])
                    {
                        longTrail = threesholdSerie[i];
                    }
                }
                else
                {
                    if (closeSerie[i] < longTrail)
                    {
                        longTrail = float.NaN;
                    }
                    else
                    {
                        longTrail = threesholdSerie[i];
                    }
                }
                longStopSerie[i] = longTrail;

                //if (showShort)
                //{
                //    if (float.IsNaN(shortTrail))
                //    {
                //        if (highSerie[i] < emaSerie[i] && closeSerie[i] < openSerie[i])
                //        {
                //            shortTrail = emaSerie[i] + atrSerie[i];
                //        }
                //    }
                //    else
                //    {
                //        if (closeSerie[i] > shortTrail)
                //        {
                //            shortTrail = float.NaN;
                //        }
                //        else
                //        {
                //            shortTrail = Math.Min(emaSerie[i] + atrSerie[i], shortTrail);
                //        }
                //        //if (lowSerie[i] < emaSerie[i])
                //        //{
                //        //    trail = Math.Max(lowSerie[i] - atrSerie[i], trail);
                //        //}
                //    }
                //    shortStopSerie[i] = shortTrail;
                //}
            }

            this.Series[0] = longStopSerie;
            this.Series[0].Name = longStopSerie.Name;
            this.Series[1] = shortStopSerie;
            this.Series[1].Name = shortStopSerie.Name;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }
    }
}