using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_STOKSTREND : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "FastKPeriod", "SlowKPeriod", "SlowDPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 14, 3, 3 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "SLOWSTREND()" };


        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Black) { DashStyle = System.Drawing.Drawing2D.DashStyle.Custom } };
                return seriePens;
            }
        }
        public override HLine[] HorizontalLines
        {
            get
            {
                HLine[] lines = new HLine[] { new HLine(0, new Pen(Color.DarkGray)) };
                return lines;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie slowK = stockSerie.CalculateFastOscillator((int)this.parameters[0]).CalculateEMA((int)this.parameters[1]);
            FloatSerie slowD = slowK.CalculateEMA((int)this.parameters[2]);
            var trendSerie = slowK - slowD;
            this.series[0] = trendSerie;
            this.series[0].Name = this.SerieNames[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 1; i < slowK.Count; i++)
            {
                this.eventSeries[0][i] = trendSerie[i - 1] < 0 && trendSerie[i] >= 0;
                this.eventSeries[1][i] = trendSerie[i - 1] > 0 && trendSerie[i] <= 0;
                this.eventSeries[2][i] = trendSerie[i] >= 0;
                this.eventSeries[3][i] = trendSerie[i] <= 0;
            }
        }

        static string[] eventNames = new string[] { "BullishCrossing", "BearishCrossing", "Bullish", "Bearish" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
