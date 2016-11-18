using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    class StockPaintBar_CUPHANDLE : StockPaintBarBase
    {
        public StockPaintBar_CUPHANDLE()
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
                    eventNames = new string[] { "CUPHANDLE", "ReversedCUPHANDLE" };
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

                if (stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
                {
                    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Clear();
                }
                else
                {
                    stockSerie.StockAnalysis.DrawingItems.Add(stockSerie.BarDuration, new StockDrawingItems());
                }
                StockDrawingItems drawingItems = stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration];

                int longPeriod = (int)this.parameters[0];
                int shortPeriod = (int)this.parameters[1];

                FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                HalfLine2D supportLine = null; ;
                float longHigh = float.MinValue, longLow = float.MaxValue, shortHigh = float.MinValue, shortLow = float.MaxValue;
                for (int i = longPeriod; i < stockSerie.Count; i++)
                {
                    closeSerie.GetMinMax(i - longPeriod, i - shortPeriod, ref longLow, ref longHigh);
                    closeSerie.GetMinMax(i - shortPeriod, i - 1, ref shortLow, ref shortHigh);
                    float longRange = longHigh - longLow;
                    float shortRange = shortHigh - shortLow;
                    if (shortRange > longRange) continue;

                    // Check bullish Cup And Handle
                    if (closeSerie[i] > longHigh && shortLow > longLow && shortHigh < longHigh)
                    {
                        this.eventSeries[0][i] = true;
                        int highIndex = closeSerie.FindMaxIndex(i - longPeriod, i - shortPeriod);
                        int lowIndex = closeSerie.FindMinIndex(i - longPeriod, i - shortPeriod);

                        drawingItems.Add(new Rectangle2D(new PointF(i - longPeriod, longHigh), new PointF(i, longLow)));

                        //drawingItems.Add(new Bullet2D(new PointF(highIndex, longHigh), 3) { Pen = SeriePens[0] });
                        //drawingItems.Add(new Bullet2D(new PointF(lowIndex, longLow), 3) { Pen = SeriePens[0] });

                        //drawingItems.Add(new Segment2D(highIndex, longHigh, i, longHigh) { Pen = SeriePens[0] });
                        //drawingItems.Add(new Segment2D(i - shortPeriod, shortLow, i, shortLow) { Pen = SeriePens[0] });

                        //// Support management
                        //PointF lowPoint = new PointF(lowIndex, longLow);
                        //HalfLine2D support = new HalfLine2D(lowPoint, new PointF(lowIndex + 1, closeSerie[lowIndex + 1])) { Pen = SeriePens[0] };
                        //for (int j = lowIndex + 2; j < i; j++)
                        //{
                        //    HalfLine2D line = new HalfLine2D(new PointF(lowIndex, longLow), new PointF(j, closeSerie[j])) { Pen = SeriePens[0] };
                        //    if (line.a < support.a) support = line;
                        //}
                        //drawingItems.Add(support);
                    }

                    //// Check bearish Cup And Handle
                    //if (closeSerie[i] < longLow && shortHigh < longHigh && shortLow > longLow)
                    //{
                    //    this.eventSeries[1][i] = true;

                    //    int lowhIndex = closeSerie.FindMinIndex(i - longPeriod, i - shortPeriod);
                    //    drawingItems.Add(new Bullet2D(new PointF(lowhIndex, longLow), 3) { Pen = SeriePens[1] });

                    //    drawingItems.Add(new Segment2D(lowhIndex, longLow, i, longLow) { Pen = SeriePens[1] });
                    //    drawingItems.Add(new Segment2D(i - shortPeriod, shortHigh, i, shortHigh) { Pen = SeriePens[1] });
                    //}
                }
            }
            finally
            {
                DrawingItem.CreatePersistent = false;
            }
        }
    }
}
