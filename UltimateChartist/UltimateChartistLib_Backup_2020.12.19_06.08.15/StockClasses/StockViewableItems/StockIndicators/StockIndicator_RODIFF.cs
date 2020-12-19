using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_RODIFF : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "Smoothing" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 100, 1 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames
        {
            get
            {
                return new string[]
                    {
                    "RODIFF(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")"
                    };
            }
        }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black) };
                }
                return seriePens;
            }
        }

        static HLine[] lines = null;

        public override HLine[] HorizontalLines
        {
            get
            {
                if (lines == null)
                {
                    lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)) };
                }
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            var rorSerie = (stockSerie.CalculateRateOfRise((int)this.parameters[0])).CalculateEMA((int)this.parameters[1]) * 100f;
            var rodSerie = (stockSerie.CalculateRateOfDecline((int)this.parameters[0])).CalculateEMA((int)this.parameters[1]) * -100f;

            var diff = rorSerie - rodSerie;

            this.series[0] = diff;
            this.Series[0].Name = this.SerieNames[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 2; i < stockSerie.Count; i++)
            {
                var bullish = diff[i] >= 0;
                this.eventSeries[0][i] = bullish;
                this.eventSeries[1][i] = !bullish;
                this.eventSeries[2][i] = diff[i - 1] < 0 && bullish;
                this.eventSeries[3][i] = diff[i-1] >= 0 && !bullish;
            }
        }
        static string[] eventNames = new string[] { "Bullish", "Bearish", "BullSignal", "BearSignal" };
        public override string[] EventNames => eventNames;

        static readonly bool[] isEvent = new bool[] { false, false, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
