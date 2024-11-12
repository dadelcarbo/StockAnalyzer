using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_MAX : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override object[] ParameterDefaultValues => new Object[] { "ROR(35)", 35 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeIndicator(), new ParamRangeInt(0, 500) };
        public override string[] ParameterNames => new string[] { "Indicator", "Lookback" };

        public override string[] SerieNames => new string[] { $"MAX({this.Parameters[0]})", $"{this.Parameters[1]}" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.DarkRed), new Pen(Color.Black) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot } };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var indicator = stockSerie.GetIndicator(this.parameters[0].ToString().Replace("_", ","));
            if (!string.IsNullOrEmpty(indicator.SerieFormats?[0]))
            {
                this.serieFormats = new string[] { indicator.SerieFormats[0], indicator.SerieFormats[0] };
            }
            FloatSerie indicatorSerie = indicator.Series[0];

            this.series[0] = indicatorSerie.MaxSerie((int)this.parameters[1]);
            this.series[1] = indicatorSerie;
            this.SetSerieNames();

            CreateEventSeries(stockSerie.Count);
        }

        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;

        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
