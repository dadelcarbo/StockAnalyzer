using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILPERCENT : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Trail stop calculated when exceeding a treeshold from previous low percentage and selling when falling below the highest minus percentage";

        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }

        public override string[] ParameterNames => new string[] { "BuyThreshold", "SellThreshold" };
        public override Object[] ParameterDefaultValues => new Object[] { 0.15f, 0.15f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeFloat(0f, 1f), new ParamRangeFloat(0f, 1f) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, this.SerieNames[1], float.NaN);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            float upPercent = 1f + (float)this.parameters[0];
            float downPercent = 1f - (float)this.parameters[1];
            int lookBack = 20;

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            bool upTrend = true;
            float trail = closeSerie.GetMin(0, lookBack);
            for (int i = 0; i < lookBack; i++)
            {
                longStopSerie[i] = trail;
            }
            for (int i = lookBack; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] < trail)
                    {
                        upTrend = false;
                        trail = closeSerie[i] * upPercent;
                        shortStopSerie[i] = trail;
                    }
                    else
                    {
                        longStopSerie[i] = trail = Math.Max(trail, closeSerie[i] * downPercent);
                    }
                }
                else
                {
                    if (closeSerie[i] > trail)
                    {
                        upTrend = true;
                        trail = closeSerie[i] * downPercent;
                        longStopSerie[i] = trail;
                    }
                    else
                    {
                        shortStopSerie[i] = trail = Math.Min(trail, closeSerie[i] * upPercent);
                    }
                }
            }

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }
    }
}
