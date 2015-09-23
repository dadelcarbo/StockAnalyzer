using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_HMA : StockIndicatorBase
    {
        public StockIndicator_HMA()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }

        public override string Name
        {
            get { return "HMA(" + this.Parameters[0].ToString() + ")"; }
        }

        public override string Definition
        {
            get { return "HMA(int Period)"; }
        }
        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { 20 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500)}; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }

        public override string[] SerieNames { get { return new string[] { "HMA(" + this.Parameters[0].ToString() + ")" }; } }

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
            FloatSerie HMASerie = closeSerie.CalculateHMA((int)this.parameters[0]);
            this.series[0] = HMASerie;
            this.series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            for (int i = 2; i < HMASerie.Count; i++)
            {
                this.eventSeries[0][i] = (HMASerie[i - 2] > HMASerie[i - 1] && HMASerie[i - 1] < HMASerie[i]);
                this.eventSeries[1][i] = (HMASerie[i - 2] < HMASerie[i - 1] && HMASerie[i - 1] > HMASerie[i]);
                this.eventSeries[2][i] = closeSerie[i] > HMASerie[i];
                this.eventSeries[3][i] = closeSerie[i] < HMASerie[i];
                this.eventSeries[4][i] = HMASerie[i] > HMASerie[i - 1];
                this.eventSeries[5][i] = HMASerie[i] < HMASerie[i - 1];
            }
        }

        static string[] eventNames = new string[] { "Bottom", "Top", "PriceAbove", "PriceBelow", "UpWard", "DownWard" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, false, false, false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
