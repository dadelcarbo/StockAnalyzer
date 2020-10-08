using System;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_GUPPY : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string[] ParameterNames => new string[] {
                "FastPeriod1", "FastPeriod2", "FastPeriod3", "FastPeriod4", "FastPeriod5", "FastPeriod6",
                "SlowPeriod1", "SlowPeriod2", "SlowPeriod3", "SlowPeriod4", "SlowPeriod5", "SlowPeriod6"
            };

        public override Object[] ParameterDefaultValues => new Object[] { 3, 5, 8, 10, 12, 15, 30, 35, 40, 45, 50, 60 };

        public override ParamRange[] ParameterRanges => new ParamRange[] {
            new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000),
            new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000) };

        public override string[] SerieNames => this.parameters.Select(p => $"EMA_{p.ToString()}").ToArray();

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Green), new Pen(Color.Green), new Pen(Color.Green), new Pen(Color.Green), new Pen(Color.Green),
                        new Pen(Color.Red), new Pen(Color.Red), new Pen(Color.Red), new Pen(Color.Red), new Pen(Color.Red), new Pen(Color.Red) };

                }
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
        }

        static string[] eventNames = new string[] { "UpTrend", "DownTrend" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
