using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ROC3 : StockIndicatorBase
    {
        public StockIndicator_ROC3()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }

        public override string Definition
        {
            get { return "ROC3(int LongPeriod, int MediumPeriod, int ShortPeriod, int Smoothing, int SignalSmoothing)"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "LongPeriod", "MediumPeriod", "MediumPeriod", "Smoothing", "SignalSmoothing" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 200, 100, 50, 6, 12 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) , new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames
        {
            get
            {
                return new string[]
                {
                    "ROC3(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + "," + this.Parameters[2].ToString()  + "," + this.Parameters[3].ToString() + ")",
                    "SIGNAL(" + this.Parameters[4].ToString() + ")"
                };
            }
        }

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
                    lines = new HLine[] {new HLine(0, new Pen(Color.LightGray))};
                }
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie rocSerie = (stockSerie.CalculateRateOfChange((int)this.parameters[0]) + stockSerie.CalculateRateOfChange((int)this.parameters[1]) + stockSerie.CalculateRateOfChange((int)this.parameters[2])).CalculateEMA((int)this.parameters[3]);
            FloatSerie signalSerie = rocSerie.CalculateEMA((int)this.parameters[4]);

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
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
