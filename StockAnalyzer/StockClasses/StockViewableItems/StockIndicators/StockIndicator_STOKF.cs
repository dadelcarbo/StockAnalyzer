using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_STOKF : StockIndicatorBase, IRange
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.RangedIndicator;
        public float Max => 100.0f;
        public float Min => 0.0f;
        public override string[] ParameterNames => new string[] { "FastKPeriod", "SmoothPeriod", "Overbought", "Oversold" };
        public override Object[] ParameterDefaultValues => new Object[] { 14, 3, 75f, 25f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 100f), new ParamRangeFloat(0f, 100f) };

        public override string[] SerieNames => new string[] { "FastK(" + this.Parameters[0].ToString() + ")", "FastD(" + this.Parameters[1].ToString() + ")" };


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
            FloatSerie fastK = stockSerie.CalculateFastOscillator((int)this.parameters[0], IndicatorType.HighLow);
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
                bool isOverSold = fastK[i] <= oversold;
                bool isOverBought = fastK[i] >= overbought;
                this.eventSeries[2][i] = isOverBought;
                this.eventSeries[3][i] = isOverSold;
                this.eventSeries[4][i] = (!isOverSold) && this.eventSeries[3][i - 1];
                this.eventSeries[5][i] = (!isOverBought) && this.eventSeries[2][i - 1];
                this.eventSeries[6][i] = fastK[i] > fastD[i];
                this.eventSeries[7][i] = fastK[i] < fastD[i];
            }
        }

        static readonly string[] eventNames = new string[] { "BullishCrossing", "BearishCrossing", "Overbought", "Oversold", "OutOfOversold", "OutOfOverbought", "Bullish", "Bearish" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, false, false, true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
