using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    class StockPaintBar_RANGE : StockPaintBarBase
    {
        public StockPaintBar_RANGE()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override bool HasTrendLine => true;

        public override string[] ParameterNames => new string[] { "Period", "Max Width %" };
        public override Object[] ParameterDefaultValues => new Object[] { 200, 20 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 1000), new ParamRangeInt(1, 100) };

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                eventNames ??= new string[] { "RANGE", "ReversedRANGE" };
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true, true };
        public override bool[] IsEvent => isEvent;

        public override Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red) };
                    foreach (Pen pen in seriePens)
                    {
                        pen.Width = 2;
                    }
                }
                return seriePens;
            }
        }

        class Range
        {
            public float high;
            public float low;
            public float startIndex;
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            DrawingItem.CreatePersistent = false;
            try
            {
                // Detecting events
                this.CreateEventSeries(stockSerie.Count);

                if (!stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
                {
                    stockSerie.StockAnalysis.DrawingItems.Add(stockSerie.BarDuration, new StockDrawingItems());
                }
                else
                {
                    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].RemoveAll(di => !di.IsPersistent);
                }
                StockDrawingItems drawingItems = stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration];

                int period = (int)this.parameters[0];
                float maxWidth = (int)this.parameters[1] * 0.01f;

                FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
                FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

                float high = float.MinValue, low = float.MaxValue, rangePercent;
                Range range = null;
                for (int i = period; i < stockSerie.Count; i++)
                {
                    high = highSerie.GetMax(i - period, i);
                    low = lowSerie.GetMin(i - period, i);

                    rangePercent = (high - low) / high;
                    if (range == null)
                    {
                        if (rangePercent < maxWidth)
                        {
                            // Start a new range
                            range = new Range() { low = low, high = high, startIndex = i - period };
                        }
                    }
                    else
                    {
                        if (closeSerie[i] > range.high)
                        {
                            // Broken up
                            drawingItems.Add(new Rectangle2D(new PointF(range.startIndex, range.high), new PointF(i, range.low)));
                            range = null;
                            i += period;
                        }
                        else if (closeSerie[i] < range.low)
                        {
                            // Broken down
                            drawingItems.Add(new Rectangle2D(new PointF(range.startIndex, range.high), new PointF(i, range.low)));
                            range = null;
                            i += period / 2;
                        }
                        else
                        {
                            // Adjust current range
                            range.low = Math.Max(range.low, low);
                            range.high = Math.Min(range.high, high);
                        }
                    }
                }
                if (range != null)
                {
                    drawingItems.Add(new Rectangle2D(new PointF(range.startIndex, range.high), new PointF(stockSerie.Count, range.low)) { Pen = Pens.Gainsboro });
                }
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }
    }
}
