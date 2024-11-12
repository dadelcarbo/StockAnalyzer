using StockAnalyzer.StockDrawing;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    class StockAutoDrawing_BOXEMABREAKOUT : StockAutoDrawingBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Draws low volatility BOX pattern in case of breakout of a new high while price is above EMA";

        public override string[] ParameterNames => new string[] { "EMAPeriod", "MinRise", "MinLength", "MaxConsoRatio" };

        public override Object[] ParameterDefaultValues => new Object[] { 35, 0.2f, 4, 0.25f };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500), new ParamRangeFloat(0f, 10f), new ParamRangeInt(2, 500), new ParamRangeFloat(0f, 1f) };

        static string[] eventNames = null;
        public override string[] EventNames => eventNames ??= new string[] { "BrokenUp", "InBox" };

        static readonly bool[] isEvent = new bool[] { true, false };
        public override bool[] IsEvent => isEvent;

        public override Pen[] SeriePens => seriePens ?? new Pen[] { new Pen(Color.Green) { Width = 1 }, new Pen(Color.DarkRed) { Width = 1 } };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var emaPeriod = (int)this.parameters[0];
            var minRise = (float)this.parameters[1];
            var boxMinLength = (int)this.parameters[2];
            var maxConsoRatio = (float)this.parameters[3];

            var emaSerie = stockSerie.GetIndicator($"EMA({emaPeriod})").Series[0];
            var rorSerie = stockSerie.GetIndicator($"ROR({emaPeriod})").Series[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var brokenUpEvents = this.GetEvents("BrokenUp");
            var inBoxEvents = this.GetEvents("InBox");

            try
            {
                var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                var bodyHighSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                var bodyLowSerie = stockSerie.GetSerie(StockDataType.CLOSE);

                bool inBox = false;
                float boxHigh = 0, boxLow = 0, ror = 0;
                int boxStartIndex = 0;

                for (int i = emaPeriod + 1; i < stockSerie.Count; i++)
                {
                    if (!inBox) // Check box validity and box exit
                    {
                        if (closeSerie[i] < closeSerie[i - 1] && closeSerie[i] >= emaSerie[i]) // Consolidation starts
                        {
                            ror = rorSerie[i];
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
                            if (range > maxConsoRatio)
                            {
                                inBox = false;
                                continue;
                            } // Box range too big
                        }
                    }
                }
                if (inBox && closeSerie[stockSerie.LastIndex] > boxLow)
                {
                    var box = new Box(new PointF(boxStartIndex, boxHigh), new PointF(stockSerie.LastIndex, boxLow)) { Pen = this.SeriePens[0], Fill = true };
                    this.DrawingItems.Insert(0, box);
                    inBoxEvents[stockSerie.LastIndex] = true;
                }
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }
    }
}
