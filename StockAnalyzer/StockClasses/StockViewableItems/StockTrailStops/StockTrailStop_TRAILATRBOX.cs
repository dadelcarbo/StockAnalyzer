using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILATRBOX : StockTrailStopBase
    {
        public override string Definition => "Draws Trail Stop based ATR Boxes";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string[] ParameterNames
        {
            get { return new string[] { "ATRPeriod", "NbStartDev", "NbTrailDev" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 10, 2f, 2f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get
            {
                return new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(0.0f, 20.0f),
                new ParamRangeFloat(0.0f, 20.0f),
                };
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStop = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);
            FloatSerie shortStop = new FloatSerie(stockSerie.Count, this.SerieNames[1], float.NaN);

            var atrPeriod = (int)this.parameters[0];
            var startDev = (float)parameters[1];
            var trailDev = (float)parameters[2];

            var atrSerie = stockSerie.GetIndicator("ATR(" + atrPeriod + ")").Series[0];

            bool upTrend = false;
            var previousLow = stockSerie.Values.First().LOW;

            int i = 1;
            foreach (var currentBar in stockSerie.Values.Skip(1))
            {
                if (upTrend)
                {
                    if (currentBar.CLOSE < longStop[i - 1])
                    { // Trailing stop has been broken => reverse trend
                        upTrend = false;
                        previousLow = currentBar.LOW;
                    }
                    else
                    {
                        // UpTrend still in place
                        var nbBox = (int)Math.Floor((currentBar.CLOSE - longStop[i - 1]) / (atrSerie[i - 1] * trailDev)) - 1;
                        if (nbBox >= 1)
                            longStop[i] = longStop[i - 1] + atrSerie[i - 1] * trailDev;
                        else
                            longStop[i] = longStop[i - 1];
                    }
                }
                else
                {
                    if (currentBar.CLOSE > previousLow + atrSerie[i - 1] * startDev)
                    {  // Up trend staring
                        upTrend = true;
                        longStop[i] = currentBar.CLOSE - atrSerie[i - 1] * trailDev;
                    }
                    else
                    {
                        // Up trend not started yet
                        previousLow = Math.Min(previousLow, currentBar.LOW);
                    }
                }
                i++;
            }

            this.Series[0] = longStop;
            this.Series[1] = shortStop;

            // Generate events
            this.GenerateEvents(stockSerie, longStop, shortStop);
        }

    }
}
