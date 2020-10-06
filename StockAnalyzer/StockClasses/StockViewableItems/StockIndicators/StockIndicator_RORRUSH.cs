using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_RORRUSH : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Rate of rise" + Environment.NewLine + "Plots the current percent increase from the lowest low in the period";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "Period", "Smoothing", "Trigger" };
        public override Object[] ParameterDefaultValues => new Object[] { 100, 1, 10f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(1f, 500f) };
        public override string[] SerieNames => new string[]
                    {
                    "ROR(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")"
                    };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.DarkGreen) };
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
            var period = (int)this.parameters[0];
            FloatSerie rorSerie = (stockSerie.CalculateRateOfRise(period).CalculateEMA((int)this.parameters[1])) * 100f;

            this.series[0] = rorSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            var trigger = (float)this.parameters[2];
            for (int i = period; i < stockSerie.Count; i++)
            {
                var min = rorSerie.GetMin(i - period, i - 1);

                this.eventSeries[0][i] = rorSerie[i] - min > trigger;
            }
        }
        static string[] eventNames = new string[] { "Rush" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true };
        public override bool[] IsEvent => isEvent;
    }
}
