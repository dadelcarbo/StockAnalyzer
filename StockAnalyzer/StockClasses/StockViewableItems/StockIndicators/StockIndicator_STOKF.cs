using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_STOKF : StockIndicatorBase, IRange
    {
        public StockIndicator_STOKF()
        {
        }
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

        public override string Name
        {
            get { return "STOKF(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")"; }
        }
        public override string Definition
        {
            get { return "STOKF(int FastKPeriod, int FastDPeriod, float Overbought, float Oversold)"; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "FastKPeriod", "FastKPeriod", "Overbought", "Oversold" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 14, 3, 75f, 25f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 100f), new ParamRangeFloat(0f, 100f) }; }
        }

        public override string[] SerieNames { get { return new string[] { "FastK(" + this.Parameters[0].ToString() + ")", "FastD(" + this.Parameters[1].ToString() + ")" }; } }


        public override System.Drawing.Pen[] SeriePens
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
                HLine[] lines = new HLine[] { new HLine(50, new Pen(Color.LightGray)), new HLine(80, new Pen(Color.Gray)), new HLine(20, new Pen(Color.Gray)) };
                lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[1].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[2].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                return lines;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie fastK = stockSerie.CalculateFastOscillator((int)this.parameters[0]);
            FloatSerie fastD = fastK.CalculateMA((int)this.parameters[1]);
            this.series[0] = fastK;
            this.series[1] = fastD;
            this.series[1].Name = this.SerieNames[1];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            float overbought = (float)this.parameters[2];
            float oversold = (float)this.parameters[3];

            for (int i = 1; i < fastK.Count; i++)
            {
                this.eventSeries[0][i] = (fastD[i - 1] > fastK[i - 1] && fastD[i] < fastK[i]);
                this.eventSeries[1][i] = (fastD[i - 1] < fastK[i - 1] && fastD[i] > fastK[i]);
                this.eventSeries[2][i] = fastK[i] >= overbought;
                this.eventSeries[3][i] = fastK[i] <= oversold;
            }
        }

        static string[] eventNames = new string[] { "BullishCrossing", "BearishCrossing", "Overbought", "Oversold" };
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
