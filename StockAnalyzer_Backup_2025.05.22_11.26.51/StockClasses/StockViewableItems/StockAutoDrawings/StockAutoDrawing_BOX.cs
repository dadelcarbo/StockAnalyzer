using StockAnalyzer.StockDrawing;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    class StockAutoDrawing_BOX : StockAutoDrawingBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Draws BOX pattern as defined per FinancialWisdom.";

        public override string[] ParameterNames => new string[] { "Length", "Range" };

        public override Object[] ParameterDefaultValues => new Object[] { 8, 0.15f };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500), new ParamRangeFloat(0f, 10f) };

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                eventNames ??= new string[] { "BrokenUp" };
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true };
        public override bool[] IsEvent => isEvent;

        public override Pen[] SeriePens => seriePens ?? new Pen[] { new Pen(Color.Green) { Width = 1 } };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var boxLength = (int)this.parameters[0];
            var boxRange = (float)this.parameters[1];

            var highestInSerie = stockSerie.GetIndicator($"HIGHEST({boxLength})").Series[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var brokenUpEvents = this.Events[Array.IndexOf(this.EventNames, "BrokenUp")];

            try
            {
                var bodyHighSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);
                var bodyLowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);
                var volumeSerie = stockSerie.GetSerie(StockDataType.VOLUME);
                var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                var emaSerie = closeSerie.CalculateEMA(boxLength);

                for (int i = boxLength + 1; i < stockSerie.Count; i++)
                {
                    if (highestInSerie[i] > boxLength)
                    {
                        int nextIndex = i;
                        bool valid = false;
                        for (int j = 1; j < boxLength; j++)
                        {
                            if (closeSerie[i - j] < emaSerie[i - j])
                            {
                                valid = true;
                                nextIndex = i - j + 1;
                                break;
                            }
                        }
                        if (!valid)
                            continue;
                        var index = i - boxLength;
                        var boxHigh = bodyHighSerie.GetMax(index, i - 1);
                        var boxLow = bodyLowSerie.GetMin(index, i - 1);
                        var range = (boxHigh - boxLow) / boxHigh;
                        if (range > boxRange)
                            continue;
                        index--;
                        while (index > 0 && closeSerie[index] <= boxHigh && closeSerie[index] >= boxLow)
                        {
                            index--;
                        }
                        index++;
                        // Box broken up
                        brokenUpEvents[i] = true;
                        var box = new Box(new PointF(index, boxHigh), new PointF(i, boxLow)) { Pen = this.SeriePens[0], Fill = true };
                        this.DrawingItems.Insert(0, box);
                        i = nextIndex + boxLength;
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
