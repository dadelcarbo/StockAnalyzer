using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_PRICE : StockPaintBarBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;

        public override string[] ParameterNames
        {
            get { return new string[] { "LowRange", "HighRange" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 10f, 20f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeFloat(0f, 10000f), new ParamRangeFloat(0f, 10000f) }; }
        }

        static string[] eventNames = new string[] {
            "LowerTHan", "GreaterThan", "InRange"
        };

        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] {
            false, false, false};

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
                    seriePens = new Pen[] {
                        new Pen(Color.Red),
                        new Pen(Color.Green),
                        new Pen(Color.Black) };

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
            this.CreateEventSeries(stockSerie.Count);

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var lowRange = (float)parameters[0];
            var highRange = (float)parameters[1];
            for (int i = 0; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = closeSerie[i] <= lowRange;
                this.eventSeries[1][i] = closeSerie[i] >= highRange;
                this.eventSeries[2][i] = closeSerie[i] <= highRange && closeSerie[i] >= lowRange;
            }
        }
    }
}
