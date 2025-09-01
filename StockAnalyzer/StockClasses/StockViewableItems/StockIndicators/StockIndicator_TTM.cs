using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TTM : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override object[] ParameterDefaultValues => new Object[] { 35, 1f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 10f) };
        public override string[] ParameterNames => new string[] { "Period", "Ratio" };

        public override string[] SerieNames => new string[] { $"TTM({this.Parameters[0]},{this.Parameters[1]})" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black) { DashStyle = System.Drawing.Drawing2D.DashStyle.Custom } };

        static HLine[] lines = null;
        public override HLine[] HorizontalLines => lines ??= new HLine[] { new HLine(0, new Pen(Color.LightGray)) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.Parameters[0];
            var ratio = (float)this.Parameters[1];
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var emaSerie = closeSerie.CalculateEMA(period);
            var stdDev = closeSerie.CalculateStdev(period);

            var atrSerie = stockSerie.GetSerie(StockDataType.ATR).CalculateEMA(period);

            this.series[0] = (stdDev - atrSerie * ratio) / emaSerie;
            this.Series[0].Name = this.Name;
        }

        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
