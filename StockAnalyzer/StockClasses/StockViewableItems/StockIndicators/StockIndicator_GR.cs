using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_GR : StockIndicatorBase
    {
        public override string Name => "GR(" + ((DateTime)this.Parameters[0]).ToShortDateString() + ")";
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }

        public override string Definition
        {
            get { return "Adjust close value reinvesting dividends"; }
        }
        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { new DateTime(2000, 01, 01) }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeDateTime(new DateTime(2000, 01, 01), DateTime.Today.AddDays(1)) }; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "StartDate" }; }
        }
        public override string[] SerieNames { get { return new string[] { "GR" }; } }

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Blue) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var startDate = (DateTime)this.parameters[0];

            FloatSerie grSerie = new FloatSerie(stockSerie.Count, "GR");
            this.Series[0] = grSerie;
            grSerie[0] = closeSerie[0];
            float dividend = 0;
            int i = 1;
            var previousBar = stockSerie.Values.First();
            foreach (var value in stockSerie.Values.Skip(1))
            {
                if (value.DATE.Date >= startDate)
                {
                    var entries = stockSerie?.Dividend?.Entries.Where(e => e.Date > previousBar.DATE && e.Date <= value.DATE.Date).ToList();
                    if (entries.Count > 0)
                    {
                        dividend = entries.Sum(e => e.Dividend) / previousBar.CLOSE;
                    }
                    else
                    {
                        dividend = 0;
                    }
                }
                var adjVar = 1 + value.VARIATION + dividend;
                grSerie[i] = grSerie[i - 1] * adjVar;
                previousBar = value;
                i++;
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
