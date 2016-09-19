using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_EMAL : StockIndicatorBase
    {
        public StockIndicator_EMAL()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }

        public override string Name
        {
            get { return "EMAL(" + this.Parameters[0].ToString() + ")"; }
        }

        public override string Definition
        {
            get { return "EMAL(int Period)"; }
        }
        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { 20 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }
        public override string[] SerieNames { get { return new string[] { "EMAL(" + this.Parameters[0].ToString() + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Blue) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie maSerie = lowSerie.CalculateEMA((int)this.parameters[0]);
            this.series[0] = maSerie;
            this.series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            for (int i = 2; i < maSerie.Count; i++)
            {
                this.eventSeries[0][i] = (maSerie[i - 2] > maSerie[i - 1] && maSerie[i - 1] < maSerie[i]);
                this.eventSeries[1][i] = (maSerie[i - 2] < maSerie[i - 1] && maSerie[i - 1] > maSerie[i]);
                this.eventSeries[2][i] = closeSerie[i - 1] < maSerie[i - 1] && closeSerie[i] > maSerie[i];
                this.eventSeries[3][i] = closeSerie[i - 1] > maSerie[i - 1] && closeSerie[i] < maSerie[i];
                this.eventSeries[4][i] = lowSerie[i] > maSerie[i] && lowSerie[i - 1] < maSerie[i - 1];
                this.eventSeries[5][i] = highSerie[i] < maSerie[i] && highSerie[i - 1] > maSerie[i - 1];
                this.eventSeries[6][i] = lowSerie[i] > maSerie[i] && closeSerie[i - 1] < closeSerie[i];
                this.eventSeries[7][i] = highSerie[i] < maSerie[i] && closeSerie[i - 1] > closeSerie[i];
                if (this.eventSeries[8][i - 1])
                {
                    // Check if BullRun Persists
                    this.eventSeries[8][i] = !this.eventSeries[5][i];
                }
                else
                {
                    // Check if BullRun Starts
                    this.eventSeries[8][i] = this.eventSeries[4][i];
                }
                if (this.eventSeries[9][i - 1])
                {
                    // Check if BearRun Persists
                    this.eventSeries[9][i] = !this.eventSeries[4][i];
                }
                else
                {
                    // Check if BearRun Starts
                    this.eventSeries[9][i] = this.eventSeries[5][i];
                }
            }
        }

        static string[] eventNames = new string[] { "Bottom", "Top", "CrossAbove", "CrossBelow", "FirstBarAbove", "FirstBarBelow", "Bullish", "Bearish", "BullRun", "BearRun" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, true, true, true, true, false, false, false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
