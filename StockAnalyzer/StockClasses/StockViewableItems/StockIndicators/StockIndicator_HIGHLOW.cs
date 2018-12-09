using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_HIGHLOW : StockIndicatorBase, IRange
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.RangedIndicator; }
        }
        public float Max
        {
            get { return 1.1f; }
        }

        public float Min
        {
            get { return -1.1f; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "FastSmoothing", "SlowSmoothing", "OverBought", "OverSold" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 14, 3, 3, 0.5f, -0.5f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(-1f, 1f), new ParamRangeFloat(-1f, 1f) }; }
        }

        public override string[] SerieNames { get { return new string[] { "Fast(" + this.Parameters[1].ToString() + ")", "Slow(" + this.Parameters[2].ToString() + ")" }; } }

        public override Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red) };
                }
                return seriePens;
            }
        }

        public override HLine[] HorizontalLines
        {
            get
            {
                HLine[] lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)), new HLine(.5f, new Pen(Color.Gray)), new HLine(-.5f, new Pen(Color.Gray)) };
                lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[1].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[2].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.parameters[0];
            var fastSmooting = (int)this.parameters[1];
            var slowSmooting = (int)this.parameters[2];
            var overbought = (float)this.parameters[3];
            var oversold = (float)this.parameters[4];

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            var lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            FloatSerie raw = new FloatSerie(stockSerie.Count);

            float lowestLow = float.MaxValue;
            float highestHigh = float.MinValue;
            for (int i = 0; i < period; i++)
            {
                lowestLow = Math.Min(lowestLow, lowSerie[i]);
                highestHigh = Math.Min(highestHigh, highSerie[i]);

                var range = (highestHigh - lowestLow) / 2f;

                raw[i] = -1f + (closeSerie[i] - lowestLow) / range;
            }
            for (int i = period; i < stockSerie.Count; i++)
            {
                lowestLow = lowSerie.GetMin(i - period, i);
                highestHigh = highSerie.GetMax(i - period, i);
                if (highestHigh == lowestLow)
                {
                    raw[i] = 0.0f;
                }
                else
                {
                    var range = (highestHigh - lowestLow) / 2f;

                    raw[i] = -1f + (closeSerie[i] - lowestLow) / range;
                }
            }

            FloatSerie fastSerie = raw.CalculateEMA(fastSmooting);
            FloatSerie slowSerie = fastSerie.CalculateEMA(slowSmooting);
            this.series[0] = fastSerie;
            this.series[0].Name = this.SerieNames[0];
            this.series[1] = slowSerie;
            this.series[1].Name = this.SerieNames[1];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = period; i < fastSerie.Count; i++)
            {
                this.eventSeries[0][i] = (slowSerie[i - 1] > fastSerie[i - 1] && slowSerie[i] < fastSerie[i]);
                this.eventSeries[1][i] = (slowSerie[i - 1] < fastSerie[i - 1] && slowSerie[i] > fastSerie[i]);
                this.eventSeries[2][i] = fastSerie[i] > slowSerie[i];
                this.eventSeries[3][i] = fastSerie[i] < slowSerie[i];
                this.eventSeries[4][i] = fastSerie[i - 1] > overbought && fastSerie.IsTop(i - 1);
                this.eventSeries[5][i] = fastSerie[i - 1] < oversold && fastSerie.IsBottom(i - 1);
                float min = 0, max = 0;
                fastSerie.GetMinMax(i-period, i-1, ref min, ref max);
                this.eventSeries[6][i] = fastSerie.IsTop(i - 1) && fastSerie[i - 1] < max;
                this.eventSeries[7][i] = fastSerie.IsBottom(i - 1) && fastSerie[i - 1] > min;
            }
        }

        static string[] eventNames = new string[] { "BullishCrossing", "BearishCrossing", "Bullish", "Bearish", "OverboughtTop", "OversoldBottom", "LowerTop", "HigherLow" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, false, false, true, true, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
