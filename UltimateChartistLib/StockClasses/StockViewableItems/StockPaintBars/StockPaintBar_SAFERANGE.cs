using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    class StockPaintBar_SAFERANGE : StockPaintBarBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }

        public override bool HasTrendLine { get { return true; } }

        public override string[] ParameterNames
        {
            get { return new string[] { "LowTrigger", "HighTrigger" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 0.05f, 0.05f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeFloat(0f, 1f), new ParamRangeFloat(0f, 1f) }; }
        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[] { "Bullish", "Bearish" };
                }
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true, true };
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
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red) };
                    foreach (Pen pen in seriePens)
                    {
                        pen.Width = 2;
                    }
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            var lowTriggerRatio = 1 + (float)this.parameters[0];
            var highTriggerRatio = 1 - (float)this.parameters[1];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie LowSerie = stockSerie.GetSerie(StockDataType.LOW);

            bool isBullish = false;
            bool isBearish = false;
            float bullTrigger = closeSerie[0] * lowTriggerRatio;
            float bearTrigger = closeSerie[0] * highTriggerRatio;
            for (int i = 1; i < stockSerie.Count; i++)
            {
                var close = closeSerie[i];
                if (isBullish)
                {
                    bearTrigger = Math.Max(closeSerie[i] * highTriggerRatio, bearTrigger);
                    if (close < bearTrigger)
                    {
                        isBullish = false;
                        bullTrigger = closeSerie[i] * lowTriggerRatio;
                    }
                }
                else
                {
                    bullTrigger = Math.Min(closeSerie[i] * lowTriggerRatio, bullTrigger);
                    if (close > bullTrigger)
                    {
                        isBullish = true;
                        bearTrigger = closeSerie[i] * highTriggerRatio;
                    }
                }
                if (isBearish ^ isBullish)
                {
                    this.eventSeries[0][i] = isBullish;
                    this.eventSeries[1][i] = isBearish;
                }
            }
        }
    }
}
