using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockData;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILCUPHL : StockTrailStopBase
    {
        public override string Definition => "Detect Cup and Handle patterns and initiate trailing stop based on DONCHIAN LOW";

        public override string[] ParameterNames => new string[] { "Period", "Right HL", "TrailPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 3, true, 12 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500), new ParamRangeBool(), new ParamRangeInt(0, 500) };

        public override void ApplyTo(DataSerie dataSerie)
        {
            if (!dataSerie.Instrument.StockAnalysis.DrawingItems.ContainsKey(dataSerie.BarDuration))
            {
                dataSerie.Instrument.StockAnalysis.DrawingItems.Add(dataSerie.BarDuration, new StockDrawingItems());
            }
            else
            {
                dataSerie.Instrument.StockAnalysis.DrawingItems[dataSerie.BarDuration].RemoveAll(di => !di.IsPersistent);
            }
            var period = (int)this.parameters[0];
            var rightHigherLow = (bool)this.parameters[1];
            var trailPeriod = (int)this.parameters[2];
            FloatSerie closeSerie = dataSerie.GetSerie(StockDataType.CLOSE);

            this.series[0] = new FloatSerie(dataSerie.Count, SerieNames[0], float.NaN);
            this.series[0].Name = this.SerieNames[0];
            this.series[1] = new FloatSerie(dataSerie.Count, SerieNames[1], float.NaN);
            this.series[1].Name = this.SerieNames[1];

            // Detecting events
            this.CreateEventSeries(dataSerie.Count);
            var brokenUpEvents = this.Events[Array.IndexOf(this.EventNames, "BrokenUp")];
            var brokenDownEvents = this.Events[Array.IndexOf(this.EventNames, "BrokenDown")];
            var bullEvents = this.Events[Array.IndexOf(this.EventNames, "Bullish")];

            var bodyLowSerie = dataSerie.GetSerie(StockDataType.LOW);

            bool isBull = false;
            float trailStop = float.NaN;
            for (int i = Math.Max(period * 2, trailPeriod * 2); i < dataSerie.Count; i++)
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
                        dataSerie.Instrument.StockAnalysis.DrawingItems[dataSerie.BarDuration].Add(cupHandle);
                    }
                }
            }

            // Generate events
            this.GenerateEvents(dataSerie, this.series[0], this.series[1]);
        }
    }
}