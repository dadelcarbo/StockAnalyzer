using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILLARGEBAR : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Trail stop stating when a large up bar is detected. It trails as TOPEMA";

        public override string[] ParameterNames => new string[] { "BarSize", "TrailPeriod" };
        public override Object[] ParameterDefaultValues => new Object[] { 0.05f, 35 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeFloat(0f, 100f), new ParamRangeInt(0, 500) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, this.SerieNames[1], float.NaN);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            float barSize = (float)this.parameters[0];
            int period = (int)this.parameters[1];
            float alpha = 2.0f / (float)(period + 1);
            float trail = float.NaN;

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            var varSerie = stockSerie.GetSerie(StockDataType.VARIATION);

            bool upTrend = false;
            for (int i = period; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] < trail)
                    {
                        upTrend = false;
                        trail = closeSerie[i] * barSize;
                    }
                    else
                    {
                        trail = trail + alpha * (closeSerie[i] - trail);
                        longStopSerie[i] = trail;
                    }
                }
                else
                {
                    if (varSerie[i] > barSize)
                    {
                        upTrend = true;
                        trail = lowSerie[i];
                        longStopSerie[i] = trail;
                    }
                }
            }

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }
    }
}
