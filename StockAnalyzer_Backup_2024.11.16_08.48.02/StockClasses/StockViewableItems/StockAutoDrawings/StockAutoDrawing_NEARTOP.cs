using StockAnalyzer.StockDrawing;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    class StockAutoDrawing_NEARTOP : StockAutoDrawingBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Draws a resistance line when price is getting close to highest";

        public override string[] ParameterNames => new string[] { "MaxPeriod", "MinPeriod", "NbATR" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 7, 0.5f };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500), new ParamRangeInt(2, 500), new ParamRangeFloat(0f, 10f) };

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                eventNames ??= new string[] { "NearTop" };
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true };
        public override bool[] IsEvent => isEvent;

        public override Pen[] SeriePens => seriePens ?? new Pen[] { new Pen(Color.DarkGreen) { Width = 2 } };

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);
            var maxPeriod = (int)this.parameters[0];
            var minPeriod = (int)this.parameters[1];
            if (maxPeriod > stockSerie.LastIndex - 1)
                return;

            try
            {
                var bodyHighSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);
                var topPoint = bodyHighSerie.GetMaxPoint(stockSerie.LastIndex - maxPeriod, stockSerie.LastIndex);
                while (topPoint.X == stockSerie.LastIndex - maxPeriod && !bodyHighSerie.IsTop(stockSerie.LastIndex - maxPeriod) && topPoint.X <= stockSerie.LastIndex - minPeriod)
                {
                    maxPeriod--;
                    topPoint = bodyHighSerie.GetMaxPoint(stockSerie.LastIndex - maxPeriod, stockSerie.LastIndex);
                }
                if (topPoint.X > stockSerie.LastIndex - minPeriod) // Top is between min and max period
                    return;

                var nbAtr = (float)this.parameters[2];
                var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

                var close = closeSerie[stockSerie.LastIndex];

                var atrSerie = stockSerie.GetIndicator($"ATR(9)").Series[0];
                if (close + nbAtr * atrSerie[stockSerie.LastIndex] > topPoint.Y)
                {
                    var resistance = new HalfLine2D(topPoint, 1, 0) { Pen = this.SeriePens[0] };
                    this.DrawingItems.Insert(0, resistance);

                    // Detecting events
                    var brokenUpEvents = this.Events[Array.IndexOf<string>(this.EventNames, "NearTop")];
                    brokenUpEvents[stockSerie.LastIndex] = true;
                }
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }
    }
}
