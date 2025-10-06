using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TOPOSC : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draws multiple supports from bottom based on Mac Clellan oscillator";

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;

        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.SimpleCurve;

        public override string[] ParameterNames => new string[] { "FastPeriod", "SlowPeriod", "Smoothing" };

        public override Object[] ParameterDefaultValues => new Object[] { 35, 75, 3 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "OSC UNCH", $"EMA({parameters[0]})", $"EMA({parameters[1]})", "SUM/10" };
        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Blue), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.DarkRed) };
        public override Area[] Areas => areas ??= new Area[]
            {
                new Area {Name="Bull", Color = Color.FromArgb(64, Color.Green), Visibility = true }
            };

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);

            FloatSerie supportSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);
            FloatSerie slowSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);
            FloatSerie fastSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);
            FloatSerie unchSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);

            this.Areas[0].UpLine = new FloatSerie(stockSerie.Count, float.NaN);
            this.Areas[0].DownLine = new FloatSerie(stockSerie.Count, float.NaN);

            int fastPeriod = (int)this.parameters[0];
            int slowPeriod = (int)this.parameters[1];
            float fastAlfa = 2.0f / (float)(fastPeriod + 1);
            float slowAlfa = 2.0f / (float)(slowPeriod + 1);
            int smoothingPeriod = (int)this.parameters[2];

            this.Series[0] = unchSerie;
            this.Series[0].Name = this.SerieNames[0];
            this.Series[1] = fastSerie;
            this.Series[1].Name = this.SerieNames[1];
            this.Series[2] = slowSerie;
            this.Series[2].Name = this.SerieNames[2];
            this.Series[3] = supportSerie;
            this.Series[3].Name = this.SerieNames[3];

            if (fastPeriod >= stockSerie.Count || slowPeriod >= stockSerie.Count)
                return;

            FloatSerie emaSerie = stockSerie.GetIndicator($"EMA({smoothingPeriod})").Series[0];
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var fastEma = float.NaN;
            var slowEma = float.NaN;
            var osc = 0.0f;
            var unch = float.NaN;
            var support = float.NaN;
            var previousLow = lowSerie[0];

            // Detecting events
            for (int i = 2; i < stockSerie.Count; i++)
            {
                // Support Management
                if (float.IsNaN(support))
                {
                    if (emaSerie[i - 1] < emaSerie[i] && emaSerie[i - 2] < emaSerie[i]) // Support Detected (new low)
                    {
                        support = fastEma = slowEma = unch = lowSerie.GetMin(i - smoothingPeriod, i);
                        this.Events[0][i] = true; // SupportDetected
                        if (support > previousLow)
                        {
                            this.Events[6][i] = true; // HigerLow
                            this.StockTexts.Add(new StockText { AbovePrice = false, Index = i - 1, Text = "HL" });
                        }
                        previousLow = support;
                    }
                }
                else
                {
                    if (closeSerie[i] < slowEma) // Broken down
                    {
                        fastEma = float.NaN;
                        slowEma = float.NaN;
                        osc = 0.0f;
                        unch = float.NaN;
                        support = float.NaN;

                        this.Events[3][i] = true;// SupportBroken
                    }
                    else
                    {
                        fastEma += fastAlfa * (lowSerie[i] - fastEma);
                        slowEma += slowAlfa * (lowSerie[i] - slowEma);
                        osc = fastEma - slowEma;
                        unch = fastEma + osc;
                        support = slowEma - osc;
                    }
                }

                if (unch > fastEma)
                {
                    this.areas[0].UpLine[i] = unch;
                    this.areas[0].DownLine[i] = fastEma;
                }

                unchSerie[i] = unch;
                fastSerie[i] = fastEma;
                slowSerie[i] = slowEma;
                supportSerie[i] = support;
                this.Events[8][i] = !float.IsNaN(support); // Bullish
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