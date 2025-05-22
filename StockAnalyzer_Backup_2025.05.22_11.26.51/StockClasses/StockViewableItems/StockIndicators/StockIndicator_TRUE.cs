using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TRUE : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;

        public override object[] ParameterDefaultValues => new Object[] { 3 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };
        public override string[] ParameterNames => new string[] { "NbBars" };

        public override string[] SerieNames => new string[] { };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            int i = 0;
            int nbBar = (int)this.parameters[0];
            float max = float.MinValue;
            float min = float.MaxValue;
            float previousHigh = float.MinValue;
            float previousLow = float.MaxValue;
            int higherLowIndex = Array.IndexOf(this.EventNames, "HigherLow");
            int lowerHighIndex = Array.IndexOf(this.EventNames, "LowerHigh");
            foreach (StockDailyValue value in stockSerie.Values)
            {
                this.eventSeries[0][i] = true;
                this.eventSeries[1][i] = false;

                if (i > nbBar && value.IsComplete)
                {
                    this.eventSeries[2][i] = value.VARIATION > 0f;
                    this.eventSeries[3][i] = value.VARIATION < 0f;

                    if (this.eventSeries[4][i - 1])
                    {
                        if (this.eventSeries[2][i])
                        {
                            // New RecHigherClose
                            this.eventSeries[4][i] = true;
                        }
                        else
                        {
                            // EndOfHigherClose
                            this.eventSeries[6][i] = true;
                            if (value.HIGH < previousHigh)
                            {
                                this.eventSeries[lowerHighIndex][i] = true; // LowerHigh

                                this.stockTexts.Add(new StockText
                                {
                                    AbovePrice = true,
                                    Index = i - 1,
                                    Text = "LH"
                                });
                            }
                            previousHigh = value.HIGH;
                        }
                    }
                    else
                    {
                        this.eventSeries[4][i] = true;
                        for (int j = i - nbBar + 1; j <= i; j++)
                        {
                            this.eventSeries[4][i] &= this.eventSeries[2][j];
                        }
                    }
                    if (this.eventSeries[5][i - 1])
                    {
                        if (this.eventSeries[3][i])
                        {
                            // New RecLowerClose
                            this.eventSeries[5][i] = true;
                        }
                        else
                        {
                            // EndOfLowerClose
                            this.eventSeries[7][i] = true;
                            if (value.LOW > previousLow)
                            {
                                this.eventSeries[higherLowIndex][i] = true; // HigherLow

                                this.stockTexts.Add(new StockText
                                {
                                    AbovePrice = false,
                                    Index = i - 1,
                                    Text = "HL"
                                });
                            }
                            previousLow = value.LOW;
                        }
                    }
                    else
                    {
                        this.eventSeries[5][i] = true;
                        for (int j = i - nbBar + 1; j <= i; j++)
                        {
                            this.eventSeries[5][i] &= this.eventSeries[3][j];
                        }
                    }

                    this.eventSeries[9][i] = value.CLOSE > max;
                    this.eventSeries[10][i] = value.CLOSE < min;
                    max = Math.Max(max, value.CLOSE);
                    min = Math.Min(min, value.CLOSE);
                }
                this.eventSeries[8][i] = value.IsComplete;
                i++;
            }
        }

        static readonly string[] eventNames = new string[] {
            "True", "False",
            "HigherClose", "LowerClose",
            "RecHigherClose", "RecLowerClose",
            "EndOfHigherClose", "EndOfLowerClose",
            "BarComplete",
            "AllTimeHigh", "AllTimeLow",
            "HigherLow", "LowerHigh"
        };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { false, false, true, true, false, false, true, true, false, true, true, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
