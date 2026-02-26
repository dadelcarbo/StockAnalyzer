using StockAnalyzer.StockDrawing;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    public class StockAutoDrawing_PIVOT : StockAutoDrawingBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Draws pivot point being extremum over period";

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 20 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500) };

        public override string[] SerieNames => new string[] { };
        public override Pen[] SeriePens => seriePens ??= new Pen[] { };

        static readonly Pen supportPen = new Pen(Brushes.DarkRed, 3);
        static readonly Pen resistancePen = new Pen(Brushes.DarkGreen, 3);

        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.parameters[0];

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = period; i < stockSerie.Count - period; i++)
            {
                var isNewLow = true;
                var isNewHigh = true;
                for (int j = i - period; j < i; j++)
                {
                    isNewLow = closeSerie.IsBottom(i, period);
                    if (isNewLow)
                    {
                        var pivot = new PointF(i, closeSerie[i]);
                        this.DrawingItems.Add(new Bullet2D(pivot, 3, Brushes.Red));
                        this.DrawingItems.Add(new Segment2D(pivot, new PointF(i + period, closeSerie[i]), supportPen));
                        this.eventSeries[0][i] = isNewLow = closeSerie.IsBottom(i, period);
                    }
                    else
                    {
                        isNewHigh = closeSerie.IsTop(i, period);
                        if (isNewHigh)
                        {
                            var pivot = new PointF(i, closeSerie[i]);
                            this.DrawingItems.Add(new Bullet2D(pivot, 3, Brushes.Green));
                            this.DrawingItems.Add(new Segment2D(pivot, new PointF(i + period, closeSerie[i]), resistancePen));
                            this.eventSeries[1][i] = closeSerie.IsTop(i, period);
                        }
                    }
                }
            }


        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                eventNames ??= new string[] { "NewLow", "NewHigh" };
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true, true };
        public override bool[] IsEvent => isEvent;
    }
}