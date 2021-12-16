using System;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    public class StockAutoDrawing_CUPHANDLEINV : StockAutoDrawingBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Detect inverted Cup and Handle patterns and initiate trailing stop";

        public override string[] ParameterNames => new string[] { "Period", "Right LH", "TrailPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 3, true, 12 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500), new ParamRangeBool(), new ParamRangeInt(2, 500) };

        public override string[] SerieNames => new string[] { "CUPHANDLEINV.LS", "CUPHANDLEINV.SS" };
 public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Red, 2), new Pen(Color.Green, 2) };
                    seriePens[0].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    seriePens[1].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                }
                return seriePens;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.parameters[0];
            var rightLowerHigh = (bool)this.parameters[1];
            var trailPeriod = (int)this.parameters[2];
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);

            var lowestInSerie = stockSerie.GetIndicator($"LOWEST({period})").Series[0];
            this.series[0] = new FloatSerie(stockSerie.Count, SerieNames[0], float.NaN);
            this.series[0].Name = this.SerieNames[0];
            this.series[1] = new FloatSerie(stockSerie.Count, SerieNames[1], float.NaN);
            this.series[1].Name = this.SerieNames[1];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var brokenUpEvents = this.Events[Array.IndexOf<string>(this.EventNames, "BrokenUp")];
            var brokenDownEvents = this.Events[Array.IndexOf<string>(this.EventNames, "BrokenDown")];
            var bearEvents = this.Events[Array.IndexOf<string>(this.EventNames, "Bearish")];

            var bodyHighSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);
            var bodyLowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);

            bool isBear = false;
            float trailStop = float.NaN;
            float lowestInBars = float.MaxValue; // Reference for trailing stop

            for (int i = Math.Max(period * 2, trailPeriod * 2); i < stockSerie.Count; i++)
            {
                if (isBear) // Trail Stop
                {
                    if (closeSerie[i] > trailStop) // Stop broken
                    {
                        isBear = false;
                        trailStop = float.NaN;
                        brokenUpEvents[i] = true;
                        continue;
                    }
                    else if (lowestInSerie[i] > lowestInBars)// Trail Stop after target has been touched
                    {
                        if (bodyHighSerie[i - 1] > bodyHighSerie[i])
                        {
                            trailStop = Math.Min(trailStop, bodyHighSerie.GetMax(i - trailPeriod, i));
                        }
                    }
                    this.series[1][i] = trailStop;
                    bearEvents[i] = true;
                }
                else
                {
                    if (lowestInSerie[i] == i) // Alltime low
                        continue;
                    if (lowestInSerie[i] <= (period * 2)) // Smaller than period
                        continue;

                    // Find Pivot
                    int startIndex = i - (int)lowestInSerie[i];
                    var pivotIndex = bodyLowSerie.FindMinIndex(startIndex + 1, i - 1);

                    while (pivotIndex - startIndex + 1 < period && i - pivotIndex > (period * 2))
                    {
                        startIndex = pivotIndex;
                        pivotIndex = bodyHighSerie.FindMinIndex(startIndex + 1, i - 1);
                    }
                    if (pivotIndex - startIndex + 1 < period || i - pivotIndex < period) // Pivot distance smaller than period
                        continue;

                    var pivot = new PointF { X = pivotIndex, Y = bodyLowSerie[pivotIndex] };
                    var startPoint = new PointF { X = startIndex - 1, Y = pivot.Y };
                    var endPoint = new PointF { X = i, Y = pivot.Y };

                    // Calculate  right and left highs
                    var leftHigh = new PointF();
                    var rightHigh = new PointF();
                    var high = float.MinValue;
                    for (int k = startIndex; k < pivotIndex; k++)
                    {
                        var bodyHigh = bodyHighSerie[k];
                        if (high <= bodyHigh)
                        {
                            leftHigh.X = k;
                            leftHigh.Y = high = bodyHigh;
                        }
                    }
                    trailStop = high;
                    high = float.MinValue;
                    for (int k = pivotIndex + 1; k < i; k++)
                    {
                        var bodyHigh = bodyHighSerie[k];
                        if (high < bodyHigh)
                        {
                            rightHigh.X = k;
                            rightHigh.Y = high = bodyHigh;
                        }
                    }
                    if (!rightLowerHigh || (rightLowerHigh && rightHigh.Y < leftHigh.Y))
                    {
                        this.series[1][i] = trailStop = Math.Min(trailStop, high);
                        lowestInBars = lowestInSerie[i];
                        isBear = true;
                        brokenDownEvents[i] = bearEvents[i] = true;

                        // Draw open cup and handle
                        var cupHandle = new CupHandle2D(startPoint, endPoint, pivot, leftHigh, rightHigh, Pens.Black, true);
                        this.DrawingItems.Insert(0, cupHandle);
                    }
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
                    eventNames = new string[] { "BrokenDown", "BrokenUp", "Bearish" };
                }
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true, true, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}