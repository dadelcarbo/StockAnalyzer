using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILEMACD : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draw a trail stop that is calculated as a EMACD from a HIGHEST and trailing according to Finacial Wisdom EMACD style.";

        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }
        public override string[] ParameterNames
        {
            get { return new string[] { "LongPeriod", "ShortPeriod", "SignalPeriod" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 26, 12, 9 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }
        public override string[] SerieNames { get { return new string[] { "TRAILEMACD.LS", "TRAILEMACD.SS" }; } }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, SerieNames[0], float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, SerieNames[0], float.NaN);

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
