using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TRIP : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string Definition => "Calculate the distance travelled by price over a period, sum of in %";

        public override object[] ParameterDefaultValues => new Object[] { 260 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500)};
        public override string[] ParameterNames => new string[] { "Period" };
        public override string[] SerieNames => new string[] { $"PERF({this.Parameters[0]})" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie adrSerie = stockSerie.GetSerie(StockDataType.ADR) / stockSerie.GetSerie(StockDataType.CLOSE);
            var period = (int)this.parameters[0];

            FloatSerie tripSerie = new FloatSerie(stockSerie.Count);

            for (int i = period; i < stockSerie.Count; i++)
            {
                float trip = 0f;
                for (int j = 0; j < period; j++)
                {
                    trip += adrSerie[i - j];
                }
                tripSerie[i] = trip;
            }

            this.series[0] = tripSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
        }

        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
