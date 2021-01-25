using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_STOKFBODY : StockIndicatorBase, IRange
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
            get { return new string[] { "FastKPeriod" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 14 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { "FastK(" + this.Parameters[0].ToString() + ")" }; } }


        public override Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green) };
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
            //  %K = 100*(Close - lowest(14))/(highest(14)-lowest(14))
            //  %D = MA3(%K)
            int period = (int)this.parameters[0];
            FloatSerie fastK = stockSerie.CalculateFastBodyOscillator(period);
            this.series[0] = fastK;
            this.series[0].Name = this.SerieNames[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = period + 1; i < fastK.Count; i++)
            {
                this.eventSeries[0][i] = (fastK[i - 1] < 50f && fastK[i] > 50f);
                this.eventSeries[1][i] = (fastK[i - 1] > 50f && fastK[i] < 50f);
                this.eventSeries[2][i] = fastK[i] >= 50f;
                this.eventSeries[3][i] = fastK[i] < 50f;
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
