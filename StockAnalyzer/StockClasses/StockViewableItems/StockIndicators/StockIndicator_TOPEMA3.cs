using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TOPEMA3 : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draws EMA supports and resistance from detected bottom with different periods, detecting bottom from smoothing EMA price cross over";

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;

        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.SimpleCurve;

        public override string[] ParameterNames => new string[] { "FastPeriod", "MidPeriod", "SlowPeriod", "Smoothing" };

        public override Object[] ParameterDefaultValues => new Object[] { 35, 75, 175, 6 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "Slow", "Mid", "Fast", "Trigger" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Blue), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.DarkRed) };
        public override Area[] Areas => areas ??= new Area[]
            {
                new Area {Name="Bull", Color = Color.FromArgb(64, Color.Green), Visibility = true }
            };

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);

            FloatSerie fastSupportSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);
            FloatSerie midSupportSerie = new FloatSerie(stockSerie.Count, this.SerieNames[1], float.NaN);
            FloatSerie slowSupportSerie = new FloatSerie(stockSerie.Count, this.SerieNames[2], float.NaN);

            this.Areas[0].UpLine = new FloatSerie(stockSerie.Count, float.NaN);
            this.Areas[0].DownLine = new FloatSerie(stockSerie.Count, float.NaN);

            int fastPeriod = (int)this.parameters[0];
            int midPeriod = (int)this.parameters[1];
            int slowPeriod = (int)this.parameters[2];

            float fastAlfa = 2.0f / (float)(fastPeriod + 1);
            float midAlfa = 2.0f / (float)(midPeriod + 1);
            float slowAlfa = 2.0f / (float)(slowPeriod + 1);

            int smoothingPeriod = (int)this.parameters[3];

            FloatSerie emaSerie = stockSerie.GetIndicator($"EMA({smoothingPeriod})").Series[0];

            this.Series[0] = fastSupportSerie;
            this.Series[0].Name = this.SerieNames[0];
            this.Series[1] = midSupportSerie;
            this.Series[1].Name = this.SerieNames[1];
            this.Series[2] = slowSupportSerie;
            this.Series[2].Name = this.SerieNames[2];
            this.Series[3] = emaSerie;
            this.Series[3].Name = this.SerieNames[3];

            if (this.Parameters.Cast<int>().Max() >= stockSerie.Count)
                return;

            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            float fastEma = float.NaN, midEma = float.NaN, slowEma = float.NaN;
            bool isBullish = false;
            float bottom = lowSerie[0];
            float previousBottom = lowSerie[0];

            // Detecting events
            for (int i = 1; i < stockSerie.Count; i++)
            {
                if (isBullish)
                {
                    if (closeSerie[i] < slowEma)
                    {
                        this.Events[3][i] = true;// SupportBroken
                        isBullish = false;
                        bottom = lowSerie[i];
                        fastEma = midEma = slowEma = float.NaN;
                    }
                    else
                    {
                        fastEma = Math.Max(fastEma, fastEma + fastAlfa * (lowSerie[i] - fastEma));
                        midEma = Math.Max(midEma, midEma + midAlfa * (lowSerie[i] - midEma));
                        slowEma = Math.Max(slowEma, slowEma + slowAlfa * (lowSerie[i] - slowEma));
                        //fastEma = fastEma + fastAlfa * (lowSerie[i] - fastEma);
                        //midEma = midEma + midAlfa * (lowSerie[i] - midEma);
                        //slowEma = slowEma + slowAlfa * (lowSerie[i] - slowEma);


                        if (fastEma > midEma)
                        {
                            this.areas[0].UpLine[i] = fastEma;
                            this.areas[0].DownLine[i] = midEma;
                        }

                    }
                }
                else
                {
                    if (closeSerie[i] > emaSerie[i] && closeSerie[i - 1] < emaSerie[i - 1])
                    {
                        isBullish = true;
                        fastEma = midEma = slowEma = bottom;
                        this.Events[0][i] = true; // SupportDetected
                        if (previousBottom < bottom)
                        {
                            this.Events[6][i] = true; // HigerLow
                            this.StockTexts.Add(new StockText { AbovePrice = false, Index = i - 1, Text = "HL" });
                        }
                        previousBottom = bottom;
                    }
                    else
                    {
                        bottom = Math.Min(lowSerie[i], bottom);
                    }
                }
                fastSupportSerie[i] = fastEma;
                midSupportSerie[i] = midEma;
                slowSupportSerie[i] = slowEma;

                this.Events[8][i] = isBullish; // Bullish
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