using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    public class StockAutoDrawing_CUPHANDLETRAIL : StockAutoDrawingBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Detect Cup and Handle patterns and initiate trailing stop";

        public override string[] ParameterNames => new string[] { "Period", "Right HL", "TrailPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 3, true, 12 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500), new ParamRangeBool(), new ParamRangeInt(0, 500) };

        public override string[] SerieNames => new string[] { "CUPHANDLETRAIL.LS", "CUPHANDLETRAIL.SS" };
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green, 2), new Pen(Color.Red, 2) };
                    seriePens[0].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    seriePens[1].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                }
                return seriePens;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.parameters[0];
            var rightHigherLow = (bool)this.parameters[1];
            var trailPeriod = (int)this.parameters[2];
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            var highestInSerie = stockSerie.GetIndicator($"HIGHEST({period})").Series[0];
            this.series[0] = new FloatSerie(stockSerie.Count, SerieNames[0], float.NaN);
            this.series[0].Name = this.SerieNames[0];
            this.series[1] = new FloatSerie(stockSerie.Count, SerieNames[1], float.NaN);
            this.series[1].Name = this.SerieNames[1];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var brokenUpEvents = this.Events[Array.IndexOf<string>(this.EventNames, "BrokenUp")];
            var brokenDownEvents = this.Events[Array.IndexOf<string>(this.EventNames, "BrokenDown")];
            var bullEvents = this.Events[Array.IndexOf<string>(this.EventNames, "Bullish")];

            var bodyHighSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);
            var bodyLowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);

            bool isBull = false;
            float trailStop = float.NaN;
            float highestInBars = float.MaxValue; // Reference for trailing stop
            for (int i = Math.Max(period * 2, trailPeriod * 2); i < stockSerie.Count; i++)
            {
                if (isBull) // Trail Stop
                {
                    if (closeSerie[i] < trailStop) // Stop broken
                    {
                        isBull = false;
                        brokenDownEvents[i] = true;
                        continue;
                    }
                    else if (highestInSerie[i] > highestInBars)// Trail Stop after target has been touched
                    {
                        if (bodyLowSerie[i - 1] < bodyLowSerie[i])
                        {
                            trailStop = Math.Max(trailStop, bodyLowSerie.GetMin(i - trailPeriod, i));
                        }
                    }
                    this.series[0][i] = trailStop;
                    bullEvents[i] = true;
                }
                else
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
                        this.series[0][i] = trailStop = Math.Min(Math.Max(rightLow.Y, leftLow.Y), bodyLowSerie.GetMin(i - trailPeriod, i));
                        highestInBars = highestInSerie[i];
                        isBull = trailPeriod > 0;
                        brokenUpEvents[i] = bullEvents[i] = true;

                        // Draw open cup and handle
                        var cupHandle = new CupHandle2D(startPoint, endPoint, pivot, leftLow, rightLow, Pens.Black, false, false);
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
                    eventNames = new string[] { "BrokenUp", "BrokenDown", "Bullish" };
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