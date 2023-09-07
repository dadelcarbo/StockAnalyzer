using StockAnalyzer.StockDrawing;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    class StockAutoDrawing_BOXNEW : StockAutoDrawingBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Draws BOX pattern as a consolidation from a new high.";

        public override string[] ParameterNames => new string[] { "HighestStart", "MinLength", "MaxLength", "MaxRange" };

        public override Object[] ParameterDefaultValues => new Object[] { 35, 8, 80, 0.15f };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500), new ParamRangeInt(2, 500), new ParamRangeInt(2, 500), new ParamRangeFloat(0f, 10f) };

        static string[] eventNames = null;
        public override string[] EventNames => eventNames ??= new string[] { "BrokenUp" };

        static readonly bool[] isEvent = new bool[] { true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }

        public override Pen[] SeriePens => seriePens ?? new Pen[] { new Pen(Color.Green) { Width = 1 } };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var highestStart = (int)this.parameters[0];
            var boxMinLength = (int)this.parameters[1];
            var boxMaxLength = (int)this.parameters[2];
            var boxMaxRange = (float)this.parameters[3];

            var highestInSerie = stockSerie.GetIndicator($"HIGHEST({boxMaxLength})").Series[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var brokenUpEvents = this.Events[Array.IndexOf<string>(this.EventNames, "BrokenUp")];

            try
            {
                var bodyHighSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);
                var bodyLowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);
                var highSerie = stockSerie.GetSerie(StockDataType.HIGH);
                var lowSerie = stockSerie.GetSerie(StockDataType.LOW);
                var volumeSerie = stockSerie.GetSerie(StockDataType.VOLUME);
                var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

                bool inBox = false;
                float boxHigh = 0, boxLow = 0;
                int boxStartIndex = 0;

                for (int i = highestStart + 1; i < stockSerie.Count; i++)
                {
                    if (!inBox) // Check box validity and box exit
                    {
                        if (highestInSerie[i - 1] >= highestStart && highestInSerie[i] == 0) // Consolidation starts
                        {
                            inBox = true;
                            boxHigh = bodyHighSerie[i - 1];
                            boxLow = Math.Min(bodyLowSerie[i - 1], bodyLowSerie[i]);
                            boxStartIndex = i - 1;
                        }
                    }
                    else
                    {
                        if (i - boxStartIndex >= boxMaxLength) { inBox = false; continue; } // Box too long

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
                            var range = (boxHigh - boxLow) / boxHigh;
                            if (range > boxMaxRange) { inBox = false; continue; } // Box range too big
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
