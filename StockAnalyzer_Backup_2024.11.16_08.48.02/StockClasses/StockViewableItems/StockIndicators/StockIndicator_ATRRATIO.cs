using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ATRRATIO : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "Period1", "Period2" };

        public override Object[] ParameterDefaultValues => new Object[] { 6, 18 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };
        public override string[] SerieNames => new string[] { "ATRRATIO(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" };


        public override System.Drawing.Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black) };

        static HLine[] lines = null;
        public override HLine[] HorizontalLines => lines ??= new HLine[] { new HLine(0, new Pen(Color.LightGray)) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie fastAtrSerie = stockSerie.GetIndicator("ATR(" + this.parameters[0] + ")").Series[0];
            FloatSerie slowAtrSerie = stockSerie.GetIndicator("ATR(" + this.parameters[1] + ")").Series[0];

            FloatSerie atrRatio = slowAtrSerie.Div(fastAtrSerie);

            this.series[0] = atrRatio;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 2; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = (atrRatio[i - 2] < atrRatio[i - 1] && atrRatio[i - 1] > atrRatio[i]);
                this.eventSeries[1][i] = (atrRatio[i - 2] > atrRatio[i - 1] && atrRatio[i - 1] < atrRatio[i]);
                this.eventSeries[2][i] = (atrRatio[i - 1] < 0 && atrRatio[i] >= 0);
                this.eventSeries[3][i] = (atrRatio[i - 1] > 0 && atrRatio[i] <= 0);
                this.eventSeries[4][i] = atrRatio[i] >= 0;
                this.eventSeries[5][i] = atrRatio[i] < 0;
            }
        }

        static readonly string[] eventNames = new string[] { "Top", "Bottom", "TurnedPositive", "TurnedNegative", "Bullish", "Bearish" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
