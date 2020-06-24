using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_MID : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }

        public override string Definition
        {
            get { return "Calculates the mid point from High and Low over the given period"; }
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
        public override string[] SerieNames { get { return new string[] { "MID(" + this.Parameters[0].ToString() + ")" }; } }

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
            int period = (int)this.parameters[0];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            // Calculate MID line 
            FloatSerie upLine = new FloatSerie(stockSerie.Count);
            FloatSerie midLine = new FloatSerie(stockSerie.Count);
            FloatSerie downLine = new FloatSerie(stockSerie.Count);

            upLine[0] = closeSerie[0];
            downLine[0] = closeSerie[0];
            midLine[0] = closeSerie[0];

            for (int i = 1; i < stockSerie.Count; i++)
            {
                upLine[i] = highSerie.GetMax(Math.Max(0, i - period - 1), i - 1);
                downLine[i] = lowSerie.GetMin(Math.Max(0, i - period - 1), i - 1);
                midLine[i] = (upLine[i] + downLine[i]) / 2.0f;
            }
            this.series[0] = midLine;
            this.series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            for (int i = 2; i < midLine.Count; i++)
            {
                this.eventSeries[0][i] = (midLine[i - 2] > midLine[i - 1] && midLine[i - 1] < midLine[i]);
                this.eventSeries[1][i] = (midLine[i - 2] < midLine[i - 1] && midLine[i - 1] > midLine[i]);
                this.eventSeries[2][i] = closeSerie[i - 1] < midLine[i - 1] && closeSerie[i] > midLine[i];
                this.eventSeries[3][i] = closeSerie[i - 1] > midLine[i - 1] && closeSerie[i] < midLine[i];
                this.eventSeries[4][i] = lowSerie[i] > midLine[i] && lowSerie[i - 1] < midLine[i - 1];      // FirstBarAbove
                this.eventSeries[5][i] = highSerie[i] < midLine[i] && highSerie[i - 1] > midLine[i - 1];    // FirstBarBelow
                this.eventSeries[6][i] = lowSerie[i] > midLine[i] && closeSerie[i - 1] < closeSerie[i];
                this.eventSeries[7][i] = highSerie[i] < midLine[i] && closeSerie[i - 1] > closeSerie[i];
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
