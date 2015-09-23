using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_FISHER : StockIndicatorBase
    {
        public StockIndicator_FISHER()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }

        public override string Name
        {
            get { return "FISHER(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + "," + this.Parameters[2].ToString() + ")"; }
        }
        public override string Definition
        {
            get { return "FISHER(int stochPeriod, int stochSmoothing, int fisherSmoothing)"; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "stochPeriod", "stochSmoothing", "fisherSmoothing"}; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 14, 3, 3 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500)}; }
        }

        public override string[] SerieNames { get { return new string[] { "FastFish(" + this.Parameters[0].ToString() + ")", "SlowFish(" + this.Parameters[1].ToString() + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red) };
                }
                return seriePens;
            }
        }
        public override HLine[] HorizontalLines
        {
            get
            {
                return null;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie fastFisher = stockSerie.GetSerie(StockDataType.CLOSE).CalculateStochastik((int)this.parameters[0], (int)this.parameters[1]).CalculateFisher((int)this.parameters[2]);
            FloatSerie slowFisher = fastFisher.CalculateEMA((int)this.parameters[2]);
            this.series[0] = fastFisher;
            this.series[0].Name = this.SerieNames[0];
            this.series[1] = slowFisher;
            this.series[1].Name = this.SerieNames[1];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);


            for (int i = 1; i < fastFisher.Count; i++)
            {
                this.eventSeries[0][i] = (slowFisher[i - 1] > fastFisher[i - 1] && slowFisher[i] < fastFisher[i]);
                this.eventSeries[1][i] = (slowFisher[i - 1] < fastFisher[i - 1] && slowFisher[i] > fastFisher[i]);
            }
        }

        static string[] eventNames = new string[] { "BullishCrossing", "BearishCrossing" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true};
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
