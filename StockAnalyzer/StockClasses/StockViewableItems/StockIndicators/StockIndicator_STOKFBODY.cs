using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_STOKFBODY : StockIndicatorBase, IRange
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.RangedIndicator;
        public float Max => 105.0f;

        public float Min => -5.0f;

        public override string[] ParameterNames => new string[] { "FastKPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 14 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "FastK(" + this.Parameters[0].ToString() + ")" };


        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Green) };
                return seriePens;
            }
        }

        public override HLine[] HorizontalLines => lines ??= new HLine[] {
            new HLine(50, new Pen(Color.LightGray) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }),
            new HLine(25f, new Pen(Color.Gray) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }),
            new HLine(75f, new Pen(Color.Gray) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }),
        };
        public override void ApplyTo(StockSerie stockSerie)
        {
            //  %K = 100*(Close - lowest(14))/(highest(14)-lowest(14))
            //  %D = MA3(%K)
            int period = (int)this.parameters[0];
            FloatSerie fastK = stockSerie.CalculateFastOscillator((int)this.parameters[0], InputType.Body);
            this.series[0] = fastK;
            this.series[0].Name = this.SerieNames[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = period + 1; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = (fastK[i - 1] < 50f && fastK[i] > 50f);
                this.eventSeries[1][i] = (fastK[i - 1] > 50f && fastK[i] < 50f);
                this.eventSeries[2][i] = fastK[i] >= 50f;
                this.eventSeries[3][i] = fastK[i] < 50f;
            }
        }

        static readonly string[] eventNames = new string[] { "BullishCrossing", "BearishCrossing", "Bullish", "Bearish" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
