using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILEMACD : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draw a trail stop that is calculated as a EMACD from a HIGHEST and trailing according to Financial Wisdom EMACD style.";

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

            var emacd = stockSerie.GetIndicator($"EMACD({longPeriod},{shortPeriod},{signalPeriod})");
            var highest = stockSerie.GetIndicator($"HIGHEST({longPeriod})").Series[0];
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            var positiveEvents = emacd.GetEvents("Positive");
            var firstBelowSignalEvents = emacd.GetEvents("FirstBelowSignal");
            float trailStop = float.NaN;

            bool upTrend = false;
            for (int i = longPeriod; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] < trailStop)
                    {
                        upTrend = false;
                    }
                    else
                    {
                        if (firstBelowSignalEvents[i])
                        {
                            trailStop = Math.Min(lowSerie[i], lowSerie[i - 1]);
                        }
                        longStopSerie[i] = trailStop;
                    }
                }
                else
                {
                    if (positiveEvents[i] && highest[i] > longPeriod)
                    {
                        upTrend = true;
                        trailStop = lowSerie.GetMin(i - shortPeriod, i);
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
