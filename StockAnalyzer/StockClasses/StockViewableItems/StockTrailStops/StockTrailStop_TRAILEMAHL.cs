using System;
using System.Linq;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILEMAHL : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draw a trail stop that is calculated From following top/bottom on EMA cross over.";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 2 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, "TRAILEMAHL.LS", float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, "TRAILEMAHL.SS", float.NaN);

            int period = (int)this.parameters[0];

            if (stockSerie.ValueArray.Length < period)
            {
                // Generate events
                this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
                return;
            }

            var highSerie = new FloatSerie(stockSerie.Values.Select(v => v.HIGH).ToArray());
            var lowSerie = new FloatSerie(stockSerie.Values.Select(v => v.LOW).ToArray());

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            FloatSerie emaSerie = stockSerie.GetIndicator($"EMA({period})").Series[0];

            StockDailyValue previousValue = stockSerie.Values.First();
            bool upTrend = previousValue.CLOSE > stockSerie.ValueArray[1].CLOSE;
            int i = 1;
            float emaHigh = float.MinValue, emaLow = float.MaxValue;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
                emaLow = previousValue.LOW;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
                emaHigh = previousValue.HIGH;
            }
            float previousHigh = float.MinValue, previousLow = float.MaxValue;
            foreach (StockDailyValue currentValue in stockSerie.Values.Skip(1))
            {
                // Calculate High/low when price above/below EMA
                if (closeSerie[i] > emaSerie[i])
                {
                    if (closeSerie[i - 1] > emaSerie[i - 1])
                    {
                        previousHigh = Math.Max(previousHigh, highSerie[i]);
                    }
                    else // EMA cross over Upwards
                    {
                        previousHigh = highSerie[i];
                        emaLow = previousLow;
                    }
                }
                else
                {
                    if (closeSerie[i - 1] < emaSerie[i - 1])
                    {
                        previousLow = Math.Min(previousLow, lowSerie[i]);
                    }
                    else // EMA cross over Downwards
                    {
                        previousLow = lowSerie[i];
                        emaHigh = previousHigh;
                    }
                }
                if (upTrend)
                {
                    if (currentValue.CLOSE < longStopSerie[i - 1])
                    { // Trailing stop has been broken => reverse trend
                        upTrend = false;
                        shortStopSerie[i] = emaHigh;
                    }
                    else
                    {
                        // Trail the stop  
                        longStopSerie[i] = Math.Max(longStopSerie[i - 1], emaLow);
                    }
                }
                else
                {
                    if (currentValue.CLOSE > shortStopSerie[i - 1])
                    {  // Trailing stop has been broken => reverse trend
                        upTrend = true;
                        longStopSerie[i] = emaLow;
                    }
                    else
                    {
                        // Trail the stop  
                        shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], emaHigh);
                    }
                }

                previousValue = currentValue;
                i++;
            }

            this.Series[0] = longStopSerie;
            this.Series[0].Name = longStopSerie.Name;
            this.Series[1] = shortStopSerie;
            this.Series[1].Name = shortStopSerie.Name;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }

    }
}