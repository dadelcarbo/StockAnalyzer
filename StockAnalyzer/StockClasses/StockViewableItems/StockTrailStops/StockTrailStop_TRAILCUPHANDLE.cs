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

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500) };

        public override string[] SerieNames => new string[] { "TRAILCUPHANDLE.LS", "TRAILCUPHANDLE.SS" };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.parameters[0];
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

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
                float entry = float.NaN;
                float trailStop = float.NaN;
                float target = float.NaN;

                for (int i = period * 2; i < stockSerie.Count; i++)
                {
                    if (isBull) // Trail Stop
                    {
                        if (float.IsNaN(entry))
                        {
                            entry = openSerie[i];
                            target = entry + entry - trailStop;
                        }
                        if (!float.IsNaN(target) && highSerie[i] > target) // Target R1 touched
                        {
                            trailStop = entry;
                            target = float.NaN;
                        }
                        if (closeSerie[i] < trailStop) // Stop broken
                        {
                            isBull = false;
                            trailStop = float.NaN;
                            entry = float.NaN;
                            brokenDownEvents[i] = true;
                            continue;
                        }
                        else if (float.IsNaN(target))// Trail Stop after target has been touched
                        {
                            trailStop = bodyLowSerie.GetMin(i - period, i);
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
                        var pivotIndex = bodyHighSerie.FindMaxIndex(startIndex + 1, i - 1);

                        if (pivotIndex - startIndex < period || i - pivotIndex < period) // Pivot distance smaller than period
                            continue;

                        var pivot = new PointF { X = pivotIndex, Y = bodyHighSerie[pivotIndex] };
                        var startPoint = new PointF { X = startIndex, Y = pivot.Y };
                        var endPoint = new PointF { X = i, Y = pivot.Y };

                        // Calculate  right and left lows
                        var leftLow = new PointF();
                        var rightLow = new PointF();
                        var low = float.MaxValue;
                        for (int k = startIndex + 1; k < pivotIndex; k++)
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

                        // Draw open cup and handle
                        var cupHandle = new CupHandle2D(startPoint, endPoint, pivot, leftLow, rightLow, Pens.Black);
                        isBull = true;
                        brokenUpEvents[i] = bullEvents[i] = true;
                        
                        stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(cupHandle);
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