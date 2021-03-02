using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_HIGHLOWDAYS : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }

        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { 10 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }

        public override string[] SerieNames { get { return new string[] { }; } }

        public override Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            int period = (int)this.parameters[0];

            // Detecting events
            float lowest = 0f, highest = 0f;

            for (int i = period; i < closeSerie.Count; i++)
            {
                highest = highSerie.GetMax(i - period, i - 1);
                lowest = lowSerie.GetMin(i - period, i - 1);

                bool isHighest = closeSerie[i] > highest;
                bool isLowest = closeSerie[i] < lowest;
                this.eventSeries[0][i] = isHighest && !this.eventSeries[2][i - 1];
                this.eventSeries[1][i] = isLowest && !this.eventSeries[3][i - 1];
                this.eventSeries[2][i] = isHighest;
                this.eventSeries[3][i] = isLowest;
            }
        }

        static string[] eventNames = new string[] { "NewHighest", "NewLowest", "Highest", "Lowest" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}


