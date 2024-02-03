using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_AR : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "FastPeriod", "SlowPeriod", "MAType" };

        public override Object[] ParameterDefaultValues => new Object[] { 21, 200, "EMA" };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeMA() };
        public override string[] SerieNames => new string[] { "AR(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Black) { DashStyle = System.Drawing.Drawing2D.DashStyle.Custom } };
                return seriePens;
            }
        }
        static HLine[] lines = null;
        public override HLine[] HorizontalLines
        {
            get
            {
                lines ??= new HLine[] { new HLine(0, new Pen(Color.LightGray)) };
                return lines;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            var fastSerie = stockSerie.GetIndicator($"{this.parameters[2]}({this.parameters[0]})").Series[0];
            var slowSerie = stockSerie.GetIndicator($"{this.parameters[2]}({this.parameters[1]})").Series[0];

            this.series[0] = fastSerie / slowSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
        }

        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
