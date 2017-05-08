using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TRUE : StockIndicatorBase, IRange
    {
        public StockIndicator_TRUE()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.RangedIndicator; }
        }

        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { 1 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "NbBars" }; }
        }

        public override string[] SerieNames { get { return new string[] { "TRUE()" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Blue) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie TRUESerie = new FloatSerie(stockSerie.Count);
            this.series[0] = TRUESerie;
            this.series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            int i = 0;
            int nbBar = (int)this.parameters[0];
            float max = float.MinValue;
            float min = float.MaxValue;
            foreach (StockDailyValue value in stockSerie.Values)
            {
                this.eventSeries[0][i] = true;
                this.eventSeries[1][i] = false;

                if (i > nbBar)
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

        static string[] eventNames = new string[] { "True", "False", "HigherClose", "LowerClose", "RecHigherClose", "RecLowerClose", "EndOfHigherClose", "EndOfLowerClose", "BarComplete", "AllTimeHigh", "AllTimeLow" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false, true, true, false, false, true, true, false, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }

        public float Max
        {
            get { return 1.0f; }
        }

        public float Min
        {
            get { return -1.0f; }
        }
    }
}
