using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILHIGHLOW : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 2 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500) };

        public override string[] SerieNames => new string[] { "TRAILHIGHLOW.LS", "TRAILHIGHLOW.SS" };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            int period = (int)this.Parameters[0];

            var trailEMA = stockSerie.GetTrailStop($"TRAILEMA({period.ToString()})");

            //"BrokenUp", "BrokenDown",           // 0,1
            //"Pullback", "EndOfTrend",           // 2,3
            //"HigherLow", "LowerHigh",           // 4,5
            //"Bullish", "Bearish"                // 6,7
            var higherLow = trailEMA.Events[4];
            var lowerHigh = trailEMA.Events[5];
            var bullish = trailEMA.Events[6];

            bool upTrend = bullish[period - 1];
            for (int i = 0; i < period; i++)
            {
                longStopSerie[i] = trailEMA.Series[0][i];
                shortStopSerie[i] = trailEMA.Series[1][i];
            }
            for (int i = period; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] < longStopSerie[i - 1]) // Broken Down
                    {
                        upTrend = false;
                        shortStopSerie[i] = trailEMA.Series[1][i];
                        longStopSerie[i] = float.NaN;
                    }
                    else
                    {
                        if (higherLow[i]) // TrailUp
                        {
                            longStopSerie[i] = trailEMA.Series[0][i];
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
                        longStopSerie[i] = trailEMA.Series[0][i];
                        shortStopSerie[i] = float.NaN;
                    }
                    else
                    {
                        if (lowerHigh[i]) // TrailDown
                        {
                            shortStopSerie[i] = trailEMA.Series[1][i];
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