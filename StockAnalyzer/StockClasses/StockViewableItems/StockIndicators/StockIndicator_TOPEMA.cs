using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TOPEMA : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }

        public override IndicatorDisplayStyle DisplayStyle
        {
            get { return IndicatorDisplayStyle.SimpleCurve; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 30 }; }
        }

        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames
        {
            get { return new string[] { "TOPEMA.Sup", "TOPEMA.Res" }; }
        }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Green, 2), new Pen(Color.Red, 2) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);
            int period = (int)this.parameters[0];

            FloatSerie supportSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);
            FloatSerie resistanceSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);

            this.Series[0] = supportSerie;
            this.Series[0].Name = this.SerieNames[0];
            this.Series[1] = resistanceSerie;
            this.Series[1].Name = this.SerieNames[1];

            if (period >= stockSerie.Count)
                return;

            float alpha = 2.0f / (float)(period + 1);

            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            float resistanceEMA = highSerie[0];
            float supportEMA = lowSerie[0];
            supportSerie[period - 1] = supportEMA;
            resistanceSerie[period - 1] = resistanceEMA;
            bool isBullish = false;
            bool isBearish = false;
            float previousLow = supportEMA;
            float previousHigh = resistanceEMA;

            // Detecting events

            for (int i = 1; i < stockSerie.Count; i++)
            {
                // Resistance Management
                if (!float.IsNaN(resistanceEMA))
                {
                    resistanceEMA = Math.Min(resistanceEMA, resistanceEMA + alpha * (highSerie[i] - resistanceEMA));
                    if (closeSerie[i] > resistanceEMA) // Broken up
                    {
                        resistanceEMA = float.NaN;
                        this.Events[2][i] = true;// ResistanceBroken
                        if (isBullish == false)
                        {
                            isBullish = true;
                            this.Events[4][i] = true;// FirstResistanceBroken
                        }
                        isBearish = false;
                    }
                }
                else
                {
                    if (highSerie[i - 1] > highSerie[i] && highSerie[i - 2] > highSerie[i])
                    // Resistance Detected (new top ==> latest high is lowset than two previous highs tp avoid continous line on break)
                    {
                        resistanceEMA = Math.Max(highSerie[i - 2], highSerie[i - 1]);
                        resistanceSerie[i - 1] = resistanceEMA;
                        resistanceEMA = resistanceEMA + alpha * (highSerie[i] - resistanceEMA);
                        this.Events[1][i] = true; // ResistanceDetected
                        if (resistanceEMA < previousHigh)
                        {
                            this.Events[7][i] = true; // LowerHigh
                            this.StockTexts.Add(new StockText { AbovePrice = true, Index = i - 1, Text = "LH" });
                        }
                        previousHigh = resistanceEMA;
                    }
                }

                // Support Management
                if (!float.IsNaN(supportEMA))
                {
                    supportEMA = Math.Max(supportEMA, supportEMA + alpha * (lowSerie[i] - supportEMA));
                    if (closeSerie[i] < supportEMA) // Broken down
                    {
                        supportEMA = float.NaN;
                        this.Events[3][i] = true;// SupportBroken
                        if (isBearish == false)
                        {
                            isBearish = true;
                            this.Events[5][i] = true;// FirstSupportBroken
                        }
                        isBullish = false;
                    }
                }
                else
                {
                    if (lowSerie[i - 1] < lowSerie[i] && lowSerie[i - 2] < lowSerie[i]) // Support Detected (new low)
                    {
                        supportEMA = Math.Min(lowSerie[i - 2], lowSerie[i - 1]);
                        supportSerie[i - 1] = supportEMA;
                        supportEMA = supportEMA + alpha * (lowSerie[i] - supportEMA);
                        this.Events[0][i] = true; // SupportDetected
                        if (supportEMA > previousLow)
                        {
                            this.Events[6][i] = true; // HigerLow
                            this.StockTexts.Add(new StockText { AbovePrice = false, Index = i - 1, Text = "HL" });
                        }
                        previousLow = supportEMA;
                    }
                }

                supportSerie[i] = supportEMA;
                resistanceSerie[i] = resistanceEMA;
                this.Events[8][i] = isBullish; // Bullish
                this.Events[9][i] = isBearish; // Bearish
            }

        }

        private static string[] eventNames = new string[]
        {
            "SupportDetected", "ResistanceDetected",          // 0,1
            "ResistanceBroken", "SupportBroken",              // 2,3
            "FirstResistanceBroken", "FirstSupportBroken",    // 4,5
            "HigherLow", "LowerHigh",                         // 6,7
            "Bullish", "Bearish",                             // 8,9
        };
        public override string[] EventNames => eventNames;

        private static readonly bool[] isEvent = new bool[] { true, true, true, true, true, true, true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}