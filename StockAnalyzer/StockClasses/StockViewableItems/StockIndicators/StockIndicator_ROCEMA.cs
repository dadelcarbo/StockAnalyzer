using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ROCEMA : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Rate of change" + Environment.NewLine + "Plots the current percent change from the begining of the period";

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override string[] ParameterNames => new string[] { "FastPeriod", "SlowPeriod", "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 50, 50 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };


        public override string[] SerieNames => new string[] { $"ROCEMA({this.Parameters[0]},{this.Parameters[1]},{this.Parameters[2]})" };

        public override System.Drawing.Pen[] SeriePens => seriePens ??= (seriePens = new Pen[] { new Pen(Color.Black) });

        static HLine[] lines = null;
        public override HLine[] HorizontalLines => lines ??= new HLine[] { new HLine(0, new Pen(Color.LightGray)) };
        public override void ApplyTo(StockSerie stockSerie)
        {
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var fastEma = closeSerie.CalculateEMA((int)this.parameters[0]);
            var slowEma = closeSerie.CalculateEMA((int)this.parameters[1]);
            int period = (int)this.parameters[2];
            FloatSerie rocSerie = new FloatSerie(stockSerie.Count);
            for (int i = period; i < stockSerie.Count; i++)
            {
                rocSerie[i] = (fastEma[i] - slowEma[i - period]) / slowEma[i - period];
            }

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
