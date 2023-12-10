using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_OBVEX : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override bool RequiresVolumeData => true;

        public override object[] ParameterDefaultValues => new Object[] { 20 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };

        public override string[] ParameterNames => new string[] { "Period" };
        public override string[] SerieNames => new string[] { "OBVEX(" + this.Parameters[0].ToString() + ")" };

        static HLine[] lines = null;
        public override HLine[] HorizontalLines
        {
            get
            {
                if (lines == null)
                {
                    lines = new HLine[] { new HLine(0, new Pen(Color.Gray)) };
                    lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                }
                return lines;
            }
        }

        public override System.Drawing.Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.series[0] = stockSerie.CalculateOnBalanceVolumeEx((int)this.parameters[0]);
            this.Series[0].Name = this.Name;
        }

        static string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
