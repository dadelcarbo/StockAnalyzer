using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILCUPEMA : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Detect Cup and Handle patterns and initiate trailing stop based on LOW EMA";

        public override string[] ParameterNames => new string[] { "Period", "Right HL", "TrailPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 3, true, 12 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500), new ParamRangeBool(), new ParamRangeInt(0, 500) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            if (!stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
            {
                stockSerie.StockAnalysis.DrawingItems.Add(stockSerie.BarDuration, new StockDrawingItems());
            }
            else
            {
                stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].RemoveAll(di => !di.IsPersistent);
            }
            var period = (int)this.parameters[0];
            var rightHigherLow = (bool)this.parameters[1];
            var trailPeriod = (int)this.parameters[2];
            float alpha = 2.0f / (float)(trailPeriod + 1);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
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
            for (int i = period * 2; i < stockSerie.Count; i++)
            {
                if (isBull) // Trail Stop
                {
                    if (closeSerie[i] < trailStop) // Stop broken
                    {
                        isBull = false;
                        brokenDownEvents[i] = true;
                        continue;
                    }
                    else
                    {
                        trailStop = trailStop + alpha * (lowSerie[i] - trailStop);
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
                        this.series[0][i] = trailStop = rightLow.Y;
                        highestInBars = highestInSerie[i];
                        isBull = trailPeriod > 0;
                        brokenUpEvents[i] = bullEvents[i] = true;

                        // Draw open cup and handle
                        var cupHandle = new CupHandle2D(startPoint, endPoint, pivot, leftLow, rightLow, Pens.Black, false, false);
                        cupHandle.IsPersistent = false;
                        stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(cupHandle);
                    }
                }
            }

            // Generate events
            this.GenerateEvents(stockSerie, this.series[0], this.series[1]);
        }
    }
}