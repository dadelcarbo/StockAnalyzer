using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ROD : StockIndicatorBase
    {
        public StockIndicator_ROD()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "Smoothing" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 200, 6 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames
        {
            get
            {
                return new string[]
                    {
                    "ROD(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")"
                    };
            }
        }

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
            FloatSerie rocSerie = (stockSerie.CalculateRateOfDecline((int)this.parameters[0])).CalculateEMA((int)this.parameters[1]) * -100f;

            this.series[0] = rocSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 2; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = (rocSerie[i - 2] < rocSerie[i - 1] && rocSerie[i - 1] > rocSerie[i]);
                this.eventSeries[1][i] = (rocSerie[i - 2] > rocSerie[i - 1] && rocSerie[i - 1] < rocSerie[i]);
                this.eventSeries[2][i] = (rocSerie[i] == 0);
                this.eventSeries[3][i] = (rocSerie[i - 1] == 0 && rocSerie[i] > 0);
            }
        }
        static string[] eventNames = new string[] { "Top", "Bottom", "Zero", "OutOfZero" };
        public override string[] EventNames => eventNames;

        static readonly bool[] isEvent = new bool[] { true, true, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
