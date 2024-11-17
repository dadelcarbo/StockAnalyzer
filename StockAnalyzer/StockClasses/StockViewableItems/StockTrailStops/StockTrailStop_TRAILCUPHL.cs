using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILCUPHL : StockTrailStopBase
    {
        public override string Definition => "Detect Cup and Handle patterns and initiate trailing stop based on DONCHIAN LOW";

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
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

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

            var bodyLowSerie = stockSerie.GetSerie(StockDataType.LOW);

            bool isBull = false;
            float trailStop = float.NaN;
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
                    else
                        trailStop = Math.Max(trailStop, bodyLowSerie.GetMin(i - trailPeriod, i));

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