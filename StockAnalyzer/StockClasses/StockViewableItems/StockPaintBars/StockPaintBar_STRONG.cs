using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_STRONG : StockPaintBarBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;

        public override string[] ParameterNames
        {
            get { return new string[] { "Smoothing" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 1 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 100) }; }
        }

        static string[] eventNames = new string[] {
            "Bullish", "Bearish", "InRange"
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
                        new Pen(Color.Green),
                        new Pen(Color.Red),
                        new Pen(Color.White) };

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
            var smoothingPeriod = (int)this.Parameters[0];
            var highSerie = smoothingPeriod <= 1 ? stockSerie.GetSerie(StockDataType.HIGH) : stockSerie.GetSerie(StockDataType.HIGH).CalculateEMA(smoothingPeriod);
            var lowSerie = smoothingPeriod <= 1 ? stockSerie.GetSerie(StockDataType.LOW) : stockSerie.GetSerie(StockDataType.LOW).CalculateEMA(smoothingPeriod);

            for (int i = 1; i < stockSerie.Count; i++)
            {
                bool bullish = lowSerie[i - 1] < lowSerie[i] && highSerie[i - 1] < highSerie[i];
                bool bearish = lowSerie[i - 1] > lowSerie[i] && highSerie[i - 1] > highSerie[i];
                this.eventSeries[0][i] = bullish;
                this.eventSeries[1][i] = bearish;
                this.eventSeries[2][i] = !(bullish || bearish);
            }
        }
    }
}
