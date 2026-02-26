using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILCUPPIVOT : StockTrailStopBase
    {
        public override string Definition => "Detect Cup and Handle patterns and initiate trailing stop based on handle low";

        public override string[] ParameterNames => new string[] { "Period", "Right HL", "InputSmoothing" };

        public override Object[] ParameterDefaultValues => new Object[] { 3, true, 1 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500), new ParamRangeBool(), new ParamRangeInt(1, 500) };

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
            var inputSmoothing = (int)this.parameters[2];
            FloatSerie closeSerie = inputSmoothing > 1 ? stockSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(inputSmoothing) : stockSerie.GetSerie(StockDataType.CLOSE);

            this.series[0] = new FloatSerie(stockSerie.Count, SerieNames[0], float.NaN);
            this.series[0].Name = this.SerieNames[0];
            this.series[1] = new FloatSerie(stockSerie.Count, SerieNames[1], float.NaN);
            this.series[1].Name = this.SerieNames[1];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var brokenUpEvents = this.Events[Array.IndexOf(this.EventNames, "BrokenUp")];
            var brokenDownEvents = this.Events[Array.IndexOf(this.EventNames, "BrokenDown")];
            var bullEvents = this.Events[Array.IndexOf(this.EventNames, "Bullish")];

            var bodyLowSerie = stockSerie.GetSerie(StockDataType.LOW);

            bool isBull = false;
            float trailStop = float.NaN;
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
                        var cupHandle = closeSerie.DetectCupHandle(i, period, rightHigherLow);
                        if (cupHandle != null)
                        {
                            trailStop = Math.Max(trailStop, cupHandle.RightLow.Y);

                            cupHandle.IsPersistent = false;
                            stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(cupHandle);
                        }
                    }
                    this.series[0][i] = trailStop;
                    bullEvents[i] = true;
                }
                else
                {
                    var cupHandle = closeSerie.DetectCupHandle(i, period, rightHigherLow);
                    if (cupHandle != null)
                    {
                        this.series[0][i] = trailStop = cupHandle.RightLow.Y;
                        isBull = true;
                        brokenUpEvents[i] = bullEvents[i] = true;

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