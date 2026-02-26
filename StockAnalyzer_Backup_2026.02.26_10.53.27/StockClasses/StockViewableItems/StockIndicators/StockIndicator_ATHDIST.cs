using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ATHDIST : StockIndicatorBase
    {
        public override string Definition => "Calculates the number of ATR to reach new ATH";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override string[] ParameterNames => new string[] { "ATH Lookback", "ATR Period" };
        public override object[] ParameterDefaultValues => new Object[] { 35, 10 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(0, 500) };

        public override string[] SerieNames => new string[] { $"ATHDIST({this.Parameters[0]},{this.Parameters[1]})" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black) };

        public override HLine[] HorizontalLines => lines ??= new HLine[] { new HLine(1, new Pen(Color.LightGray)), new HLine(2, new Pen(Color.LightGray)), new HLine(3, new Pen(Color.LightGray)) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);

            var athDistSerie = new FloatSerie(stockSerie.Count);

            this.series[0] = athDistSerie;
            this.Series[0].Name = this.Name;

            int lookback = (int)this.Parameters[0];
            int atrPeriod = (int)this.Parameters[1];

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var athSerie = closeSerie.MaxSerie(lookback);
            var atrSerie = stockSerie.GetIndicator($"ATR({atrPeriod})").Series[0];

            for (int i = lookback; i < stockSerie.Count; i++)
            {
                athDistSerie[i] = (athSerie[i] - closeSerie[i]) / atrSerie[i];
            }
        }
        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
