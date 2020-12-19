using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    class StockPaintBar_WEDGE : StockPaintBarBase
    {
        public StockPaintBar_WEDGE()
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
            get { return new string[] { "LongPeriod", "ShortPeriod" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 200, 100 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[] { "FallingWedge", "RisingWedge" };
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

        public override void ApplyTo(StockSerie stockSerie)
        {
            DrawingItem.CreatePersistent = false;
            try
            {
                // Detecting events
                this.CreateEventSeries(stockSerie.Count);

                int longPeriod = (int)this.parameters[0];
                int shortPeriod = (int)this.parameters[1];

                if (stockSerie.Count < longPeriod) return;

                if (stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
                {
                    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Clear();
                }
                else
                {
                    stockSerie.StockAnalysis.DrawingItems.Add(stockSerie.BarDuration, new StockDrawingItems());
                }
                StockDrawingItems drawingItems = stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration];

                FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
                FloatSerie LowSerie = stockSerie.GetSerie(StockDataType.LOW);
                HalfLine2D risingUpLine = null, nextRisingUpLine = null;
                PointF A, B;

                int highIndex = highSerie.FindMaxIndex(stockSerie.Count - longPeriod, stockSerie.Count - shortPeriod - 1);
                float high = highSerie[highIndex];

                A = new PointF(highIndex, high);
                B = new PointF(highIndex + 1, highSerie[highIndex + 1]);
                risingUpLine = new HalfLine2D(A, B);

                List<HalfLine2D> lines = new List<HalfLine2D>();

                int lastHighIndex = highIndex;
                bool theEnd = false;
                while (!theEnd)
                {
                    bool added = false;
                    for (int i = highIndex + 2; i < stockSerie.Count - shortPeriod; i++)
                    {
                        nextRisingUpLine = new HalfLine2D(A, new PointF(i, highSerie[i]));
                        if (nextRisingUpLine.a > risingUpLine.a)
                        {
                            risingUpLine = nextRisingUpLine;
                            added = true;
                            lastHighIndex = i;
                        }
                    }

                    if (!added)
                    {
                        theEnd = true;
                    }
                    else
                    {
                        bool cross = false;
                        for (int i = (int)risingUpLine.Point2.X+1; i < stockSerie.Count - shortPeriod; i++)
                        {
                            if (!risingUpLine.IsAbovePoint(new PointF(i, highSerie[i])))
                            {
                                cross = true;
                                break;
                            }
                        }
                        if (!cross)
                        {
                            lines.Add(risingUpLine);
                            highIndex = lastHighIndex;
                            A = risingUpLine.Point2;
                            B = new PointF(highIndex + 1, highSerie[highIndex + 1]);
                            risingUpLine = new HalfLine2D(A, B);
                        }
                    }
                }
                if (lines.Count > 0)
                {
                    HalfLine2D betterLine = lines.First();
                    foreach (var line in lines)
                    {
                        if (line.a < betterLine.a) betterLine = line;
                    }
                    drawingItems.Add(betterLine);
                }
            }
            finally
            {
                DrawingItem.CreatePersistent = false;
            }
        }
    }
}
