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

        public override string Definition => "Draws BOX pattern as defined per FinancialWisdom.";

        public override string[] ParameterNames => new string[] { "Length", "Range" };

        public override Object[] ParameterDefaultValues => new Object[] { 8, 0.1f };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500), new ParamRangeFloat(0f, 10f) };

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[] { "BrokenUp" };
                }
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true };
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
                    seriePens = new Pen[] { new Pen(Color.Green) { Width = 2 } };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            var boxLength = (int)this.parameters[0];
            var boxRange = (float)this.parameters[1];
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var highestInSerie = stockSerie.GetIndicator($"HIGHEST({boxLength})").Series[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var brokenUpEvents = this.Events[Array.IndexOf<string>(this.EventNames, "BrokenUp")];

            try
            {
                var bodyHighSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);
                var bodyLowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);
                var volumeSerie = stockSerie.GetSerie(StockDataType.BODYLOW);

                for (int i = boxLength + 1; i < stockSerie.Count; i++)
                {
                    if (highestInSerie[i] > boxLength)
                    {
                        // Check Box Size
                        var boxHigh = bodyHighSerie.GetMax(i - boxLength - 1, i - 1);
                        var boxLow = bodyLowSerie.GetMin(i - boxLength - 1, i - 1);
                        var range = (boxHigh - boxLow) / boxHigh;
                        if (range < boxRange && (volumeSerie[i] == 0 || volumeSerie[i] > volumeSerie[i - 1]))
                        {
                            // Box broken up
                            brokenUpEvents[i] = true;
                            Rectangle2D box = new Rectangle2D(new PointF(i - boxLength - 1, boxHigh), new PointF(i, boxLow)) { Pen = this.SeriePens[0], Fill = true };
                            this.DrawingItems.Insert(0, box);
                        }
                        i += boxLength;
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
