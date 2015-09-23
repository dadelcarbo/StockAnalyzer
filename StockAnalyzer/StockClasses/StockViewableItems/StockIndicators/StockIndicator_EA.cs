using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_EA : StockIndicatorBase
    {
        public StockIndicator_EA()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }

        public override string Name
        {
            get { return "EA(" + this.Parameters[0].ToString() + ")"; }
        }

        public override string Definition
        {
            get { return "EA(int Period)"; }
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
        public override string[] SerieNames { get { return new string[] { "EA(" + this.Parameters[0].ToString() + ")" }; } }

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
            FloatSerie EASerie = stockSerie.GetSerie(StockDataType.CLOSE).CalculateEA((int)this.parameters[0]);
            this.series[0] = EASerie;
            this.series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            for (int i = 2; i < EASerie.Count; i++)
            {
                this.eventSeries[0][i] = (EASerie[i - 2] > EASerie[i - 1] && EASerie[i - 1] < EASerie[i]);
                this.eventSeries[1][i] = (EASerie[i - 2] < EASerie[i - 1] && EASerie[i - 1] > EASerie[i]);
            }
        }

        static string[] eventNames = new string[] { "Bottom", "Top" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true,true};
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
