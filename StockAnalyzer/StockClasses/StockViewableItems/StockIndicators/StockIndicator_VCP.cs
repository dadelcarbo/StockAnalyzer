using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_VCP : StockIndicatorBase, IRange
    {
        public override string Definition => $"Calculate the ratio between a long term and short range, it's usefull to find volatility contraction.";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.RangedIndicator;

        public float Max => 100.0f;
        public float Min => 0.0f;

        public override bool RequiresVolumeData { get { return false; } }

        public override string[] ParameterNames
        {
            get { return new string[] { "LongPeriod", "ShortPeriod" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 35, 12 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }
        public override string[] SerieNames => new string[] { "VCP(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" };

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
        HLine[] lines;
        public override HLine[] HorizontalLines => lines ??= new HLine[] { new HLine(25f, new Pen(Color.Gray) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var longPeriod = (int)parameters[0];
            var shortPeriod = (int)parameters[1];
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            FloatSerie vcpSerie = new FloatSerie(stockSerie.Count);
            for (int i = longPeriod; i < stockSerie.Count; i++)
            {
                var longRangeHigh = highSerie.GetMax(i - longPeriod, i);
                var longRangeLow = lowSerie.GetMin(i - longPeriod, i);
                var shortRangeHigh = highSerie.GetMax(i - shortPeriod, i);
                var shortRangeLow = lowSerie.GetMin(i - shortPeriod, i);

                vcpSerie[i] = 100f * (shortRangeHigh - shortRangeLow) / (longRangeHigh - longRangeLow);
            }

            this.Series[0] = vcpSerie;
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
