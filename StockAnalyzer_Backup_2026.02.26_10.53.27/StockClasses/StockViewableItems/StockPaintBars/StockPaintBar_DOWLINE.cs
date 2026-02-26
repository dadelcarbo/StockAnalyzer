using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_DOWLINE : StockPaintBarBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override bool HasTrendLine => true;

        public override string Definition => "DOWLINE(int hlPeriod)";
        public override string[] ParameterNames => new string[] { "hlPeriod" };
        public override Object[] ParameterDefaultValues => new Object[] { 3 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 100) };

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                eventNames ??= Enum.GetNames(typeof(StockSerie.DowEvent));
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { false, false, false, true, true, true };
        public override bool[] IsEvent => isEvent;

        public override Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Black) };
                    foreach (Pen pen in seriePens)
                    {
                        pen.Width = 2;
                    }
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            stockSerie.generateDowTrendLines(0, stockSerie.Count - 1, (int)this.parameters[0], ref this.eventSeries);
        }
    }
}
