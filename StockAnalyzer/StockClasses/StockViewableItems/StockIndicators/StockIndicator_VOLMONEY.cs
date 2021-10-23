using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_VOLMONEY : StockIndicatorBase
    {
        public override string Definition => "Calculate the exchanged volume money, Price*Volume in M€";
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }

        public override bool RequiresVolumeData { get { return true; } }

        public override string[] ParameterNames
        {
            get { return new string[] { }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { }; }
        }
        public override string[] SerieNames { get { return new string[] { "VOLMONEY M€" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black, 1) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie volume = stockSerie.GetSerie(StockDataType.VOLUME) * stockSerie.GetSerie(StockDataType.CLOSE) / 1000000.0f;

            this.Series[0] = volume;
            this.Series[0].Name = SerieNames[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
        }

        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
