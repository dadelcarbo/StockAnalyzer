using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_MTFMA : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "FastBarCount", "SlowBarCount" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 3, 9 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames
        {
            get
            {
                return new string[]
                  {
                      "Signal_MA3",
                      "Fast_MA3",
                      "Slow_MA3",
                  };
            }
        }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Green, 1), new Pen(Color.Black, 1), new Pen(Color.Red, 1) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int fastBarCount = (int)this.parameters[0];
            int slowBarCount = (int)this.parameters[1];

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var fastSerie = new FloatSerie(stockSerie.Count);
            var slowSerie = new FloatSerie(stockSerie.Count);
            fastSerie[0] = fastSerie[1] = fastSerie[2] = closeSerie[2];
            slowSerie[0] = slowSerie[1] = slowSerie[2] = closeSerie[2];

            float fast0;
            float fast1 = closeSerie[2];
            float fast2 = closeSerie[2];
            float slow0;
            float slow1 = closeSerie[2];
            float slow2 = closeSerie[2];
            var previousValue = stockSerie.Values.ElementAt(2);

            int i = 3;
            int fastCount = 3;
            int slowCount = 3;
            foreach (var dailyValue in stockSerie.Values.Skip(3))
            {
                if (fastCount >= fastBarCount)
                { // New week starting
                    fast0 = fast1;
                    fast1 = fast2;
                    fast2 = previousValue.CLOSE;
                    fastSerie[i] = (fast0 + fast1 + fast2) / 3;
                    fastCount = 0;
                }
                else
                {
                    fastSerie[i] = fastSerie[i - 1];
                    fastCount++;
                }
                if (slowCount >= slowBarCount)
                { // New month starting
                    slow0 = slow1;
                    slow1 = slow2;
                    slow2 = previousValue.CLOSE;
                    slowSerie[i] = (slow0 + slow1 + slow2) / 3;
                    slowCount = 0;
                }
                else
                {
                    slowSerie[i] = slowSerie[i - 1];
                    slowCount++;
                }

                i++;
                previousValue = dailyValue;
            }

            this.Series[0] = closeSerie.CalculateMA(3);
            this.Series[1] = fastSerie;
            this.Series[2] = slowSerie;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
        }
        static string[] eventNames = new string[] { };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
