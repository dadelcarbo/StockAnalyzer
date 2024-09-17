using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ROR : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Rate of rise" + Environment.NewLine + "Plots the current percent increase from the lowest low in the period";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "Period" };
        public override Object[] ParameterDefaultValues => new Object[] { 100 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };
        public override string[] SerieNames => new string[] { "ROR(" + this.Parameters[0].ToString() + ")" };

        public override System.Drawing.Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black) };
        public override string[] SerieFormats => serieFormats ??= new string[] { "P2" };

        static HLine[] lines = null;
        public override HLine[] HorizontalLines => lines ??= new HLine[] { new HLine(0, new Pen(Color.LightGray)) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];
            FloatSerie rocSerie = (stockSerie.CalculateRateOfRise(period, false));

            this.series[0] = rocSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            for (int i = 2; i < stockSerie.Count; i++)
            {
                float roc = rocSerie[i];
                float previousRoc = rocSerie[i - 1];
                this.eventSeries[0][i] = (rocSerie[i - 2] < previousRoc && previousRoc > roc);
                this.eventSeries[1][i] = (rocSerie[i - 2] > previousRoc && previousRoc < roc);
                this.eventSeries[2][i] = roc == 0;
                this.eventSeries[3][i] = (previousRoc == 0 && roc > 0);
            }
        }
        static readonly string[] eventNames = new string[] { "Top", "Bottom", "Zero", "OutOfZero" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
