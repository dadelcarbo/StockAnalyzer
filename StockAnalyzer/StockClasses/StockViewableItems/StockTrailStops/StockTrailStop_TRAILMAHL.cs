using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILMAHL : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draw a trail stop that is calculated when a MA is making a new HL taking lowest a trail, and trailup at each new HL.";

        public override string[] ParameterNames => new string[] { "Period", "MA Type" };

        public override Object[] ParameterDefaultValues => new Object[] { 35, "EMA" };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeMA() };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, float.NaN);

            FloatSerie maSerie = stockSerie.GetIndicator($"{parameters[1]}({parameters[0]})").Series[0];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            bool upTrend = false;
            float previousMaBottom = maSerie[1];
            float previousLow = closeSerie.GetMin(0, 2);
            float longStop = float.NaN;
            for (int i = 2; i < stockSerie.Count; i++)
            {
                bool isMaBottom = maSerie.IsBottom(i - 1);
                bool isMaHigerLow = isMaBottom && maSerie[i - 1] > previousMaBottom;
                if (isMaBottom) { previousMaBottom = maSerie[i - 1]; }

                if (upTrend)
                {
                    if (isMaHigerLow) // Need to trail up
                    {
                        longStop = previousLow;
                    }
                    else
                    {
                        if (closeSerie[i] < longStop)
                        {
                            longStop = float.NaN;
                            upTrend = false;
                        }
                    }
                }
                else
                {
                    if (isMaHigerLow) // Need to start uptrend
                    {
                        longStop = previousLow;
                        upTrend = true;
                    }
                }
                longStopSerie[i] = longStop;
                previousLow = (maSerie[i - 1] > maSerie[i]) ? Math.Min(previousLow, closeSerie[i]) : float.MaxValue;
            }



            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }
    }
}
