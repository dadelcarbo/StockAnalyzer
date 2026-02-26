using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_DDADR : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Drowdown expressed in ADR" + Environment.NewLine + "Plots the drawdown in number of ADR";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "Period", "ADR Period", "Trigger" };
        public override Object[] ParameterDefaultValues => new Object[] { 35, 15, 2f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500), new ParamRangeInt(0, 500), new ParamRangeFloat(0, 500) };
        public override string[] SerieNames => new string[] { $"DDADR({this.Parameters[0]},{this.Parameters[1]})" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black) };

        public override HLine[] HorizontalLines => lines ??= new HLine[] { new HLine((float)this.parameters[2], new Pen(Color.Black) { Width = 2 }) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var ddSerie = stockSerie.CalculateDrawdownValue((int)this.parameters[0]);
            var adrSerie = stockSerie.GetIndicator($"ADR({this.parameters[1]}").Series[0];
            var DDADRSerie = ddSerie / adrSerie;

            this.series[0] = DDADRSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            var trigger = (float)this.parameters[2];
            for (int i = 2; i < stockSerie.Count; i++)
            {
                int index = 0;
                this.eventSeries[index++][i] = DDADRSerie[i] > trigger;
                this.eventSeries[index++][i] = DDADRSerie[i] < trigger;

            }
        }
        static readonly string[] eventNames = new string[] { "AboveTrigger", "BelowTrigger" };
        public override string[] EventNames => eventNames;

        static readonly bool[] isEvent = new bool[] { true, true, true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
