using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    class StockPaintBar_FLAG : StockPaintBarBase
    {
        public StockPaintBar_FLAG()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }

        public override bool HasTrendLine { get { return true; } }

        public override string[] ParameterNames
        {
            get { return new string[] { "Lookback", "Lag" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 200, 5 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 1000), new ParamRangeInt(1, 100) }; }
        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[] { "BrokenUp", "BrokenDown" };
                }
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true, true };
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
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red) };
                    foreach (Pen pen in seriePens)
                    {
                        pen.Width = 2;
                    }
                }
                return seriePens;
            }
        }

        class Flag
        {
            /// <summary>
            /// First High
            /// </summary>
            public PointF A { get; set; }
            /// <summary>
            /// Second High
            /// </summary>
            public PointF B { get; set; }
            /// <summary>
            /// First Low
            /// </summary>
            public PointF C { get; set; }
            /// <summary>
            /// Second Low
            /// </summary>
            public PointF D { get; set; }

            public HalfLine2D Resistance { get { return new HalfLine2D(A, B); } }

            public HalfLine2D Support { get { return new HalfLine2D(C, D); } }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            DrawingItem.CreatePersistent = false;
            try
            {
                // Detecting events
                this.CreateEventSeries(stockSerie.Count);

                if (stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
                {
                    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Clear();
                }
                else
                {
                    stockSerie.StockAnalysis.DrawingItems.Add(stockSerie.BarDuration, new StockDrawingItems());
                }
                StockDrawingItems drawingItems = stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration];

                int period = (int)this.parameters[0];
                int lag = (int)this.parameters[1];

                FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
                FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

                float firstHigh = float.MinValue, firstLow = float.MaxValue;
                float secondHigh = float.MinValue, secondLow = float.MaxValue;
                Flag flag = null;

                List<PointF> highs = new List<PointF>();
                List<PointF> lows = new List<PointF>();
                for (int i = period; i < stockSerie.Count; i++)
                {
                    if (highSerie.IsTopIsh(i))
                    {
                        highs.Add(new PointF(i, highSerie[i]));
                    }
                    if (lowSerie.IsBottomIsh(i))
                    {
                        lows.Add(new PointF(i, lowSerie[i]));
                    }
                }

                for (int i = period; i < stockSerie.Count; i++)
                {
                    PointF current = new PointF(i, closeSerie[i]);
                    if (flag == null) // Looking for a new flag
                    {
                        int index1 = i - period;
                        int index2 = i - period / 2;
                        int index3 = i - lag;

                        // Check first half high
                        firstHigh = highSerie.GetMax(index1, index2);
                        secondHigh = highSerie.GetMax(index2 + 1, index3);
                        firstLow = lowSerie.GetMin(index1, index2);
                        secondLow = lowSerie.GetMin(index2 + 1, index3);

                        if (firstHigh > secondHigh && firstLow < secondLow)
                        {
                            // This is a good candidate for a flag
                            flag = new Flag();
                            flag.A = new PointF(highSerie.FindMaxIndex(index1, index2), firstHigh);
                            flag.B = new PointF(highSerie.FindMaxIndex(index2 + 1, index3), secondHigh);

                            flag.C = new PointF(highSerie.FindMinIndex(index1, index2), firstLow);
                            flag.D = new PointF(highSerie.FindMinIndex(index2 + 1, index3), secondLow);

                            // Check if we are still in the flag
                            if (!flag.Resistance.IsAbovePoint(current) || flag.Support.IsAbovePoint(current))
                            {
                                flag = null;
                            }
                            //else
                            //{
                            //    drawingItems.Add(new Bullet2D(new PointF(index1, firstHigh), 3, Brushes.Crimson));
                            //    drawingItems.Add(new Bullet2D(new PointF(index1, firstLow), 3, Brushes.Crimson));
                            //    drawingItems.Add(new Bullet2D(new PointF(index2, firstHigh), 3, Brushes.Crimson));
                            //    drawingItems.Add(new Bullet2D(new PointF(index2, firstLow), 3, Brushes.Crimson));
                            //    drawingItems.Add(new Bullet2D(new PointF(index3, firstHigh), 3, Brushes.Crimson));
                            //    drawingItems.Add(new Bullet2D(new PointF(index3, firstLow), 3, Brushes.Crimson));
                            //    drawingItems.Add(new Bullet2D(new PointF(i, firstHigh), 3, Brushes.Crimson));
                            //    drawingItems.Add(new Bullet2D(new PointF(i, firstLow), 3, Brushes.Crimson));
                            //}
                        }
                    }
                    else // Continue current flag
                    {
                        // Check if we are still in the flag
                        if (!flag.Resistance.IsAbovePoint(current))
                        {
                            // flag has broken up
                            drawingItems.Add(flag.Resistance.Cut(i, true));
                            drawingItems.Add(flag.Support.Cut(i, true));

                            this.Events[0][i] = true;

                            flag = null;
                            i += period / 2;
                        }
                        else if (flag.Support.IsAbovePoint(current))
                        {
                            // flag has broken down
                            drawingItems.Add(flag.Resistance.Cut(i, true));
                            drawingItems.Add(flag.Support.Cut(i, true));

                            this.Events[1][i] = true;

                            flag = null;
                            i += period / 2;
                        }
                    }
                }
                if (flag != null)
                {
                    drawingItems.Add(flag.Resistance);
                    drawingItems.Add(flag.Support);
                }
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }
    }
}
