using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILBARPERCENT : StockTrailStopBase
    {
        public override string Definition => "Draws Trail Stop following trend with a given percentage per bar";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;
        public override string[] ParameterNames => new string[] { "Percent" };

        public override Object[] ParameterDefaultValues => new Object[] { 0.01f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeFloat(0.001f, 20f) };
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;
            float progress = 1.0f + (float)this.Parameters[0];

            longStopSerie = new FloatSerie(stockSerie.Count, "TRAILBARPERCENT.LS", float.NaN);
            shortStopSerie = new FloatSerie(stockSerie.Count, "TRAILBARPERCENT.SS", float.NaN);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);

            bool upTrend = false;
            shortStopSerie[0] = highSerie[0];
            float previousExtremum = lowSerie[0];
            for (int i = 1; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] < longStopSerie[i - 1])
                    {
                        upTrend = false;
                        shortStopSerie[i] = previousExtremum;
                        previousExtremum = lowSerie[i];
                    }
                    else
                    {
                        longStopSerie[i] = longStopSerie[i - 1] * progress;
                        previousExtremum = Math.Max(previousExtremum, highSerie[i]);
                    }
                }
                else
                {
                    if (closeSerie[i] > shortStopSerie[i - 1])
                    {
                        upTrend = true;
                        longStopSerie[i] = previousExtremum;
                        previousExtremum = highSerie[i];
                    }
                    else
                    {
                        shortStopSerie[i] = shortStopSerie[i - 1] / progress;
                        previousExtremum = Math.Min(previousExtremum, lowSerie[i]);
                    }
                }
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
