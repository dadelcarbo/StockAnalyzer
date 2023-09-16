using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    public class StockAutoDrawing_CUPHANDLE : StockAutoDrawingBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Detect Cup and Handle patterns";

        public override string[] ParameterNames => new string[] { "Period", "Right HL" };

        public override Object[] ParameterDefaultValues => new Object[] { 3, true };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500), new ParamRangeBool() };

        public override string[] SerieNames => new string[] { };
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { };
                }
                return seriePens;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.parameters[0];
            var rightHigherLow = (bool)this.parameters[1];
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            var highestInSerie = stockSerie.GetIndicator($"HIGHEST({period})").Series[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var brokenUpEvents = this.Events[Array.IndexOf<string>(this.EventNames, "BrokenUp")];

            var bodyHighSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);
            var bodyLowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);

            for (int i = period * 2; i < stockSerie.Count; i++)
            {
                if (highestInSerie[i] <= (period * 2)) // Smaller than period
                    continue;

                // Find Pivot
                int startIndex = i - (int)highestInSerie[i];
                var pivotIndex = bodyHighSerie.FindMaxIndex(startIndex + 2, i - 1);
                while (pivotIndex - startIndex + 1 < period && i - pivotIndex > (period * 2))
                {
                    startIndex = pivotIndex;
                    pivotIndex = bodyHighSerie.FindMaxIndex(startIndex + 1, i - 1);
                }
                if (pivotIndex - startIndex + 1 < period || i - pivotIndex < period) // Pivot distance smaller than period
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
                if (!rightHigherLow || (rightHigherLow && rightLow.Y > leftLow.Y))
                {
                    brokenUpEvents[i] = true;

                    // Draw open cup and handle
                    var cupHandle = new CupHandle2D(startPoint, endPoint, pivot, leftLow, rightLow, Pens.Black, false, false);
                    this.DrawingItems.Insert(0, cupHandle);
                }
            }
        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[] { "BrokenUp" };
                }
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}