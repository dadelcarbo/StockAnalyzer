using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_CANDLEPATTERN : StockPaintBarBase
    {
        public StockPaintBar_CANDLEPATTERN()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }

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
            get { return new ParamRange[] { }; }
        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[] { "ThreeLineStrike" };
                }
                return eventNames;
            }
        }

        static readonly bool[] isEvent = new bool[] { true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green) };
                    foreach (Pen pen in seriePens)
                    {
                        pen.Width = 2;
                    }
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            float accuracy = 0.1f; // 10%
            for (int i = 4; i < stockSerie.Count; i++)
            {
                // Thee line strike
                if (closeSerie[i - 3] < openSerie[i - 3] && closeSerie[i - 2] < openSerie[i - 2] && closeSerie[i - 1] < openSerie[i - 1] && lowSerie[i] < closeSerie[i - 1] && closeSerie[i] > openSerie[i - 3])
                {
                    this.eventSeries[0][i] = true;
                }
            }
        }
    }
}
