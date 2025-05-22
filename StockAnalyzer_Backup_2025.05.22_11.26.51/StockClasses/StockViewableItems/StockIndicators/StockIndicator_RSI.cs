using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_RSI : StockIndicatorBase, IRange
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.RangedIndicator;
        public override string[] ParameterNames => new string[] { "Period", "InputSmoothing" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 1 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "RSI(" + this.Parameters[0].ToString() + ")" };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Black) };
                return seriePens;
            }
        }
        public override HLine[] HorizontalLines
        {
            get
            {
                HLine[] lines = new HLine[] { new HLine(50, new Pen(Color.LightGray)), new HLine(70f, new Pen(Color.Gray)), new HLine(30f, new Pen(Color.Gray)) };
                lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[1].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[2].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE).CalculateEMA((int)this.parameters[1]);
            FloatSerie rsiSerie;
            if (closeSerie.Any(i => i <= 0.0f))
            {
                rsiSerie = closeSerie.CalculateRSI((int)this.parameters[0], false);
            }
            else
            {
                rsiSerie = closeSerie.CalculateRSI((int)this.parameters[0], true);
            }
            this.series[0] = rsiSerie;
            this.series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 2; i < rsiSerie.Count; i++)
            {
                this.eventSeries[0][i] = rsiSerie[i - 2] < rsiSerie[i - 1] && rsiSerie[i - 1] > rsiSerie[i];
                this.eventSeries[1][i] = rsiSerie[i - 2] > rsiSerie[i - 1] && rsiSerie[i - 1] < rsiSerie[i];
            }
        }

        public float Max => 100.0f;

        public float Min => 0.0f;

        static readonly string[] eventNames = new string[] { "Top", "Bottom" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true };
        public override bool[] IsEvent => isEvent;
    }
}
