using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_GUPPY : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string[] ParameterNames => new string[] {
                "FastPeriod1", "FastPeriod2", "FastPeriod3", "FastPeriod4", "FastPeriod5", "FastPeriod6",
                "SlowPeriod1", "SlowPeriod2", "SlowPeriod3", "SlowPeriod4", "SlowPeriod5", "SlowPeriod6"
            };

        public override Object[] ParameterDefaultValues => new Object[] { 3, 5, 8, 10, 12, 15, 30, 35, 40, 45, 50, 60 };

        public override ParamRange[] ParameterRanges => new ParamRange[] {
            new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000),
            new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000) };

        public override string[] SerieNames => this.parameters.Select(p => $"EMA_{p.ToString()}").ToArray();

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Green), new Pen(Color.Green), new Pen(Color.Green), new Pen(Color.Green), new Pen(Color.Green), new Pen(Color.Green),
                        new Pen(Color.Red), new Pen(Color.Red), new Pen(Color.Red), new Pen(Color.Red), new Pen(Color.Red), new Pen(Color.Red) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            for (int i = 0; i < parameters.Length; i++)
            {
                this.Series[i] = closeSerie.CalculateEMA((int)this.parameters[i]);
            }

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            var fastSerie1 = this.Series[0];
            var slowSerie1 = this.Series[5];
            var fastSerie2 = this.Series[6];
            var slowSerie2 = this.Series[11];

            for (int i = (int)this.parameters.Last(); i < stockSerie.Count; i++)
            {
                // BullStart
                this.eventSeries[0][i] = fastSerie2[i] > slowSerie2[i] && fastSerie1[i] > slowSerie1[i] && fastSerie2[i - 1] < slowSerie2[i - 1];
                // BearStart
                this.eventSeries[1][i] = fastSerie2[i] < slowSerie2[i] && fastSerie1[i] < slowSerie1[i] && fastSerie1[i - 1] > slowSerie1[i - 1];
                // BullCrossing
                this.eventSeries[2][i] = closeSerie[i - 1] < fastSerie1[i - 1] && closeSerie[i] > fastSerie1[i] && fastSerie1[i] > slowSerie1[i] && fastSerie2[i] > slowSerie2[i];
                // BearCrossing
                this.eventSeries[3][i] = closeSerie[i - 1] > fastSerie1[i - 1] && closeSerie[i] < fastSerie1[i] && fastSerie1[i] < slowSerie1[i] && fastSerie2[i] < slowSerie2[i];
            }
        }

        static readonly string[] eventNames = new string[] { "BullStart", "BearStart", "BullCrossing", "BearCrossing" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
