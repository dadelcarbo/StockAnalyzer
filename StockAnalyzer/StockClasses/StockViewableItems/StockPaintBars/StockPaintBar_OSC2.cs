using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    class StockPaintBar_OSC2 : StockPaintBarBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }

        public override bool HasTrendLine { get { return false; } }

        public override string[] ParameterNames
        {
            get { return new string[] { "FastPeriod1", "SlowPeriod1", "FastPeriod2", "SlowPeriod2" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 3, 15, 30, 60 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 1000), new ParamRangeInt(1, 100), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 100) }; }
        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[] { "Bullish", "BrokenUp" };
                }
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { false, true };
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
                    seriePens = new Pen[] { new Pen(Color.Green) { Width = 1 }, new Pen(Color.Green) { Width = 2 } };
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

            var osc1 = stockSerie.GetIndicator($"OSC({fastPeriod1},{slowPeriod1},True)").Series[0];
            var osc2 = stockSerie.GetIndicator($"OSC({fastPeriod2},{slowPeriod2},True)").Series[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            bool bull = false;
            for (int i = slowPeriod2; i < stockSerie.Count; i++)
            {
                if (bull)
                {
                    if (osc2[i] < 0)
                    {
                        bull = false;
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
