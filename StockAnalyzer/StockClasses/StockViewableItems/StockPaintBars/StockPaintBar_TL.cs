using StockAnalyzer.StockDrawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_TL : StockPaintBarBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }

        public override bool HasTrendLine { get { return true; } }

        public override string Definition
        {
            get { return "Draws trendlines"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "LeftStrenth", "RightStrength", "NbPivots" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 3, 3, 10 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 10), new ParamRangeInt(1, 10), new ParamRangeInt(2, 50) }; }
        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = Enum.GetNames(typeof(StockSerie.TLEvent));
                }
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true, true, true, true, false, false };
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
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Green), new Pen(Color.Red) };
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
            try
            {
                if (!stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
                {
                    stockSerie.StockAnalysis.DrawingItems.Add(stockSerie.BarDuration, new StockDrawingItems());
                }
                else
                {
                    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].RemoveAll(di => !di.IsPersistent);
                }
                DrawingItem.CreatePersistent = false;

                var highSerie = stockSerie.GetSerie(StockDataType.HIGH);
                var lowSerie = stockSerie.GetSerie(StockDataType.LOW);
                var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

                Stack<PointF> bottomStack = new Stack<PointF>();
                Line2DBase support = null;
                for (int i = 2; i < stockSerie.Count; i++)
                {
                    if (support != null)
                    {
                        if (support.ValueAtX(i) > closeSerie[i])
                        {
                            // Support broken
                            stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Remove(support);
                            stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(support.Cut(i, true));
                            support = null;
                            bottomStack.Pop();
                        }
                    }
                    float bottom = lowSerie[i - 1];
                    if (lowSerie[i] > bottom && lowSerie[i - 2] > bottom) // Check if bottom
                    {
                        var bottomPoint = new PointF(i - 1, bottom);
                        stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(new Bullet2D(bottomPoint, 3, Brushes.Green));

                        if (bottomStack.Count > 0 && bottomStack.Peek().Y < bottom)
                        {
                            bottomStack.Push(bottomPoint);
                            // Add steepest line
                            if (bottomStack.Count > 1)
                            {
                                float maxSteep = float.MinValue;
                                HalfLine2D maxLine = null;
                                foreach (var p in bottomStack.Skip(1))
                                {
                                    var line = new HalfLine2D(p, bottomPoint);
                                    if (line.VY > maxSteep)
                                    {
                                        bool broken = false;
                                        for (int j = (int)(p.X + 1); j < i - 2; j++)
                                        {
                                            if (line.IsAbovePoint(new PointF(j, closeSerie[j])))
                                            {
                                                broken = true;
                                                break;
                                            }
                                        }
                                        if (!broken)
                                        {
                                            maxSteep = line.VY;
                                            maxLine = line;
                                        }
                                    }
                                }
                                if (maxLine != null)
                                {
                                    support = maxLine;
                                    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(support);
                                }
                            }
                            else
                            {
                                while (bottomStack.Count > 0 && bottomStack.Peek().Y > bottom)
                                {
                                    bottomStack.Pop();
                                }
                                bottomStack.Push(bottomPoint);
                            }
                        }
                    }
                }
                //foreach (var p in bottomStack)
                //{
                //    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(new Bullet2D(p, 3, Brushes.Green));
                //}
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }

        public void ApplyTo2(StockSerie stockSerie)
        {
            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            try
            {
                if (!stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
                {
                    stockSerie.StockAnalysis.DrawingItems.Add(stockSerie.BarDuration, new StockDrawingItems());
                }
                else
                {
                    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].RemoveAll(di => !di.IsPersistent);
                }
                DrawingItem.CreatePersistent = false;

                var highSerie = stockSerie.GetSerie(StockDataType.HIGH);
                var lowSerie = stockSerie.GetSerie(StockDataType.LOW);
                var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

                Stack<PointF> bottomStack = new Stack<PointF>();
                Line2DBase support = null;
                for (int i = 2; i < stockSerie.Count; i++)
                {
                    if (support != null)
                    {
                        if (support.ValueAtX(i) > closeSerie[i])
                        {
                            // Support broken
                            stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Remove(support);
                            stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(support.Cut(i, true));
                            support = null;
                            bottomStack.Pop();
                        }
                    }
                    float bottom = lowSerie[i - 1];
                    if (lowSerie[i] > bottom && lowSerie[i - 2] > bottom) // Check if bottom
                    {
                        var bottomPoint = new PointF(i - 1, bottom);
                        stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(new Bullet2D(bottomPoint, 3, Brushes.Green));

                        if (bottomStack.Count > 0 && bottomStack.Peek().Y < bottom)
                        {
                            bottomStack.Push(bottomPoint);
                            // Add steepest line
                            if (bottomStack.Count > 1)
                            {
                                float maxSteep = float.MinValue;
                                HalfLine2D maxLine = null;
                                foreach (var p in bottomStack.Skip(1))
                                {
                                    var line = new HalfLine2D(p, bottomPoint);
                                    if (line.VY > maxSteep)
                                    {
                                        maxSteep = line.VY;
                                        maxLine = line;
                                    }
                                }
                                if (maxLine != null)
                                {
                                    support = maxLine;
                                    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(support);
                                }
                            }
                        }
                        else
                        {
                            while (bottomStack.Count > 0 && bottomStack.Peek().Y > bottom)
                            {
                                bottomStack.Pop();
                            }
                            bottomStack.Push(bottomPoint);
                        }
                    }
                }
                //foreach (var p in bottomStack)
                //{
                //    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(new Bullet2D(p, 3, Brushes.Green));
                //}
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }
    }
}
