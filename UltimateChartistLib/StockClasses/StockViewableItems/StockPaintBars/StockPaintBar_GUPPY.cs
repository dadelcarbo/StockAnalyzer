using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    class StockPaintBar_GUPPY : StockPaintBarBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }

        public override bool HasTrendLine { get { return false; } }

        public override string[] ParameterNames => new string[] { "FastPeriod1", "SlowPeriod1", "FastPeriod2", "SlowPeriod2" };
        public override Object[] ParameterDefaultValues => new Object[] { 3, 15, 30, 60 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000) };

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[] { "Bullish", "LongEntry", "LongExit" };
                }
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { false, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green) { Width = 1 }, new Pen(Color.Green) { Width = 2 }, new Pen(Color.Red) { Width = 2 } };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int fastPeriod1 = (int)this.parameters[0];
            int slowPeriod1 = (int)this.parameters[1];
            int fastPeriod2 = (int)this.parameters[2];
            int slowPeriod2 = (int)this.parameters[3];

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var fastEMA1 = closeSerie.CalculateEMA(fastPeriod1);
            var slowEMA1 = closeSerie.CalculateEMA(slowPeriod1);
            var fastEMA2 = closeSerie.CalculateEMA(fastPeriod2);
            var slowEMA2 = closeSerie.CalculateEMA(slowPeriod2);

            var osc1 = fastEMA1 - slowEMA1;
            var osc2 = fastEMA2 - slowEMA2;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            bool bull = false;
            for (int i = slowPeriod2; i < stockSerie.Count; i++)
            {
                if (bull)
                {
                    if (closeSerie[i] < slowEMA2[i])
                    {
                        bull = false;
                        this.Events[2][i] = true;
                    }
                }
                else
                {
                    if (osc2[i] > 0 && osc1[i - 1] < 0 && osc1[i] > 0)
                    {
                        bull = true;
                        this.Events[1][i] = true;
                    }
                }

                this.Events[0][i] = bull;
            }
        }
    }
}
