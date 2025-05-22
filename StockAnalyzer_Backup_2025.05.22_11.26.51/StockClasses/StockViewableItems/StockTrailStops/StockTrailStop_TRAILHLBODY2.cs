using StockAnalyzer.StockMath;
using System;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILHLBODY2 : StockTrailStopBase
    {
        public override string[] ParameterNames => new string[] { "TrailUpPeriod", "TrailDownPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 20 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500), new ParamRangeInt(0, 500) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, SerieNames[0]);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, SerieNames[1]);

            int trailUpPeriod = (int)this.parameters[0];
            int trailDownPeriod = (int)this.parameters[1];

            if (stockSerie.ValueArray.Length < Math.Max(trailUpPeriod, trailDownPeriod))
            {
                // Generate events
                this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
                return;
            }

            var bodyHighSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);
            var bodyLowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            StockDailyValue previousValue = stockSerie.Values.First();
            bool upTrend = previousValue.CLOSE > stockSerie.ValueArray[1].CLOSE;
            int i = 1;
            if (upTrend)
            {
                longStopSerie[0] = previousValue.LOW;
                shortStopSerie[0] = float.NaN;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = previousValue.HIGH;
            }
            foreach (StockDailyValue currentValue in stockSerie.Values.Skip(1))
            {
                if (i > Math.Max(trailUpPeriod, trailDownPeriod))
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = bodyHighSerie.GetMax(i - trailDownPeriod - 1, i);
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], bodyLowSerie.GetMin(i - trailUpPeriod, i));
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = bodyLowSerie.GetMin(i - trailUpPeriod - 1, i);
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], bodyHighSerie.GetMax(i - trailDownPeriod, i));
                        }
                    }
                }
                else
                {
                    if (upTrend)
                    {
                        if (currentValue.CLOSE < longStopSerie[i - 1])
                        { // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = bodyHighSerie.GetMax(0, i);
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = Math.Max(longStopSerie[i - 1], bodyLowSerie.GetMin(0, i));
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                    else
                    {
                        if (currentValue.CLOSE > shortStopSerie[i - 1])
                        {  // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            longStopSerie[i] = bodyLowSerie.GetMin(0, i);
                            shortStopSerie[i] = float.NaN;
                        }
                        else
                        {
                            // Trail the stop  
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], bodyHighSerie.GetMax(0, i));
                        }
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