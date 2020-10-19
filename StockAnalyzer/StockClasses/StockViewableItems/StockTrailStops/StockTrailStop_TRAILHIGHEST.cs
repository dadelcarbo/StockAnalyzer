using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILHIGHEST : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Trails when making a new high to the lowest in period (and vice versa in down trend)";

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 2 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500) };

        public override string[] SerieNames => new string[] { "TRAILHIGHEST.LS", "TRAILHIGHEST.SS" };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);

            int period = (int)this.Parameters[0];

            float lowest = lowSerie.GetMin(0, period - 1);
            float highest = highSerie.GetMax(0, period - 1);
            for (int i = 0; i < period; i++)
            {
                longStopSerie[i] = lowest;
                shortStopSerie[i] = float.NaN;
            }
            bool upTrend = true;

            for (int i = period; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] < longStopSerie[i - 1]) // Broken Down
                    {
                        upTrend = false;
                        shortStopSerie[i] = highSerie.GetMax(i - period, i);
                        longStopSerie[i] = float.NaN;
                        lowest = lowSerie[i];
                    }
                    else
                    {
                        if (highSerie[i] > highest) // TrailUp
                        {
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], lowSerie.GetMin(i - period, i));
                            highest = highSerie[i];
                        }
                        else
                        {
                            longStopSerie[i] = longStopSerie[i - 1];
                        }
                        shortStopSerie[i] = float.NaN;
                    }
                }
                else
                {
                    if (closeSerie[i] > shortStopSerie[i - 1]) // Broken Up
                    {
                        upTrend = true;
                        longStopSerie[i] = lowSerie.GetMin(i - period, i);
                        shortStopSerie[i] = float.NaN;
                        highest = highSerie[i];
                    }
                    else
                    {
                        if (lowSerie[i] < lowest) // TrailDown
                        {
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], highSerie.GetMax(i - period, i));
                            lowest = lowSerie[i];
                        }
                        else
                        {
                            shortStopSerie[i] = shortStopSerie[i - 1];
                        }
                        longStopSerie[i] = float.NaN;
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