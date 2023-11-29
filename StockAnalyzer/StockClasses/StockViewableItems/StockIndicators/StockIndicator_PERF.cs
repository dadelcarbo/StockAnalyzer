using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_PERF : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public override string Definition => "Calculate conditional return based on condition";

        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { 260, 0.75f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 5f) }; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period1", "NbDev" }; }
        }
        public override string[] SerieNames { get { return new string[] { $"PERF({this.Parameters[0]},{this.Parameters[1]})" }; } }

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
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var ema1 = closeSerie.CalculateEMA((int)this.parameters[0]);
            var stdev = closeSerie.CalculateStdev((int)this.parameters[0]);

            FloatSerie perfSerie = new FloatSerie(stockSerie.Count);

            var nbDev = (float)this.parameters[1];

            bool upSwing = false;
            float perf = 100f;
            perfSerie[0] = perf;
            for (int i = 1; i < stockSerie.Count; i++)
            {
                if (upSwing)
                {
                    perf *= 1f + (closeSerie[i] - closeSerie[i - 1]) / closeSerie[i - 1];
                }
                upSwing = closeSerie[i] > (ema1[i] + nbDev * stdev[i]);
                perfSerie[i] = perf;
            }

            this.series[0] = perfSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
        }

        static string[] eventNames = new string[] { };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
