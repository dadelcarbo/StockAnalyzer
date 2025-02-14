using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_RANGE : StockIndicatorBase
    {
        public override string Definition => $"Calculate a long term and short range, it's usefull to find volatility contraction.";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override bool RequiresVolumeData => false;

        public override string[] ParameterNames => new string[] { "LongPeriod", "ShortPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 35, 12 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };
        public override string[] SerieNames => new string[] { $"RANGE1({Parameters[0]})", $"RANGE2({Parameters[1]})" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.DarkGreen, 1), new Pen(Color.DarkRed, 1) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            var longPeriod = (int)parameters[0];
            var shortPeriod = (int)parameters[1];
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            FloatSerie range1Serie = new FloatSerie(stockSerie.Count);
            FloatSerie range2Serie = new FloatSerie(stockSerie.Count);
            for (int i = longPeriod; i < stockSerie.Count; i++)
            {
                var longRangeHigh = highSerie.GetMax(i - longPeriod, i);
                var longRangeLow = lowSerie.GetMin(i - longPeriod, i);
                var shortRangeHigh = highSerie.GetMax(i - shortPeriod, i);
                var shortRangeLow = lowSerie.GetMin(i - shortPeriod, i);

                range1Serie[i] = longRangeHigh - longRangeLow;
                range2Serie[i] = shortRangeHigh - shortRangeLow;
            }

            this.Series[0] = range1Serie;
            this.Series[0].Name = SerieNames[0];
            this.Series[1] = range2Serie;
            this.Series[1].Name = SerieNames[1];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
        }

        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
