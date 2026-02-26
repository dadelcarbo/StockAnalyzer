using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ROD : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Rate of decline" + Environment.NewLine + "Plots the current percent decrease from the highest high in the period";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "Period", "Trigger" };
        public override Object[] ParameterDefaultValues => new Object[] { 100, 0.25f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500), new ParamRangeFloat(0, 500) };
        public override string[] SerieNames => new string[] { $"ROD({this.Parameters[0]})" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black) };
        public override string[] SerieFormats => serieFormats ??= new string[] { "P2" };

        public override HLine[] HorizontalLines => lines ??= new HLine[] { new HLine((float)this.parameters[1], new Pen(Color.Black) { Width = 2 }) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie rodSerie = (stockSerie.CalculateRateOfDecline((int)this.parameters[0]));

            this.series[0] = rodSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);


            var trigger = (float)this.parameters[1];
            for (int i = 2; i < stockSerie.Count; i++)
            {
                int index = 0;
                this.eventSeries[index++][i] = (rodSerie[i - 2] < rodSerie[i - 1] && rodSerie[i - 1] > rodSerie[i]);
                this.eventSeries[index++][i] = (rodSerie[i - 2] > rodSerie[i - 1] && rodSerie[i - 1] < rodSerie[i]);
                this.eventSeries[index++][i] = (rodSerie[i] == 0);
                this.eventSeries[index++][i] = (rodSerie[i - 1] == 0 && rodSerie[i] > 0);
                this.eventSeries[index++][i] = rodSerie[i] > trigger;
                this.eventSeries[index++][i] = rodSerie[i] < trigger;

            }
        }
        static readonly string[] eventNames = new string[] { "Top", "Bottom", "Zero", "OutOfZero", "AboveTrigger", "BelowTrigger" };
        public override string[] EventNames => eventNames;

        static readonly bool[] isEvent = new bool[] { true, true, true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
