using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_NBBARS : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override string Name => $"NBBARS({this.Parameters[0]})";

        public override string Definition => "Calculate the number of days above or below a moving average";
        public override object[] ParameterDefaultValues => new Object[] { 35, 10 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };
        public override string[] ParameterNames => new string[] { "Period", "Limit" };
        public override string[] SerieNames => new string[] { "NBBARS(" + this.Parameters[0].ToString() + ")" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Blue) };
                return seriePens;
            }
        }
        static HLine[] lines = null;
        public override HLine[] HorizontalLines
        {
            get
            {
                if (lines == null)
                {
                    lines = new HLine[] { new HLine(0.0f, new Pen(Color.Gray)) };
                    lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                }
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];
            FloatSerie nbUpDaysSerie = new FloatSerie(stockSerie.Count, "NBUPDAYS");
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie emaSerie = stockSerie.GetIndicator($"EMA({period})").Series[0];

            float nbDays = 0;
            for (int i = 0; i < closeSerie.Count; i++)
            {
                if (closeSerie[i] > emaSerie[i])
                {
                    nbDays++;
                }
                else
                {
                    nbDays = 0;
                }
                nbUpDaysSerie[i] = nbDays;
            }
            this.Series[0] = nbUpDaysSerie;
        }

        static string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
