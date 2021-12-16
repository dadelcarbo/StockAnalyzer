using System;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILBREAK : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Initiate trail stop after higher low";

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 3 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "TRAILBREAK.LS", "TRAILBREAK.SS" };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.parameters[0];
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            var hlTrailStop = stockSerie.GetTrailStop($"TRAILHL({period})");
            int higherLowIndex = Array.IndexOf<string>(hlTrailStop.EventNames, "HigherLow");
            int brokenUpIndex = Array.IndexOf<string>(hlTrailStop.EventNames, "BrokenUp");
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
                bool isBull = false;
                float trailStop = lowSerie[0];
                float previousLow = trailStop;
                for (int i = 1; i < stockSerie.Count; i++)
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
                        else if (hlTrailStop.Events[higherLowIndex][i])// Trail Stop when higher low is detected
                        {
                            trailStop = Math.Max(trailStop, previousLow);
                            previousLow = hlTrailStop.Series[0][i];
                        }
                        this.series[0][i] = trailStop;
                        bullEvents[i] = true;
                    }
                    else
                    {
                        if (hlTrailStop.Events[higherLowIndex][i])
                        {
                            isBull = true;
                            brokenUpEvents[i] = true;
                            this.series[0][i] = trailStop;
                            bullEvents[i] = true;
                        }
                        else if (hlTrailStop.Events[brokenUpIndex][i])
                        {
                            previousLow = trailStop = hlTrailStop.Series[0][i];
                        }
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