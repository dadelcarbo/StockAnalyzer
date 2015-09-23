using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_HILBERT : StockIndicatorBase, IRange
    {
        public StockIndicator_HILBERT()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.RangedIndicator; }
        }
        public float Max
        {
            get { return 1.0f; }
        }

        public float Min
        {
            get { return -1.0f; }
        }

        public override string Name
        {
            get { return "HILBERT(" + this.parameters[0] + ")"; }
        }
        public override string Definition
        {
            get { return "HILBERT(int Smoothing)"; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "Smoothing" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 1 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { "HILBERT", "HILBERTL" }; } }
        
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Red), new Pen(Color.Green) };
                }
                return seriePens;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie sineSerie = null;
            FloatSerie sineLeadSerie = null;
            stockSerie.CalculateHilbertSineWave(stockSerie.GetSerie(StockDataType.CLOSE).CalculateEMA((int)this.parameters[0]), ref sineSerie, ref sineLeadSerie);

            this.series[0] = sineSerie;
            this.series[1] = sineLeadSerie;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 10; i < sineSerie.Count; i++)
            {
                this.eventSeries[0][i] = sineLeadSerie[i] > sineSerie[i];
                this.eventSeries[1][i] = !this.eventSeries[0][i];
                this.eventSeries[2][i] = this.eventSeries[0][i] && !this.eventSeries[0][i-1];
                this.eventSeries[3][i] = this.eventSeries[1][i] && !this.eventSeries[1][i - 1];
            }
        }

        static string[] eventNames = new string[] { "UpSwing", "DownSwing", "BullishCrossing", "BearishCrossing" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] {false, false, true, true};
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
