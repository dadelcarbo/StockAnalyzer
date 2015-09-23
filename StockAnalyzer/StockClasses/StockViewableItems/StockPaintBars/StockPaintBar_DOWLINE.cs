using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_DOWLINE: StockPaintBarBase
    {
        public StockPaintBar_DOWLINE()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }

        public override bool HasTrendLine { get { return true; } }

        public override string Definition
        {
            get { return "DOWLINE(int hlPeriod)"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "hlPeriod"}; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 3 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1,100)}; }
        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = Enum.GetNames(typeof(StockSerie.DowEvent));
                }
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { false, false, false, true, true, true};
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }

        public override System.Drawing.Pen[] SeriePens
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

            stockSerie.generateDowTrendLines(0, stockSerie.Count - 1,
                (int)this.parameters[0],
                ref this.eventSeries);
        }
    }
}
