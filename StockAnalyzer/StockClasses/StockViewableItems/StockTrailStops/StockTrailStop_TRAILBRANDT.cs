using System;
using System.Linq;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILBRANDT : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 2 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, "TRAILBRANDT.LS");
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, "TRAILBRANDT.SS");

            int period = (int)this.parameters[0];

            if (stockSerie.ValueArray.Length < period)
            {
                // Generate events
                this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
                return;
            }


            StockDailyValue firstBar = stockSerie.Values.First();
            bool upTrend = firstBar.CLOSE > firstBar.OPEN;
            int i = 1;
            if (upTrend)
            {
                longStopSerie[0] = firstBar.LOW;
                shortStopSerie[0] = float.NaN;
            }
            else
            {
                longStopSerie[0] = float.NaN;
                shortStopSerie[0] = firstBar.HIGH;
            }
            StockDailyValue extremumBar = firstBar;
            StockDailyValue setupBar = null;
            foreach (var currentBar in stockSerie.Values.Skip(1))
            {
                if (upTrend)
                {
                    if (setupBar == null)
                    {
                        if (currentBar.CLOSE < extremumBar.LOW)
                        {
                            setupBar = currentBar;
                            longStopSerie[i] = setupBar.LOW;
                        }
                        else
                        {
                            if (currentBar.HIGH > extremumBar.HIGH)
                            {
                                extremumBar = currentBar;
                            }
                            longStopSerie[i] = longStopSerie[i - 1];
                        }
                        shortStopSerie[i] = float.NaN;
                    }
                    else
                    {
                        if (currentBar.CLOSE < longStopSerie[i - 1])
                        {
                            // Trailing stop has been broken => reverse trend
                            upTrend = false;
                            longStopSerie[i] = float.NaN;
                            shortStopSerie[i] = currentBar.HIGH;
                            setupBar = null;
                            extremumBar = currentBar;
                        }
                        else
                        {
                            if (currentBar.HIGH > extremumBar.HIGH)
                            {
                                extremumBar = currentBar;
                                setupBar = null;
                            }
                            longStopSerie[i] = longStopSerie[i - 1];
                            shortStopSerie[i] = float.NaN;
                        }
                    }
                }
                else
                {
                    if (setupBar == null)
                    {
                        if (currentBar.CLOSE > extremumBar.HIGH)
                        {
                            setupBar = currentBar;
                            shortStopSerie[i] = setupBar.HIGH;
                        }
                        else
                        {
                            if (currentBar.LOW < extremumBar.LOW)
                            {
                                extremumBar = currentBar;
                            }
                            shortStopSerie[i] = shortStopSerie[i - 1];
                        }
                        longStopSerie[i] = float.NaN;
                    }
                    else
                    {
                        if (currentBar.CLOSE > shortStopSerie[i - 1])
                        {
                            // Trailing stop has been broken => reverse trend
                            upTrend = true;
                            shortStopSerie[i] = float.NaN;
                            longStopSerie[i] = currentBar.LOW;
                            setupBar = null;
                            extremumBar = currentBar;
                        }
                        else
                        {
                            if (currentBar.LOW < extremumBar.LOW)
                            {
                                extremumBar = currentBar;
                                setupBar = null;
                            }
                            shortStopSerie[i] = shortStopSerie[i - 1];
                            longStopSerie[i] = float.NaN;
                        }
                    }
                }
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