using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_NSTDEV : StockIndicatorBase
    {
        public StockIndicator_NSTDEV()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override string Name => "NSTDEV(" + this.Parameters[0].ToString() + ")";

        public override string Definition => "NSTDEV(int Period)";
        public override object[] ParameterDefaultValues => new Object[] { 20 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };
        public override string[] ParameterNames => new string[] { "Period" };

        public override string[] SerieNames => new string[] { "NSTDEV(" + this.Parameters[0].ToString() + ")" };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Blue) };
                return seriePens;
            }
        }
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

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];
            FloatSerie stdevSerie = stockSerie.GetSerie(StockDataType.CLOSE).CalculateStdev(period);
            FloatSerie emaSerie = stockSerie.GetIndicator("EMA(" + period + ")").Series[0];
            FloatSerie stdevCount = new FloatSerie(emaSerie.Count, this.SerieNames[0]);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            for (int i = period; i < emaSerie.Count; i++)
            {
                if (stdevSerie[i] >= -0.00001f && stdevSerie[i] <= 0.00001f)
                {
                    stdevCount[i] = 0;
                }
                else
                {
                    stdevCount[i] = (closeSerie[i] - emaSerie[i]) / stdevSerie[i];
                }
            }
            this.Series[0] = stdevCount;
        }

        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
