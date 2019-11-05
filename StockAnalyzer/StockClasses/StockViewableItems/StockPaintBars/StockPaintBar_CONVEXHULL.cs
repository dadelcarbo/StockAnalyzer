using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_CONVEXCULL : StockPaintBarBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override bool HasTrendLine => true;

        public override string[] ParameterNames => new string[] { "LongPeriod", "ShortPeriod", "Convex" };
        public override Object[] ParameterDefaultValues => new Object[] { 200, 10, true };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 1500), new ParamRangeInt(1, 500), new ParamRangeBool() };

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[] { "AboveResistance", "BelowSupport" };
                }
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true, true };
        public override bool[] IsEvent => isEvent;

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

        public override void ApplyTo(StockSerie stockSerie)
        {
            DrawingItem.CreatePersistent = false;
            try
            {
                // Detecting events
                this.CreateEventSeries(stockSerie.Count);

                int longPeriod = (int)this.parameters[0];
                int shortPeriod = (int)this.parameters[1];
                bool convex = (bool)this.parameters[2];

                int startIndex = Math.Max(0, stockSerie.Count - longPeriod);
                int endIndex = stockSerie.Count - shortPeriod;

                if (stockSerie.Count < longPeriod) return;
                FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                HalfLine2D resistance;
                HalfLine2D support;

                GetConvexCull(convex, stockSerie, startIndex, endIndex, out resistance, out support, this.SeriePens[0], this.SeriePens[1]);

                PointF lastPoint = new PointF(stockSerie.LastCompleteIndex, closeSerie[stockSerie.LastCompleteIndex]);
                this.eventSeries[0][stockSerie.LastCompleteIndex] = !resistance.IsAbovePoint(lastPoint);
                this.eventSeries[1][stockSerie.LastCompleteIndex] = support.IsAbovePoint(lastPoint);
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }
        static public void GetSR(StockSerie stockSerie, int startIndex, int endIndex)
        {
            if (stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
            {
                stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Clear();
            }
            else
            {
                stockSerie.StockAnalysis.DrawingItems.Add(stockSerie.BarDuration, new StockDrawingItems());
            }
            StockDrawingItems drawingItems = stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration];

            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            float highest = highSerie.GetMax(startIndex, endIndex-5);
            float lowest = lowSerie.GetMin(startIndex, endIndex-5);

            PointF A, B, C, D;
            A = new PointF(startIndex, highest);
            B = new PointF(endIndex, highest);
            C = new PointF(startIndex, lowest);
            D = new PointF(endIndex, lowest);

            drawingItems.Add(new Segment2D(A, B, Pens.DarkCyan));
            drawingItems.Add(new Segment2D(C, D, Pens.DarkCyan));
        }

        static public void GetConvexCull(bool convex, StockSerie stockSerie, int startIndex, int endIndex, out HalfLine2D resistance, out HalfLine2D support,Pen resistancePen, Pen supportPen)
        {
            if (stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
            {
                stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].RemoveAll(d=>!d.IsPersistent);
            }
            else
            {
                stockSerie.StockAnalysis.DrawingItems.Add(stockSerie.BarDuration, new StockDrawingItems());
            }
            StockDrawingItems drawingItems = stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration];

            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            //float highest = highSerie.GetMax(startIndex, endIndex);
            //float lowest = lowSerie.GetMin(startIndex, endIndex);

            //PointF A, B, C, D;
            //A = new PointF(startIndex, lowest);
            //B = new PointF(startIndex, highest);
            //C = new PointF(endIndex, highest);
            //D = new PointF(endIndex, lowest);

            //drawingItems.Add(new Segment2D(A, B, Pens.DarkCyan));
            //drawingItems.Add(new Segment2D(B, C, Pens.DarkCyan));
            //drawingItems.Add(new Segment2D(C, D, Pens.DarkCyan));
            //drawingItems.Add(new Segment2D(D, A, Pens.DarkCyan));

            List<PointF> highPoints = new List<PointF>();
            List<PointF> lowPoints = new List<PointF>();
            List<PointF> points = new List<PointF>();

            for (int i = startIndex; i < endIndex; i++)
            {
                if (highSerie.IsTop(i))
                    highPoints.Add(new PointF(i, highSerie[i]));
                if (lowSerie.IsBottom(i))
                    lowPoints.Add(new PointF(i, lowSerie[i]));
            }
            lowPoints.Reverse();

            List<PointF> highCull = GetConvexCull(highPoints, convex);

            for (int i = 1; i < highCull.Count - 1; i++)
            {
                drawingItems.Add(new Segment2D(highCull[i - 1], highCull[i], resistancePen));
            }

            resistance = new HalfLine2D(highCull[highCull.Count - 2], highCull[highCull.Count - 1], resistancePen);
            drawingItems.Add(resistance);
            for (int i = 0; i < highCull.Count; i++)
            {
                drawingItems.Add(new Bullet2D(highCull[i], 2));
            }

            List<PointF> lowCull = GetConvexCull(lowPoints, convex);
            lowCull.Reverse();
            for (int i = 1; i < lowCull.Count-1; i++)
            {
                drawingItems.Add(new Segment2D(lowCull[i - 1], lowCull[i], supportPen));
            }
            support = new HalfLine2D(lowCull[lowCull.Count - 2], lowCull[lowCull.Count - 1], supportPen);
            drawingItems.Add(support);
            for (int i = 0; i < lowCull.Count; i++)
            {
                drawingItems.Add(new Bullet2D(lowCull[i], 2));
            }
        }
        static private List<PointF> GetConvexCull(List<PointF> points, bool recursive)
        {
            List<PointF> cullPoints = new List<PointF>();
            PointF p1 = points[0];
            PointF p2 = points[1];
            PointF p3;
            cullPoints.Add(p1);

            for (int i = 2; i < points.Count; i++)
            {
                p3 = points[i];
                float product = p1.CrossProduct(p2, p3);
                if (product > 0) // turn left ==> Eliminate p2
                {
                    p2 = p3;
                }
                else if (product < 0) 
                {
                    cullPoints.Add(p2);
                    p1 = p2;
                    p2 = p3;
                }
            }
            cullPoints.Add(points.Last());

            if (recursive && cullPoints.Count > 3 && (points.Count -cullPoints.Count)>0)
            {
                return GetConvexCull(cullPoints, recursive);
            }

            return cullPoints;
        }

        /// <summary>
        /// Detects local high/lows
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private List<PointF> GetCull1(List<PointF> points)
        {
            List<PointF> cullPoints = new List<PointF>();
            PointF p1 = points[0];
            PointF p2 = points[1];
            PointF p3;
            cullPoints.Add(p1);

            for (int i = 2; i < points.Count; i++)
            {
                p3 = points[i];
                float product = p1.CrossProduct(p2, p3);
                if (product > 0) // turn left ==> Eliminate p2
                {
                    p2 = p3;
                }
                else
                {
                    cullPoints.Add(p2);
                    p1 = p2;
                    p2 = p3;
                }
            }

            return cullPoints;
        }
    }
}
