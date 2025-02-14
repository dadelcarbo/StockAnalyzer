using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_LOWEMA : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;

        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.SimpleCurve;

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 35 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { $"LOWEMA({this.parameters[0]})" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.DarkGreen, 2) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);
            int period = (int)this.parameters[0];

            FloatSerie supportSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);

            this.Series[0] = supportSerie;
            this.Series[0].Name = this.SerieNames[0];

            if (period >= stockSerie.Count)
                return;

            var supportDetectedEvent = this.GetEvents("SupportDetected");
            var supportBrokenEvent = this.GetEvents("SupportBroken");
            var bullishEvent = this.GetEvents("Bullish");
            var higherLowEvent = this.GetEvents("HigherLow");

            float alpha = 2.0f / (float)(period + 1);

            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            float supportEMA = lowSerie[0];
            supportSerie[period - 1] = supportEMA;
            bool isBullish = false;
            float previousLow = supportEMA;

            for (int i = 1; i < stockSerie.Count; i++)
            {
                // Support Management
                if (float.IsNaN(supportEMA))
                {
                    if (lowSerie[i - 1] < lowSerie[i] && lowSerie[i - 2] < lowSerie[i]) // Support Detected (new low)
                    {
                        supportEMA = Math.Min(lowSerie[i - 2], lowSerie[i - 1]);
                        //supportSerie[i - 1] = supportEMA;
                        supportEMA = supportEMA + alpha * (lowSerie[i] - supportEMA);
                        supportDetectedEvent[i] = true; // SupportDetected
                        if (supportEMA > previousLow)
                        {
                            higherLowEvent[i] = true; // HigerLow
                            this.StockTexts.Add(new StockText { AbovePrice = false, Index = i - 1, Text = "HL" });
                        }
                        previousLow = supportEMA;
                    }
                }
                else
                {
                    supportEMA = Math.Max(supportEMA, supportEMA + alpha * (lowSerie[i] - supportEMA));
                    if (closeSerie[i] < supportEMA) // Broken down
                    {
                        supportEMA = float.NaN;
                        supportBrokenEvent[i] = true;// SupportBroken
                        isBullish = false;
                    }
                }

                supportSerie[i] = supportEMA;
                bullishEvent[i] = isBullish; // Bullish
            }
        }

        private static readonly string[] eventNames = new string[] { "SupportDetected", "SupportBroken", "HigherLow", "Bullish" };
        public override string[] EventNames => eventNames;

        private static readonly bool[] isEvent = new bool[] { true, true, true, false };
        public override bool[] IsEvent => isEvent;
    }
}