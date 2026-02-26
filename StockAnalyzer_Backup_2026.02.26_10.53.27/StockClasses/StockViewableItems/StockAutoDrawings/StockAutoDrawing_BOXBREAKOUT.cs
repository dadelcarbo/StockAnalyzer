using StockAnalyzer.StockDrawing;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    class StockAutoDrawing_BOXBREAKOUT : StockAutoDrawingBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Draws low volatility BOX pattern in case of breakout of a new high";

        public override string[] ParameterNames => new string[] { "HighestStart", "MinRise", "MinLength", "MaxConsoRatio" };

        public override Object[] ParameterDefaultValues => new Object[] { 35, 0.2f, 4, 0.25f };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500), new ParamRangeFloat(0f, 10f), new ParamRangeInt(2, 500), new ParamRangeFloat(0f, 1f) };

        static string[] eventNames = null;
        public override string[] EventNames => eventNames ??= new string[] { "BrokenUp" };

        static readonly bool[] isEvent = new bool[] { true };
        public override bool[] IsEvent => isEvent;

        public override Pen[] SeriePens => seriePens ?? new Pen[] { new Pen(Color.Green) { Width = 1 } };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var highestStart = (int)this.parameters[0];
            var minRise = (float)this.parameters[1];
            var boxMinLength = (int)this.parameters[2];
            var maxConsoRatio = (float)this.parameters[3];

            var highestInSerie = stockSerie.GetIndicator($"HIGHEST({highestStart})").Series[0];
            var rorSerie = stockSerie.GetIndicator($"ROR({highestStart})").Series[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var brokenUpEvents = this.Events[Array.IndexOf(this.EventNames, "BrokenUp")];

            try
            {
                var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                var bodyHighSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                var bodyLowSerie = stockSerie.GetSerie(StockDataType.CLOSE);

                bool inBox = false;
                float boxHigh = 0, boxLow = 0, ror = 0;
                int boxStartIndex = 0;

                for (int i = highestStart + 1; i < stockSerie.Count; i++)
                {
                    if (!inBox) // Check box validity and box exit
                    {
                        ror = rorSerie[i - 1];
                        if (highestInSerie[i - 1] >= highestStart && closeSerie[i] < closeSerie[i - 1] && ror > minRise) // Consolidation starts
                        {
                            inBox = true;
                            boxHigh = bodyHighSerie[i - 1];
                            boxLow = Math.Min(bodyLowSerie[i - 1], bodyLowSerie[i]);
                            boxStartIndex = i - 1;
                        }
                    }
                    else
                    {
                        if (closeSerie[i] > boxHigh) // Box broken up
                        {
                            inBox = false;
                            if (i - boxStartIndex < boxMinLength) { i--; continue; } // Box too small

                            // Box broken up
                            brokenUpEvents[i] = true;
                            var box = new Box(new PointF(boxStartIndex, boxHigh), new PointF(i, boxLow)) { Pen = this.SeriePens[0], Fill = true };
                            this.DrawingItems.Insert(0, box);
                        }
                        else // Still in box
                        {
                            boxLow = Math.Min(boxLow, bodyLowSerie[i]);
                            var range = (boxHigh - boxLow) / boxHigh / ror;
                            if (range > maxConsoRatio) { inBox = false; continue; } // Box range too big
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
