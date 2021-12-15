using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    class StockAutoDrawing_BOX : StockAutoDrawingBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Draws BOX pattern which start when a high occurs and limited low occurs (detects consolidation).";

        public override string[] ParameterNames => new string[] { "HighPeriod", "LowPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 25, 10 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500), new ParamRangeInt(2, 500) };

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[] { "BrokenUp", "BrokenDown", "InBox" };
                }
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true, true, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }

        public override Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green) { Width = 2 }, new Pen(Color.DarkRed) { Width = 2 }, new Pen(Color.Black) { Width = 2 } };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            var highPeriod = (int)this.parameters[0];
            var lowPeriod = (int)this.parameters[1];
            var validationPeriod = lowPeriod;
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);

            var highestInSerie = stockSerie.GetIndicator($"HIGHEST({highPeriod})").Series[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var brokenUpEvents = this.Events[Array.IndexOf<string>(this.EventNames, "BrokenUp")];
            var brokenDownEvents = this.Events[Array.IndexOf<string>(this.EventNames, "BrokenDown")];
            var inBoxEvents = this.Events[Array.IndexOf<string>(this.EventNames, "InBox")];

            try
            {
                var bodyHighSerie = new FloatSerie(stockSerie.Values.Select(v => v.BodyHigh).ToArray());
                var bodyLowSerie = new FloatSerie(stockSerie.Values.Select(v => v.BodyLow).ToArray());

                bool inBox = false;
                float boxHigh = 0f, boxLow = 0f, boxLowLimit = 0f;
                PointF boxStartCorner = PointF.Empty;

                for (int i = highPeriod; i < stockSerie.Count; i++)
                {
                    if (!inBox)
                    {
                        int startIndex = i - validationPeriod;
                        if (highestInSerie[startIndex] > highPeriod && highestInSerie.FindMaxIndex(startIndex, i) == startIndex) // Begining of consolidation
                        {
                            // Initiate new box
                            boxLow = bodyLowSerie.GetMin(startIndex, i);
                            boxLowLimit = bodyLowSerie.GetMin(startIndex - lowPeriod - 1, startIndex);
                            if (boxLow < boxLowLimit)
                                continue;
                            inBox = true;
                            boxHigh = Math.Max(bodyHighSerie[startIndex], bodyHighSerie[startIndex + 1]);
                            boxStartCorner = new PointF(startIndex, boxHigh);
                        }
                        else { continue; }
                    }
                    else
                    {
                        if (closeSerie[i] > boxHigh)
                        {
                            // Box broken up
                            brokenUpEvents[i] = true;
                            inBox = false;
                            Rectangle2D box = new Rectangle2D(boxStartCorner, new PointF(i, boxLow)) { Pen = this.SeriePens[0], Fill = true };
                            this.DrawingItems.Insert(0, box);
                        }
                        else
                        {
                            // Wait for box break out.
                            if (bodyLowSerie[i] > boxLowLimit)
                            {
                                boxLow = Math.Min(boxLow, bodyLowSerie[i]);
                            }
                            else
                            {
                                brokenDownEvents[i] = true;
                                inBox = false;
                                Rectangle2D box = new Rectangle2D(boxStartCorner, new PointF(i, boxLow)) { Pen = this.SeriePens[1], Fill = true };
                                this.DrawingItems.Insert(0, box);
                            }
                        }
                    }
                    if (inBox)
                    {
                        inBoxEvents[i] = true;
                    }
                }
                if (inBox)
                {
                    Rectangle2D box = new Rectangle2D(boxStartCorner, new PointF(stockSerie.LastIndex, boxLowLimit)) { Pen = Pens.Gray, Fill = true };
                    this.DrawingItems.Insert(0, box);
                }
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }
    }
}
