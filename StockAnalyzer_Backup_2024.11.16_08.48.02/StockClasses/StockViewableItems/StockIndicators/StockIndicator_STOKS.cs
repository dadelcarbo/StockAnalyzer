using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_STOKS : StockIndicatorBase, IRange
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.RangedIndicator;
        public float Max => 100.0f;

        public float Min => 0.0f;

        public override string[] ParameterNames => new string[] { "FastKPeriod", "SlowKPeriod", "SlowDPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 14, 3, 3 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "SlowK(" + this.Parameters[0].ToString() + ")", "SlowD(" + this.Parameters[1].ToString() + ")" };


        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Green), new Pen(Color.Red) };
                return seriePens;
            }
        }
        HLine[] lines;
        public override HLine[] HorizontalLines => lines ??= new HLine[] {
            new HLine(50, new Pen(Color.LightGray) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }),
            new HLine(25f, new Pen(Color.Gray) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }),
            new HLine(75f, new Pen(Color.Gray) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }),
        };
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie slowK = stockSerie.CalculateFastOscillator((int)this.parameters[0]).CalculateEMA((int)this.parameters[1]);
            FloatSerie slowD = slowK.CalculateEMA((int)this.parameters[2]);
            this.series[0] = slowK;
            this.series[0].Name = this.SerieNames[0];
            this.series[1] = slowD;
            this.series[1].Name = this.SerieNames[1];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 1; i < slowK.Count; i++)
            {
                this.eventSeries[0][i] = (slowD[i - 1] > slowK[i - 1] && slowD[i] < slowK[i]);
                this.eventSeries[1][i] = (slowD[i - 1] < slowK[i - 1] && slowD[i] > slowK[i]);
                this.eventSeries[2][i] = slowK[i] > slowD[i];
                this.eventSeries[3][i] = slowK[i] < slowD[i];
            }
        }

        static readonly string[] eventNames = new string[] { "BullishCrossing", "BearishCrossing", "Bullish", "Bearish" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
