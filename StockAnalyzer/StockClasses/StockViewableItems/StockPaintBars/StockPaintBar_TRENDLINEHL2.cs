using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_TRENDLINEHL2 : StockPaintBarBase
    {
        public StockPaintBar_TRENDLINEHL2()
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
            get { return new string[] { "Period", "NbPivots", "Lookback" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 1, 2, 200 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 200), new ParamRangeInt(2, 100), new ParamRangeInt(50, 500) }; }
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
            /// Detecting events
            this.CreateEventSeries(stockSerie.Count);

            stockSerie.generateAutomaticHL3TrendLines(0, stockSerie.Count - 1,
                (int)this.parameters[0],
                (int)this.parameters[1],
                (int)this.parameters[2],
                ref this.eventSeries);
        }
    }
}
namespace StockAnalyzer.StockClasses
{
    public class StockPivot
    {
        public int Index { get; set; }
        public float Value { get; set; }
        public PointF Point { get; set; }

        public StockPivot(int index, float value)
        {
            this.Index = index;
            this.Value = value;
            this.Point = new PointF(index, value);
        }
        public override string ToString()
        {
            return this.Point.ToString();
        }
    }

    public class MyStack<T> : List<T>
    {
        public int MaxSize { get; set; }
        public MyStack() : base(100)
        {
            MaxSize = 100;
        }
        public MyStack(int size) : base(size)
        {
            MaxSize = size;
        }
        ///
        /// <summary>
        ///     Returns the object at the top of the System.Collections.Generic.Stack`1 without
        ///     removing it.
        /// </summary>
        /// <returns>
        ///     The object at the top of the System.Collections.Generic.Stack`1.
        /// </returns>
        ///<exceptions>
        ///   T:System.InvalidOperationException:
        ///     The System.Collections.Generic.Stack`1 is empty.
        /// </exceptions>
        public T Peek()
        {
            if (this.Count > 0)
            {
                return this.Last();
            }
            else
            {
                throw new InvalidOperationException("The System.Collections.Generic.Stack`1 is empty");
            }
        }
        //
        /// Summary:
        ///     Removes and returns the object at the top of the System.Collections.Generic.Stack`1.
        //
        /// Returns:
        ///     The object removed from the top of the System.Collections.Generic.Stack`1.
        //
        /// Exceptions:
        ///   T:System.InvalidOperationException:
        ///     The System.Collections.Generic.Stack`1 is empty.
        public T Pop()
        {
            if (this.Count > 0)
            {
                T value = this.Last();
                this.RemoveAt(this.Count - 1);
                return value;
            }
            else
            {
                throw new InvalidOperationException("The System.Collections.Generic.Stack`1 is empty");
            }
        }
        //
        /// Summary:
        ///     Inserts an object at the top of the System.Collections.Generic.Stack`1.
        //
        /// Parameters:
        ///   item:
        ///     The object to push onto the System.Collections.Generic.Stack`1. The value can
        ///     be null for reference types.
        public void Push(T item)
        {
            if (this.Count >= this.MaxSize)
            {
                this.RemoveAt(0);
            }
            this.Add(item);
        }
    }

