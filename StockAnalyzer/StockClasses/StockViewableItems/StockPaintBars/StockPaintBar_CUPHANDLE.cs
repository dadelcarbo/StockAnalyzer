using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    class StockPaintBar_CUPHANDLE : StockPaintBarBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Detect Cup and Handle patterns and initiate trailing stop";

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 3 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500) };

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[] { "CupAndHandle" };
                }
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }

        public override Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green) { Width = 2 } };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.parameters[0];
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);

            var highestInSerie = stockSerie.GetIndicator($"HIGHEST({period})").Series[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var brokenUpEvents = this.Events[Array.IndexOf<string>(this.EventNames, "CupAndHandle")];

            try
            {
                if (!stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
                {
                    stockSerie.StockAnalysis.DrawingItems.Add(stockSerie.BarDuration, new StockDrawingItems());
                }
                DrawingItem.CreatePersistent = false;

                var bodyHighSerie = new FloatSerie(stockSerie.Values.Select(v => Math.Max(v.OPEN, v.CLOSE)).ToArray());
                var bodyLowSerie = new FloatSerie(stockSerie.Values.Select(v => Math.Min(v.OPEN, v.CLOSE)).ToArray());

                for (int i = period * 2; i < stockSerie.Count; i++)
                {
                    if (highestInSerie[i] == i) // Alltime high
                        continue;
                    if (highestInSerie[i] <= (period * 2)) // Smaller than period
                        continue;

                    if (stockSerie.StockName == "VIVENDI" && i == 208)
                        Console.WriteLine("Here");


                    // Find Pivot
                    int startIndex = i - (int)highestInSerie[i];
                    var pivotIndex = bodyHighSerie.FindMaxIndex(startIndex + 1, i - 1);

                    if (pivotIndex - startIndex < period || i - pivotIndex < period) // Pivot distance smaller than period
                        continue;

                    var pivot = new PointF { X = pivotIndex, Y = bodyHighSerie[pivotIndex] };
                    var startPoint = new PointF { X = startIndex - 1, Y = pivot.Y };
                    var endPoint = new PointF { X = i, Y = pivot.Y };

                    // Calculate  right and left lows
                    var leftLow = new PointF();
                    var rightLow = new PointF();
                    var low = float.MaxValue;
                    for (int k = startIndex; k < pivotIndex; k++)
                    {
                        var bodyLow = bodyLowSerie[k];
                        if (low >= bodyLow)
                        {
                            leftLow.X = k;
                            leftLow.Y = low = bodyLow;
                        }
                    }

                    low = float.MaxValue;
                    for (int k = pivotIndex + 1; k < i; k++)
                    {
                        var bodyLow = bodyLowSerie[k];
                        if (low > bodyLow)
                        {
                            rightLow.X = k;
                            rightLow.Y = low = bodyLow;
                        }
                    }

                    // Draw open cup and handle
                    var cupHandle = new CupHandle2D(startPoint, endPoint, pivot, leftLow, rightLow, Pens.Black);
                    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Insert(0, cupHandle);
                }
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }
    }
}
