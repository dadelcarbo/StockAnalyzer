using StockAnalyzer.StockMath;
using System;
using System.Drawing;

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
                eventNames ??= new string[] { "GapUp", "GapDown", "Large", "BullHammer", "BearHammer", "BullEngulfing", "BearEngulfing" };
                return eventNames;
            }
        }

        static readonly bool[] isEvent = new bool[] { true, true, true,
            true, true,
            true, true };
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
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red),
                        new Pen(Color.MediumPurple),
                        new Pen(Color.Green), new Pen(Color.Red),
                        new Pen(Color.Green), new Pen(Color.Red)
                    };
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

            FloatSerie variationSerie = stockSerie.GetSerie(StockDataType.VARIATION);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);

            FloatSerie atr1Serie = stockSerie.GetIndicator("ATR(1)").Series[0];
            FloatSerie atr20Serie = stockSerie.GetIndicator("ATR(20)").Series[0];

            for (int i = 1; i < stockSerie.Count; i++)
            {
                int eventIndex = 0;
                float high = highSerie[i];
                float low = lowSerie[i];
                // Check Gaps
                this.eventSeries[eventIndex++][i] = highSerie[i - 1] < low;
                this.eventSeries[eventIndex++][i] = lowSerie[i - 1] > high;
                this.eventSeries[eventIndex++][i] = atr1Serie[i] > 1.5 * atr20Serie[i];

                float close = closeSerie[i];
                float open = openSerie[i];
                float range = high - low;
                float upShade = (high - Math.Max(close, open)) / range;
                float downShade = (Math.Min(close, open) - low) / range;
                float body = (Math.Abs(close - open)) / range;

                // BullHammer
                this.eventSeries[eventIndex++][i] = body < 0.20f && downShade > 0.6f;
                // BearHammer
                this.eventSeries[eventIndex++][i] = body < 0.20f && upShade > 0.6f;


                var previousOpen = openSerie[i - 1];
                var previousClose = closeSerie[i - 1];

                // BullEngulfing
                this.eventSeries[eventIndex++][i] = previousClose < previousOpen && open < previousClose && close > previousOpen;

                // BearEngulfing
                this.eventSeries[eventIndex++][i] = previousClose > previousOpen && open > previousClose && close < previousOpen;
            }
        }
    }
}
