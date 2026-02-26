using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_SHARPE : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override string[] ParameterNames => new string[] { "Period", "Risk Free Rate" };

        public override Object[] ParameterDefaultValues => new Object[] { 35, 0.0f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0.0f, 10.0f) };

        public override string[] SerieNames => new string[] { "SHARPE" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Green, 2) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];
            float riskFreeRate = (float)this.parameters[1];

            this.Series[0] = stockSerie.GetSerie(StockDataType.CLOSE).CalculateSharpeRatio(period, riskFreeRate);
            this.Series[0].Name = "SHARPE";
        }

        static readonly string[] eventNames = new string[] { "UpTrend", "DownTrend", "BrokenUp", "BrokenDown", "UpTrend", "DownTrend", "BrokenUp", "BrokenDown" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { false, false, true, true, false, false, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
