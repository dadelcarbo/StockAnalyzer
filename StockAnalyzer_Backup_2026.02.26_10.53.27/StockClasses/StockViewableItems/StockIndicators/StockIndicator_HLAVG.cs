using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_HLAVG : StockIndicatorBase
    {
        public StockIndicator_HLAVG()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override object[] ParameterDefaultValues => new Object[] { 30, 1 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };
        public override string[] ParameterNames => new string[] { "Period", "InputSmoothing" };

        public override string[] SerieNames => new string[] { "HLAVG(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" };


        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.DarkRed) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE).CalculateEMA((int)this.parameters[1]);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie hlSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0]);
            this.Series[0] = hlSerie;

            int period = (int)this.parameters[0];

            float min, max;
            for (int i = 0; i < period; i++)
            {
                min = lowSerie.GetMin(0, i);
                max = highSerie.GetMax(0, i);
                hlSerie[i] = (min + max) / 2f;
            }
            for (int i = period; i < stockSerie.Count; i++)
            {
                min = lowSerie.GetMin(i - period, i);
                max = highSerie.GetMax(i - period, i);
                hlSerie[i] = (min + max) / 2f;
            }

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            for (int i = 2; i < hlSerie.Count; i++)
            {
                this.eventSeries[0][i] = (hlSerie[i - 2] > hlSerie[i - 1] && hlSerie[i - 1] < hlSerie[i]);
                this.eventSeries[1][i] = (hlSerie[i - 2] < hlSerie[i - 1] && hlSerie[i - 1] > hlSerie[i]);
                this.eventSeries[2][i] = closeSerie[i - 1] < hlSerie[i - 1] && closeSerie[i] > hlSerie[i];
                this.eventSeries[3][i] = closeSerie[i - 1] > hlSerie[i - 1] && closeSerie[i] < hlSerie[i];
                this.eventSeries[4][i] = lowSerie[i] > hlSerie[i] && lowSerie[i - 1] < hlSerie[i - 1];
                this.eventSeries[5][i] = highSerie[i] < hlSerie[i] && highSerie[i - 1] > hlSerie[i - 1];
                this.eventSeries[6][i] = lowSerie[i] > hlSerie[i] && closeSerie[i - 1] < closeSerie[i];
                this.eventSeries[7][i] = highSerie[i] < hlSerie[i] && closeSerie[i - 1] > closeSerie[i];
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

        static readonly string[] eventNames = new string[] { "Bottom", "Top", "CrossAbove", "CrossBelow", "FirstBarAbove", "FirstBarBelow", "Bullish", "Bearish", "BullRun", "BearRun" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, true, true, true, true, false, false, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
