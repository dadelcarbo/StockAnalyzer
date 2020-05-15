using System;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILCUPHANDLEINV : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Detect Cup and Handle patterns and initiate trailing stop";

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 3 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500) };

        public override string[] SerieNames => new string[] { "TRAILCUPHANDLEINV.LS", "TRAILCUPHANDLEINV.SS" };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.parameters[0];
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);

            var lowestInSerie = stockSerie.GetIndicator($"LOWEST({period})").Series[0];
            this.series[0] = new FloatSerie(stockSerie.Count, SerieNames[0], float.NaN);
            this.series[0].Name = this.Name;
            this.series[1] = new FloatSerie(stockSerie.Count, SerieNames[1], float.NaN);
            this.series[1].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var brokenUpEvents = this.Events[Array.IndexOf<string>(this.EventNames, "BrokenUp")];
            var brokenDownEvents = this.Events[Array.IndexOf<string>(this.EventNames, "BrokenDown")];
            var bearEvents = this.Events[Array.IndexOf<string>(this.EventNames, "Bearish")];

            try
            {
                if (!stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
                {
                    stockSerie.StockAnalysis.DrawingItems.Add(stockSerie.BarDuration, new StockDrawingItems());
                }
                DrawingItem.CreatePersistent = false;

                var bodyHighSerie = new FloatSerie(stockSerie.Values.Select(v => Math.Max(v.OPEN, v.CLOSE)).ToArray());
                var bodyLowSerie = new FloatSerie(stockSerie.Values.Select(v => Math.Min(v.OPEN, v.CLOSE)).ToArray());

                bool isBear = false;
                float trailStop = float.NaN;
                float lowestInBars = float.MaxValue; // Reference for trailing stop

                for (int i = period * 2; i < stockSerie.Count; i++)
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
                            // Find previous body bottom
                            for (int k = i - 1; k > 1; k--)
                            {
                                if (bodyHighSerie[k] > bodyHighSerie[k - 1])
                                {
                                    trailStop = bodyLowSerie[k];
                                    break;
                                }
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

                        if (pivotIndex - startIndex < period || i - pivotIndex < period) // Pivot distance smaller than period
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
                        this.series[1][i] = trailStop = Math.Min(trailStop, high);
                        lowestInBars = lowestInSerie[i];
                        isBear = true;
                        brokenDownEvents[i] = bearEvents[i] = true;

                        // Draw open cup and handle
                        var cupHandle = new CupHandle2D(startPoint, endPoint, pivot, leftHigh, rightHigh, Pens.Black, true);
                        stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Insert(0, cupHandle);
                    }
                }
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }
    }
}