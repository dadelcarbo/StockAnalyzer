using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_TRENDLINE : StockPaintBarBase
    {
        public StockPaintBar_TRENDLINE()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override bool HasTrendLine => true;

        public override string Definition => "TRENDLINE(int leftStrength, int rightStrength, int nbPivots)";
        public override string[] ParameterNames => new string[] { "LeftStrenth", "RightStrength", "NbPivots" };

        public override Object[] ParameterDefaultValues => new Object[] { 3, 3, 10 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 10), new ParamRangeInt(1, 10), new ParamRangeInt(2, 50) };

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                eventNames ??= Enum.GetNames(typeof(StockSerie.TLEvent));
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true, true, true, true, false, false };
        public override bool[] IsEvent => isEvent;

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Green), new Pen(Color.Red) };
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

            stockSerie.generateAutomaticTrendLines(0, stockSerie.Count - 1,
                (int)this.parameters[0],
                (int)this.parameters[1],
                (int)this.parameters[2],
                ref this.eventSeries);
        }
    }
}
