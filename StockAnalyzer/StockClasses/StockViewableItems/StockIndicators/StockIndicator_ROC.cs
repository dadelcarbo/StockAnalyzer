using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ROC : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Rate of change" + Environment.NewLine + "Plots the current percent change from the begining of the period";

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 100 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { $"ROC({this.Parameters[0]})" };

        public override System.Drawing.Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black) };

        static HLine[] lines = null;
        public override HLine[] HorizontalLines => lines ??= new HLine[] { new HLine(0, new Pen(Color.LightGray)) };
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie rocSerie = stockSerie.CalculateRateOfChange((int)this.parameters[0]);
            this.series[0] = rocSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 2; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = (rocSerie[i - 2] < rocSerie[i - 1] && rocSerie[i - 1] > rocSerie[i]);
                this.eventSeries[1][i] = (rocSerie[i - 2] > rocSerie[i - 1] && rocSerie[i - 1] < rocSerie[i]);
                this.eventSeries[2][i] = (rocSerie[i - 1] < 0 && rocSerie[i] >= 0);
                this.eventSeries[3][i] = (rocSerie[i - 1] > 0 && rocSerie[i] <= 0);
            }
        }

        static string[] eventNames = new string[] { "Top", "Bottom", "TurnedPositive", "TurnedNegative" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
