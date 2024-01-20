using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TOPEMA2 : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;

        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.SimpleCurve;

        public override string[] ParameterNames => new string[] { "UpPeriod", "DownPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 175, 35 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "TOPEMA2.Sup", "TOPEMA2.Res" };

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

            FloatSerie supportSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);
            FloatSerie resistanceSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);

            this.Series[0] = supportSerie;
            this.Series[0].Name = this.SerieNames[0];
            this.Series[1] = resistanceSerie;
            this.Series[1].Name = this.SerieNames[1];


            int periodUp = (int)this.parameters[0];
            int periodDown = (int)this.parameters[1];
            float alphaUp = 2.0f / (float)(periodUp + 1);
            float alphaDown = 2.0f / (float)(periodDown + 1);

            if (periodUp >= stockSerie.Count || periodDown >= stockSerie.Count)
                return;

            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            float resistanceEMA = highSerie[0];
            float supportEMA = lowSerie[0];
            supportSerie[periodUp - 1] = supportEMA;
            resistanceSerie[periodUp - 1] = resistanceEMA;
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
                    resistanceEMA = Math.Min(resistanceEMA, resistanceEMA + alphaDown * (highSerie[i] - resistanceEMA));
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
                        resistanceEMA = resistanceEMA + alphaDown * (highSerie[i] - resistanceEMA);
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
                    supportEMA = Math.Max(supportEMA, supportEMA + alphaUp * (lowSerie[i] - supportEMA));
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
                        supportEMA = supportEMA + alphaUp * (lowSerie[i] - supportEMA);
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

        private static readonly string[] eventNames = new string[]
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