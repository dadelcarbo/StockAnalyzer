using System;
using System.Linq;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_SPEED : StockIndicatorBase
    {
        public StockIndicator_SPEED()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }

        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { "ER(20_1_1)", 12 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeIndicator(), new ParamRangeInt(2, 500) }; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Indicator", "Period" }; }
        }

        public override string[] SerieNames { get { return new string[] { "SPEED(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black) };
                }
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
                    lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)) };
                }
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie indicatorSerie = stockSerie.GetIndicator(this.parameters[0].ToString().Replace("_", ",")).Series[0];
            FloatSerie speedSerie = indicatorSerie - indicatorSerie.CalculateEMA((int)this.parameters[1]);

            this.series[0] = speedSerie;
            this.Series[0].Name = this.Name;
            CreateEventSeries(stockSerie.Count);

            this.Events[2] = new BoolSerie(this.EventNames[2], speedSerie.Values.Select(a => a >= 0));
            this.Events[3] = new BoolSerie(this.EventNames[3], speedSerie.Values.Select(a => a < 0));
            for (int i = 1; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = this.Events[2][i] && this.Events[3][i-1];
                this.eventSeries[1][i] = this.Events[3][i] && this.Events[2][i-1];
            }
        }

        static string[] eventNames = new string[] { "BullishCrossing", "BearishCrossing", "Positive", "Negative" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }

        static readonly bool[] isEvent = new bool[] { true, true, false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
