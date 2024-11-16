using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILFW : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => true;
        public override string[] ParameterNames => new string[] { "Period", "NbATRStop" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 5f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500), new ParamRangeFloat(0f, 20f) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.Parameters[0];
            var nbAtr = (float)this.Parameters[1];
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, this.SerieNames[1], float.NaN);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie variationSerie = stockSerie.GetSerie(StockDataType.VARIATION);
            FloatSerie volumeSerie = stockSerie.GetSerie(StockDataType.VOLUME);
            FloatSerie highestSerie = stockSerie.GetIndicator($"HIGHEST({period})").Series[0];
            FloatSerie emaSerie = stockSerie.GetIndicator($"EMA({period})").Series[0];
            FloatSerie natrSerie = stockSerie.GetIndicator("NATR(14)").Series[0];
            FloatSerie oscSerie = stockSerie.GetIndicator("OSC(12,26,True,EMA)").Series[0];
            FloatSerie oscSignalSerie = oscSerie.CalculateEMA(9);

            bool holding = false;
            float trail = float.NaN;
            for (int i = period; i < stockSerie.Count; i++)
            {
                if (holding)
                {
                    if (closeSerie[i] < trail)
                    {
                        holding = false;
                        trail = float.NaN;
                    }
                    else
                    {

                        trail = emaSerie[i];
                        //if (oscSerie[i - 1] > oscSignalSerie[i - 1] && oscSerie[i] < oscSignalSerie[i])
                        //if (oscSerie[i - 2] > oscSerie[i - 1] && oscSerie[i - 1] < oscSerie[i])
                        //    trail = Math.Max(trail, Math.Min(lowSerie[i], lowSerie[i - 1]));
                    }
                }
                else
                {
                    if (closeSerie[i] < 2.0f)
                        continue;
                    if (highestSerie[i] < period)
                        continue;
                    if (closeSerie[i] * volumeSerie[i] < 1000000)
                        continue;
                    if (volumeSerie[i] < volumeSerie[i - 1] * 1.25f)
                        continue;
                    if (variationSerie[i] < 0.04f || variationSerie[i] > 0.2f)
                        continue;
                    if (closeSerie[i] < emaSerie[i])
                        continue;
                    if (natrSerie[i] > 15.0f)
                        continue;

                    trail = emaSerie[i];

                    holding = true;
                }
                longStopSerie[i] = trail;
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