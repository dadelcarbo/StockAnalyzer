using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TRAILHLSR3 : StockIndicatorBase
    {
        public StockIndicator_TRAILHLSR3()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override IndicatorDisplayStyle DisplayStyle
        {
            get { return IndicatorDisplayStyle.SupportResistance; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 3 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(0, 500) }; }
        }

        public override string[] SerieNames
        {
            get
            {
                int period = (int)this.Parameters[0];
                return new string[]
                {
               "TRAILHL_" + period + ".S", "TRAILHL_" + period + ".R", "TRAILHL_" + period*3 + ".S", "TRAILHL_" + period*3 + ".R", "TRAILHL_" + period*9 + ".S", "TRAILHL_" + period*9 + ".R"
                };
            }
        }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green, 1), new Pen(Color.Red, 1), new Pen(Color.Green, 2), new Pen(Color.Red, 2), new Pen(Color.DarkGreen, 3), new Pen(Color.DarkRed, 3) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            Queue<float> resistanceQueue = new Queue<float>(new float[] { float.MinValue, float.MinValue });
            Queue<float> supportQueue = new Queue<float>(new float[] { float.MaxValue, float.MaxValue });

            int period = (int)this.Parameters[0];
            IStockIndicator indicator1 = stockSerie.GetIndicator("TRAILHLSR(" + period + ")");
            this.Series[0] = indicator1.Series[0];
            this.Series[0].Name = this.SerieNames[0];
            this.Series[1] = indicator1.Series[1];
            this.Series[1].Name = this.SerieNames[1];

            IStockIndicator indicator2 = stockSerie.GetIndicator("TRAILHLSR(" + period * 3 + ")");
            this.Series[2] = indicator2.Series[0];
            this.Series[2].Name = this.SerieNames[2];
            this.Series[3] = indicator2.Series[1];
            this.Series[3].Name = this.SerieNames[3];

            IStockIndicator indicator3 = stockSerie.GetIndicator("TRAILHLSR(" + period * 9 + ")");
            this.Series[4] = indicator3.Series[0];
            this.Series[4].Name = this.SerieNames[4];
            this.Series[5] = indicator3.Series[1];
            this.Series[5].Name = this.SerieNames[5];

            // Detecting events
            this.eventSeries = indicator2.Events;
        }

        static string[] eventNames = new string[] { "Bullish", "Bearish" };
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
