using System;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILCUPHANDLE : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Detect Cup and Handle patterns and initiate trailing stop";

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 3 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500) };

        public override string[] SerieNames => new string[] { "TRAILCUPHANDLE.LS", "TRAILCUPHANDLE.SS" };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.parameters[0];
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);

            var highestInSerie = stockSerie.GetIndicator($"HIGHEST({period})").Series[0];
            this.series[0] = new FloatSerie(stockSerie.Count, SerieNames[0], float.NaN);
            this.series[0].Name = this.Name;
            this.series[1] = new FloatSerie(stockSerie.Count, SerieNames[1], float.NaN);
            this.series[1].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var brokenUpEvents = this.Events[Array.IndexOf<string>(this.EventNames, "BrokenUp")];
            var brokenDownEvents = this.Events[Array.IndexOf<string>(this.EventNames, "BrokenDown")];
            var bullEvents = this.Events[Array.IndexOf<string>(this.EventNames, "Bullish")];

            try
            {
                if (!stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
                {
                    stockSerie.StockAnalysis.DrawingItems.Add(stockSerie.BarDuration, new StockDrawingItems());
                }
                DrawingItem.CreatePersistent = false;

                var bodyHighSerie = new FloatSerie(stockSerie.Values.Select(v => Math.Max(v.OPEN, v.CLOSE)).ToArray());
                var bodyLowSerie = new FloatSerie(stockSerie.Values.Select(v => Math.Min(v.OPEN, v.CLOSE)).ToArray());

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
                            trailStop = float.NaN;
                            brokenDownEvents[i] = true;
                            continue;
                        }
                        else if (highestInSerie[i] > highestInBars)// Trail Stop after target has been touched
                        {
                            // Find previous body bottom
                            for (int k = i - 1; k > 1; k--)
                            {
                                if (bodyLowSerie[k] < bodyLowSerie[k - 1])
                                {
                                    trailStop = bodyLowSerie[k];
                                    break;
                                }
                            }
                        }
                        this.series[0][i] = trailStop;
                        bullEvents[i] = true;
                    }
                    else
                    {
                        if (highestInSerie[i] == i) // Alltime high
                            continue;
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
                        trailStop = low;
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
                        this.series[0][i] = trailStop = Math.Max(trailStop, low);
                        highestInBars = highestInSerie[i];
                        isBull = true;
                        brokenUpEvents[i] = bullEvents[i] = true;

                        // Draw open cup and handle
                        var cupHandle = new CupHandle2D(startPoint, endPoint, pivot, leftLow, rightLow, Pens.Black);
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