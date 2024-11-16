using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILHIGHESTATR : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Start a trail when a new high is made. It's a long only trail suitable for trend following. Initial stop is set at specific ATR, then trail can have larger ATR";

        public override string[] ParameterNames => new string[] { "Trigger", "StopATR", "TrailATR" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 2f, 6f };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500), new ParamRangeFloat(0f, 50f), new ParamRangeFloat(0f, 50f) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, "TRAILHIGHESTATR.LS", float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, "TRAILHIGHESTATR.SS", float.NaN);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            int trigger = (int)this.Parameters[0];
            var stopATR = (float)this.Parameters[1];
            var trailATR = (float)this.Parameters[2];

            this.CreateEventSeries(stockSerie.Count);

            FloatSerie highestSerie = stockSerie.GetIndicator($"HIGHEST({trigger})").Series[0];
            FloatSerie atrSerie = stockSerie.GetIndicator($"ATR(20)").Series[0];
            bool upTrend = false;
            float trailStop = float.NaN;
            float hardStop = float.NaN;
            for (int i = trigger; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] < longStopSerie[i - 1]) // Broken Down
                    {
                        upTrend = false;
                    }
                    else
                    {
                        if (closeSerie[i] > closeSerie[i - 1])
                            trailStop = Math.Max(trailStop, closeSerie[i] - trailATR * atrSerie[i]);
                        longStopSerie[i] = Math.Max(trailStop, hardStop);
                    }
                }
                else
                {
                    if (highestSerie[i] > trigger) // Broken Up
                    {
                        upTrend = true;
                        hardStop = lowSerie[i] - stopATR * atrSerie[i];
                        trailStop = closeSerie[i] - trailATR * atrSerie[i];
                        longStopSerie[i] = hardStop;
                    }
                }
            }

            this.Series[0] = longStopSerie;
            this.Series[0].Name = this.SerieNames[0];
            this.Series[1] = shortStopSerie;
            this.Series[1].Name = this.SerieNames[1];

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }
    }
}