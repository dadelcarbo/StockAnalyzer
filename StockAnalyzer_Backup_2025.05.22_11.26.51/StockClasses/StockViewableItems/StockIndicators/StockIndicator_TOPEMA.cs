using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TOPEMA : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draws EMA support and resistance from detected top or bottom with different up and down period, and detecting Top and Bottom from fast WMA price cross over";

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;

        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.SimpleCurve;

        public override string[] ParameterNames => new string[] { "UpPeriod", "DownPeriod", "Smoothing" };

        public override Object[] ParameterDefaultValues => new Object[] { 75, 35, 6 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "TOPEMA.Sup", "TOPEMA.Res", "WMA" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Green, 2), new Pen(Color.Red, 2), new Pen(Color.Blue, 2) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);

            FloatSerie supportSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);
            FloatSerie resistanceSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);

            int periodUp = (int)this.parameters[0];
            int periodDown = (int)this.parameters[1];
            float alphaUp = 2.0f / (float)(periodUp + 1);
            float alphaDown = 2.0f / (float)(periodDown + 1);
            int smoothingPeriod = (int)this.parameters[2];

            FloatSerie emaSerie = stockSerie.GetIndicator($"WMA({smoothingPeriod})").Series[0];

            this.Series[0] = supportSerie;
            this.Series[0].Name = this.SerieNames[0];
            this.Series[1] = resistanceSerie;
            this.Series[1].Name = this.SerieNames[1];
            this.Series[2] = emaSerie;
            this.Series[2].Name = this.SerieNames[2];

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
                    if (emaSerie[i - 1] > emaSerie[i] && emaSerie[i - 2] > emaSerie[i])
                    // Resistance Detected (new top ==> latest high is lowset than two previous highs tp avoid continous line on break)
                    {
                        resistanceEMA = highSerie.GetMax(i - smoothingPeriod, i);
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
                    if (emaSerie[i - 1] < emaSerie[i] && emaSerie[i - 2] < emaSerie[i]) // Support Detected (new low)
                    {
                        supportEMA = lowSerie.GetMin(i - smoothingPeriod, i); ;
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