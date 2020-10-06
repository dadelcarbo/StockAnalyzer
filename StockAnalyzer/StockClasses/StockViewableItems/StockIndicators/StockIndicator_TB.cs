using System;
using System.Collections.Generic;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TB : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string Definition
        {
            get { return "Draw a top/bottom channel"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get
            {
                return new ParamRange[]
                {
                };
            }
        }

        public override string[] SerieNames { get { return new string[] { "Top", "Bottom" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Blue), new Pen(Color.Blue) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            var topSerie = new FloatSerie(stockSerie.Count);
            var botSerie = new FloatSerie(stockSerie.Count);

            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            var previousTop = Math.Max(highSerie[0], highSerie[1]);
            topSerie[0] = previousTop;
            var previousBot = Math.Min(lowSerie[0], lowSerie[1]);
            botSerie[0] = previousBot;
            for (int i = 2; i < stockSerie.Count; i++)
            {
                // Calculate top serie
                var high = highSerie[i];
                var previousHigh = highSerie[i - 1];
                if (high > previousTop)
                {
                    topSerie[i] = previousTop = high;
                }
                else if (highSerie.IsTop(i-1))
                {
                    topSerie[i] = previousTop = previousHigh;
                }
                else
                {
                    topSerie[i] = previousTop;
                }
                // Calculate bottom serie
                var low = lowSerie[i];
                var previousLow = lowSerie[i - 1];
                if (low < previousBot)
                {
                    botSerie[i] = previousBot = low;
                }
                else if (lowSerie.IsBottom(i - 1))
                {
                    botSerie[i] = previousBot = previousLow;
                }
                else
                {
                    botSerie[i] = previousBot;
                }
            }

            this.series[0] = topSerie;
            this.Series[0].Name = this.SerieNames[0];

            this.series[1] = botSerie;
            this.Series[1].Name = this.SerieNames[1];

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