    public partial class StockSerie : SortedDictionary<DateTime, StockDailyValue>, IXmlSerializable
    {
        public void generateAutomaticHL3TrendLines(int startIndex, int endIndex, int period, int nbPivots, int maxPeriod, ref BoolSerie[] events)
        {
            DrawingItem.CreatePersistent = false;
            try
            {
                #region Initialisation
                IStockIndicator indicator = this.GetIndicator("TRAILHLSR(" + period + ")");
                FloatSerie longStop = indicator.Series[0];
                FloatSerie shortStop = indicator.Series[1];

                BoolSerie brokenDown = indicator.Events[1];
                BoolSerie brokenUp = indicator.Events[0];

                MyStack<StockPivot> highPivots = new MyStack<StockPivot>(nbPivots);
                MyStack<StockPivot> lowPivots = new MyStack<StockPivot>(nbPivots);

                FloatSerie lowSerie = this.GetSerie(StockDataType.LOW);
                FloatSerie highSerie = this.GetSerie(StockDataType.HIGH);
                FloatSerie closeSerie = this.GetSerie(StockDataType.CLOSE);

                if (this.StockAnalysis.DrawingItems.ContainsKey(this.BarDuration))
                {
                    this.StockAnalysis.DrawingItems[this.BarDuration].Clear();
                }
                else
                {
                    this.StockAnalysis.DrawingItems.Add(this.BarDuration, new StockDrawingItems());
                }

                Pen greenLargePen = new Pen(Color.Green, 2);
                Pen redLargePen = new Pen(Color.Red, 2);
                Pen greenThinPen = new Pen(Color.Green, 1);
                Pen redThinPen = new Pen(Color.Red, 1);
                #endregion
                for (int i = startIndex + period; i <= endIndex; i++)
                {
                    // Remove old Highs from list
                    highPivots.RemoveAll(p => p.Index < i - maxPeriod);
                    highPivots.RemoveAll(p => p.Value < closeSerie[i]);

                    #region Resistance detection
                    if (brokenDown[i])
                    {
                        int pivotIndex = 0;
                        for (pivotIndex = i; highSerie[pivotIndex] < shortStop[i]; pivotIndex--) ;
                        StockPivot pivot = new StockPivot(pivotIndex, shortStop[i]);

                        // Remove lower Highs from list
                        while (highPivots.Count > 0 && highPivots.Peek().Value < pivot.Value)
                        {
                            highPivots.Pop();
                        }
                        highPivots.Push(pivot);
                    }
                    else
                    {
                        if (highPivots.Count > 1)
                        {
                            /// Check Broken lines
                            for (int a1 = 0; a1 < highPivots.Count - 1; a1++)
                            {
                                var A1 = highPivots.ElementAt(a1);
                                for (int a2 = a1 + 1; a2 < highPivots.Count; a2++)
                                {
                                    var A2 = highPivots.ElementAt(a2);
                                    var line = new HalfLine2D(A1.Point, A2.Point, greenLargePen);

                                    //events[(int)TLEvent.DownTrend][i] = supportList.Count == 0;
                                    var closePoint = new PointF(i, closeSerie[i]);
                                    if (!line.IsAbovePoint(closePoint))
                                    {
                                        events[(int)TLEvent.SupportBroken][i] = true;
                                        this.StockAnalysis.DrawingItems[this.BarDuration].Add(line.Cut(i, true));
                                        this.StockAnalysis.DrawingItems[this.BarDuration].Add(new Bullet2D(A1.Point, 2));
                                        this.StockAnalysis.DrawingItems[this.BarDuration].Add(new Bullet2D(A2.Point, 2));

                                        /// Remove last point
                                        highPivots.Pop();
                                    }
                                }
                            }
                        }
                    }
                    events[(int)TLEvent.UpTrend][i] = highPivots.Count <= 1;
                    #endregion
                    #region Support detection
                    if (brokenUp[i])
                    {
                        int pivotIndex = 0;
                        for (pivotIndex = i; lowSerie[pivotIndex] > longStop[i]; pivotIndex--) ;
                        StockPivot pivot = new StockPivot(pivotIndex, longStop[i]);

                        // Remove old Highs from list
                        lowPivots.RemoveAll(p => p.Index < i - maxPeriod);
                        // Remove lower Highs from list
                        while (lowPivots.Count > 0 && lowPivots.Peek().Value > pivot.Value)
                        {
                            lowPivots.Pop();
                        }
                        lowPivots.Push(pivot);
                    }
                    else
                    {
                        if (lowPivots.Count > 1)
                        {
                            /// Check Broken lines
                            for (int a1 = 0; a1 < lowPivots.Count - 1; a1++)
                            {
                                var A1 = lowPivots.ElementAt(a1);
                                for (int a2 = a1 + 1; a2 < lowPivots.Count; a2++)
                                {
                                    var A2 = lowPivots.ElementAt(a2);
                                    var line = new HalfLine2D(A1.Point, A2.Point, redLargePen);

                                    var closePoint = new PointF(i, closeSerie[i]);
                                    if (line.IsAbovePoint(closePoint))
                                    {
                                        events[(int)TLEvent.ResistanceBroken][i] = true;
                                        this.StockAnalysis.DrawingItems[this.BarDuration].Add(line.Cut(i, true));
                                        this.StockAnalysis.DrawingItems[this.BarDuration].Add(new Bullet2D(A1.Point, 2));
                                        this.StockAnalysis.DrawingItems[this.BarDuration].Add(new Bullet2D(A2.Point, 2));

                                        /// Remove last point
                                        lowPivots.Pop();
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                #region Add not broken lines
                if (highPivots.Count > 1)
                {
                    /// Check Broken lines
                    for (int a1 = 0; a1 < highPivots.Count - 1; a1++)
                    {
                        var A1 = highPivots.ElementAt(a1);
                        for (int a2 = a1 + 1; a2 < highPivots.Count; a2++)
                        {
                            var A2 = highPivots.ElementAt(a2);
                            var line = new HalfLine2D(A1.Point, A2.Point, greenThinPen);

                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(line);
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(new Bullet2D(A1.Point, 2));
                            this.StockAnalysis.DrawingItems[this.BarDuration].Add(new Bullet2D(A2.Point, 2));
                        }
                    }
                }
                #endregion
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }
    }
}

