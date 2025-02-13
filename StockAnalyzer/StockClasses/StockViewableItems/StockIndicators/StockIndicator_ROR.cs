using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ROR : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Rate of rise" + Environment.NewLine + "Plots the current percent increase from the lowest low in the period";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "Period", "Trigger" };
        public override Object[] ParameterDefaultValues => new Object[] { 100, 0.30f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500), new ParamRangeFloat(0, 500) };
        public override string[] SerieNames => new string[] { "ROR(" + this.Parameters[0].ToString() + ")" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black) };
        public override string[] SerieFormats => serieFormats ??= new string[] { "P2" };

        static HLine[] lines = null;
        public override HLine[] HorizontalLines => lines ??= new HLine[] { new HLine((float)this.parameters[1], new Pen(Color.LightGray)) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie rocSerie = (stockSerie.CalculateRateOfRise((int)this.parameters[0], false));

            this.series[0] = rocSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            var trigger = (float)this.parameters[1];
            for (int i = 2; i < stockSerie.Count; i++)
            {
                int index = 0;
                float roc = rocSerie[i];
                float previousRoc = rocSerie[i - 1];
                this.eventSeries[index++][i] = (rocSerie[i - 2] < previousRoc && previousRoc > roc);
                this.eventSeries[index++][i] = (rocSerie[i - 2] > previousRoc && previousRoc < roc);
                this.eventSeries[index++][i] = roc == 0;
                this.eventSeries[index++][i] = (previousRoc == 0 && roc > 0);
                this.eventSeries[index++][i] = rocSerie[i] > trigger;
                this.eventSeries[index++][i] = rocSerie[i] < trigger;
            }
        }
        static readonly string[] eventNames = new string[] { "Top", "Bottom", "Zero", "OutOfZero", "AboveTrigger", "BelowTrigger" };
        public override string[] EventNames => eventNames;

        static readonly bool[] isEvent = new bool[] { true, true, true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
