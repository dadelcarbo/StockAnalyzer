using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_STOKSBODY : StockIndicatorBase, IRange
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.RangedIndicator; }
        }
        public float Max
        {
            get { return 100.0f; }
        }

        public float Min
        {
            get { return 0.0f; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "FastKPeriod", "SlowKPeriod", "SlowDPeriod" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 14, 3, 3 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { "SlowK(" + this.Parameters[0].ToString() + ")", "SlowD(" + this.Parameters[1].ToString() + ")" }; } }


        public override Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red) };
                }
                return seriePens;
            }
        }
        public override HLine[] HorizontalLines
        {
            get
            {
                HLine[] lines = new HLine[] { new HLine(50, new Pen(Color.LightGray)), new HLine(75f, new Pen(Color.Gray)), new HLine(25f, new Pen(Color.Gray)) };
                lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[1].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[2].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                return lines;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie slowK = stockSerie.CalculateFastBodyOscillator((int)this.parameters[0]).CalculateEMA((int)this.parameters[1]);
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

        static string[] eventNames = new string[] { "BullishCrossing", "BearishCrossing", "Bullish", "Bearish" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
