using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILSTOKS : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draw a trail stop that starts and trails at each STOKS Bulling cross.";

        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }
        public override string[] ParameterNames
        {
            get { return new string[] { "StokPeriod", "SmoothPeriod", "SignalPeriod" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 35, 3, 3 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, SerieNames[0], float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, SerieNames[1], float.NaN);

            int longPeriod = (int)this.parameters[0];
            int shortPeriod = (int)this.parameters[1];
            int signalPeriod = (int)this.parameters[2];

            var stokTrend = stockSerie.GetIndicator($"STOKSTREND({longPeriod},{shortPeriod},{signalPeriod})");
            var highest = stockSerie.GetIndicator($"HIGHEST({longPeriod})").Series[0];
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            var positiveEvents = stokTrend.GetEvents("Bullish");
            var bullishCrossingEvents = stokTrend.GetEvents("BullishCrossing");
            var bearishCrossingEvents = stokTrend.GetEvents("BearishCrossing");
            float trailStop = float.NaN;

            bool upTrend = false;
            int previousTurnIndex = 0;
            for (int i = longPeriod; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] < trailStop)
                    {
                        upTrend = false;
                        previousTurnIndex = i;
                    }
                    else
                    {
                        if (bearishCrossingEvents[i])
                        {
                            previousTurnIndex = i;
                        }
                        else if (bullishCrossingEvents[i])
                        {
                            trailStop = Math.Max(trailStop, lowSerie.GetMin(previousTurnIndex, i));
                        }
                        longStopSerie[i] = trailStop;
                    }
                }
                else
                {
                    if (positiveEvents[i])
                    {
                        upTrend = true;
                        trailStop = lowSerie.GetMin(previousTurnIndex, i);
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
