using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_HIGHEMA : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;

        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.SimpleCurve;

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 35 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { $"HIGHEMA({this.parameters[0]})" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.DarkRed, 2) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);
            int period = (int)this.parameters[0];

            FloatSerie resistanceSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);

            this.Series[0] = resistanceSerie;
            this.Series[0].Name = this.SerieNames[0];

            if (period >= stockSerie.Count)
                return;

            var resistanceDetectedEvent = this.GetEvents("ResistanceDetected");
            var resistanceBrokenEvent = this.GetEvents("ResistanceBroken");
            var bearishEvent = this.GetEvents("Bearish");
            var higherHighEvent = this.GetEvents("HigherHigh");

            float alpha = 2.0f / (float)(period + 1);

            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            float resistanceEMA = highSerie[0];
            resistanceSerie[period - 1] = resistanceEMA;
            bool isBearish = false;
            float previousHigh = resistanceEMA;

            for (int i = 1; i < stockSerie.Count; i++)
            {
                // Resistance Management
                if (float.IsNaN(resistanceEMA))
                {
                    if (highSerie[i - 1] > highSerie[i] && highSerie[i - 2] > highSerie[i]) // Resistance Detected (new high)
                    {
                        resistanceEMA = Math.Max(highSerie[i - 2], highSerie[i - 1]);
                        //resistanceSerie[i - 1] = resistanceEMA;
                        resistanceEMA = resistanceEMA + alpha * (highSerie[i] - resistanceEMA);
                        resistanceDetectedEvent[i] = true; // ResistanceDetected
                        if (resistanceEMA < previousHigh)
                        {
                            higherHighEvent[i] = true; // LowerHigh
                            this.StockTexts.Add(new StockText { AbovePrice = true, Index = i - 1, Text = "LH" });
                        }
                        previousHigh = resistanceEMA;
                    }
                }
                else
                {
                    resistanceEMA = Math.Min(resistanceEMA, resistanceEMA + alpha * (highSerie[i] - resistanceEMA));
                    if (closeSerie[i] > resistanceEMA) // Broken down
                    {
                        resistanceEMA = float.NaN;
                        resistanceBrokenEvent[i] = true;// ResistanceBroken
                        isBearish = false;
                    }
                }

                resistanceSerie[i] = resistanceEMA;
                bearishEvent[i] = isBearish; // Bearish
            }
        }

        private static readonly string[] eventNames = new string[] { "ResistanceDetected", "ResistanceBroken", "HigherHigh", "Bearish" };
        public override string[] EventNames => eventNames;

        private static readonly bool[] isEvent = new bool[] { true, true, true, false };
        public override bool[] IsEvent => isEvent;
    }
}