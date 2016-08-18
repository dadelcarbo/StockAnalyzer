using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_BARPATTERN : StockPaintBarBase
    {
        public StockPaintBar_BARPATTERN()
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
                    eventNames = new string[] { "GapUp", "GapDown", "Large" };
                }
                return eventNames;
            }
        }

        static readonly bool[] isEvent = new bool[] { true, true, true };
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
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.MediumPurple) };
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

            FloatSerie atr1Serie = stockSerie.GetIndicator("ATR(1)").Series[0];
            FloatSerie atr20Serie = stockSerie.GetIndicator("ATR(20)").Series[0];

            for (int i = 1; i < stockSerie.Count; i++)
            {
                // Check Gaps
                this.eventSeries[0][i] = highSerie[i - 1] < lowSerie[i];
                this.eventSeries[1][i] = lowSerie[i - 1] > highSerie[i];
                this.eventSeries[2][i] = atr1Serie[i] > 1.5*atr20Serie[i];
            }
        }
    }
}
