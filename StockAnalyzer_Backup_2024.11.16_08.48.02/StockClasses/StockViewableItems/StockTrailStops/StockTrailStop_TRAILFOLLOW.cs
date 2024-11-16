using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILFOLLOW : StockTrailStopBase
    {
        public override string Definition => "Draws Trail Stop following trend on up bar with an efficiency ratio, staying flat on down bars";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;
        public override string[] ParameterNames => new string[] { "Ratio" };

        public override Object[] ParameterDefaultValues => new Object[] { 0.5f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeFloat(0.0f, 1f) };
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;
            float ratio = (float)this.Parameters[0];

            longStopSerie = new FloatSerie(stockSerie.Count, "TRAILFOLLOW.LS", float.NaN);
            shortStopSerie = new FloatSerie(stockSerie.Count, "TRAILFOLLOW.SS", float.NaN);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie varSerie = stockSerie.GetSerie(StockDataType.VARIATION);
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
                        float var = varSerie[i];
                        if (var > 0)
                        {
                            longStopSerie[i] = longStopSerie[i - 1] * (1f + var * ratio);
                        }
                        else
                        {
                            longStopSerie[i] = longStopSerie[i - 1];
                        }
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
                        float var = varSerie[i];
                        if (var < 0)
                        {

                            shortStopSerie[i] = shortStopSerie[i - 1] / (1f - var * ratio);
                        }
                        else
                        {
                            shortStopSerie[i] = shortStopSerie[i - 1];
                        }
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
