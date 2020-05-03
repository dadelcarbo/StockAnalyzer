using System;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_GR : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }

        public override string Name
        {
            get { return "GR"; }
        }

        public override string Definition
        {
            get { return "Adjust close value reinvesting dividends"; }
        }
        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { }; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { }; }
        }
        public override string[] SerieNames { get { return new string[] { "GR" }; } }

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

            FloatSerie grSerie = new FloatSerie(stockSerie.Count, "GR");
            this.Series[0] = grSerie;
            grSerie[0] = closeSerie[0];
            float dividend = 0;
            int i = 1;
            foreach (var value in stockSerie.Values.Skip(1))
            {
                var entry = stockSerie?.Dividend?.Entries.FirstOrDefault(e => e.Date == value.DATE.Date);
                if (entry != null)
                {
                    dividend += entry.Dividend;
                }
                grSerie[i++] = value.CLOSE + dividend;
            }

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
        }

        static string[] eventNames = new string[] { };
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
