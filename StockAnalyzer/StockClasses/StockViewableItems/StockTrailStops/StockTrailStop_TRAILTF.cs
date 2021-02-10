using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILTF : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Start a trail when a new high is made and trail it when another new high is made. It's a long only trail suitable for trend following";

        public override string[] ParameterNames => new string[] { "Trigger" };

        public override Object[] ParameterDefaultValues => new Object[] { 20 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500) };

        public override string[] SerieNames => new string[] { "TRAILTF.LS", "TRAILTF.SS" };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, "TRAILTF.LS", float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, "TRAILTF.SS", float.NaN);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            int trigger = (int)this.Parameters[0];

            this.CreateEventSeries(stockSerie.Count);

            FloatSerie highestSerie = stockSerie.GetIndicator($"HIGHEST({trigger})").Series[0];
            bool upTrend = false;
            float trailStop = float.NaN;
            int previousHighIndex = 0;
            for (int i = trigger; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] < trailStop) // Broken Down
                    {
                        upTrend = false;
                        this.Events[1][i] = true;
                    }
                    else
                    {
                        this.Events[6][i] = true;
                        if (highestSerie[i - 1] > trigger && highestSerie[i] == 0)
                        {
                            trailStop = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(previousHighIndex, i));
                            previousHighIndex = i - 1;
                        }
                        longStopSerie[i] = trailStop;
                    }
                }
                else
                {
                    if (highestSerie[i] > trigger) // Broken Up
                    {
                        upTrend = true;
                        longStopSerie[i] = trailStop = lowSerie.GetMin(i - trigger, i);
                        previousHighIndex = i - trigger;
                        this.Events[0][i] = true;
                        this.Events[6][i] = true;
                    }
                }
            }

            this.Series[0] = longStopSerie;
            this.Series[0].Name = this.SerieNames[0];
            this.Series[1] = shortStopSerie;
            this.Series[1].Name = this.SerieNames[1];
        }

        private static string[] eventNames = new string[]
          {
             "BrokenUp", "BrokenDown",           // 0,1
             "Pullback", "EndOfTrend",           // 2,3
             "HigherLow", "LowerHigh",           // 4,5
             "Bullish", "Bearish",               // 6,7
             "LH_HL", "HL_LH"                    // 8,9
          };
    }
}