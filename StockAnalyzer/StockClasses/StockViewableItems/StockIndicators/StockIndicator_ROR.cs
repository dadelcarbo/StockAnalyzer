using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ROR : StockIndicatorBase
    {
        public override string Definition => "Rate of rise" + Environment.NewLine + "Plots the current percent increase from the lowest low in the period";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "Period", "Smoothing", "SignalSmoothing" };
        public override Object[] ParameterDefaultValues => new Object[] { 200, 6, 12 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };
        public override string[] SerieNames => new string[]
                    {
                    "ROR(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")",
                    "SIGNAL(" + this.Parameters[2].ToString() + ")"
                    };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.DarkGreen), new Pen(Color.DarkRed) };
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
            FloatSerie rocSerie = (stockSerie.CalculateRateOfRise((int)this.parameters[0])).CalculateEMA((int)this.parameters[1]) * 100f;
            FloatSerie signalSerie = rocSerie.CalculateEMA((int)this.parameters[2]);

            this.series[0] = rocSerie;
            this.Series[0].Name = this.Name;

            this.series[1] = signalSerie;
            this.Series[1].Name = this.Name;

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
