using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_SPEED : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override object[] ParameterDefaultValues => new Object[] { "EMA(20)", 1 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeIndicator(), new ParamRangeInt(1, 500) };
        public override string[] ParameterNames => new string[] { "Indicator", "Period" };

        public override string[] SerieNames => new string[] { "SPEED(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" };

        public override Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black) };
                    seriePens[0].DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                }
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
            FloatSerie indicatorSerie = stockSerie.GetIndicator(this.parameters[0].ToString().Replace("_", ",")).Series[0];
            var period = (int)this.parameters[1];
            FloatSerie speedSerie = new FloatSerie(stockSerie.Count);
            for (int i = period; i < stockSerie.Count; i++)
            {
                var diff = (indicatorSerie[i] - indicatorSerie[i - period]);
                speedSerie[i] = 100.0f * (diff > 0 ? diff / indicatorSerie[i] : diff / indicatorSerie[i - period]);
            }

            this.series[0] = speedSerie;
            this.Series[0].Name = this.Name;
            CreateEventSeries(stockSerie.Count);

            this.Events[2] = new BoolSerie(this.EventNames[2], speedSerie.Values.Select(a => a >= 0));
            this.Events[3] = new BoolSerie(this.EventNames[3], speedSerie.Values.Select(a => a < 0));
            for (int i = 1; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = this.Events[2][i] && this.Events[3][i - 1];
                this.eventSeries[1][i] = this.Events[3][i] && this.Events[2][i - 1];
            }
        }

        static readonly string[] eventNames = new string[] { "BullishCrossing", "BearishCrossing", "Positive", "Negative" };
        public override string[] EventNames => eventNames;

        static readonly bool[] isEvent = new bool[] { true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
