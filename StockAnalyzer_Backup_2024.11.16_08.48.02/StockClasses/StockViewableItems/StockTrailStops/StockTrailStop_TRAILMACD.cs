using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILMACD : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draw a trail stop that is initated when MACD histogram turns positive and trails at each next positive turn of the histogram.";

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;
        public override string[] ParameterNames => new string[] { "LongPeriod", "ShortPeriod", "SignalPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 26, 12, 9 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, SerieNames[0], float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, SerieNames[1], float.NaN);

            int longPeriod = (int)this.parameters[0];
            int shortPeriod = (int)this.parameters[1];
            int signalPeriod = (int)this.parameters[2];

            var histogram = stockSerie.GetIndicator($"EMACD({longPeriod},{shortPeriod},{signalPeriod})").Series[2];
            var highest = stockSerie.GetIndicator($"HIGHEST({longPeriod})").Series[0];
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var lowSerie = stockSerie.GetSerie(StockDataType.LOW);


            bool upTrend = false;
            int i = 1;
            int previousIndex = 0;
            float trailStop = 0;
            for (i = 1; histogram[i] > 0; i++) ;
            for (i = longPeriod; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (histogram[i - 1] > 0 && histogram[i] < 0)
                    {
                        previousIndex = i;
                    }
                    if (closeSerie[i] < trailStop)
                    {
                        upTrend = false;
                    }
                    else
                    {
                        if (histogram[i - 1] < 0 && histogram[i] > 0)
                        {
                            trailStop = lowSerie.GetMin(previousIndex, i - 1);
                        }
                        longStopSerie[i] = trailStop;
                    }
                }
                else
                {
                    if (histogram[i] > 0)
                    {
                        upTrend = true;
                        trailStop = lowSerie.GetMin(previousIndex, i - 1);
                        longStopSerie[i] = trailStop;
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
