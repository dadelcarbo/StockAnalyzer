using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_OVERBOUGHT : StockPaintBarBase
    {
        public StockPaintBar_OVERBOUGHT()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;

        public override string[] ParameterNames
        {
            get { return new string[] { "Indicator", "Overbought", "Oversold" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { "RSI(3_1)", 80f, 20f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeIndicator(), new ParamRangeFloat(-100f, 100f), new ParamRangeFloat(-100f, 100f) }; }
        }

        static string[] eventNames = new string[] {
            "Overbought", "Oversold"
        };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] {
            false, false};
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
                        new Pen(Color.Green) };

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
            var indicatorSerie = stockSerie.GetIndicator(this.parameters[0].ToString().Replace("_", ",")).Series[0];
            var overbought = (float)parameters[1];
            var oversold = (float)parameters[2];
            for (int i = 0; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = indicatorSerie[i] >= overbought;
                this.eventSeries[1][i] = indicatorSerie[i] <= oversold;
            }
        }
    }
}
