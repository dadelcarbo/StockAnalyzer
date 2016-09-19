using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_MFI : StockIndicatorBase, IRange
    {
        public StockIndicator_MFI()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.RangedIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "Overbought", "Oversold", "InputSmoothing" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 75f, 25f, 1 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 100f), new ParamRangeFloat(0f, 100f), new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { "MFI(" + this.Parameters[0].ToString() + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black) };
                }
                return seriePens;
            }
        }
        public override HLine[] HorizontalLines
        {
            get
            {
                HLine[] lines = new HLine[] { new HLine(50, new Pen(Color.LightGray)), new HLine((float)this.parameters[1], new Pen(Color.Gray)), new HLine((float)this.parameters[2], new Pen(Color.Gray)) };
                lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[1].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[2].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE).CalculateEMA((int)this.parameters[3]);
            FloatSerie volumeSerie = stockSerie.GetSerie(StockDataType.VOLUME);
            FloatSerie mfiSerie = new FloatSerie(stockSerie.Count);

            int period = (int)this.parameters[0];

            for (int i = period + 1; i < stockSerie.Count; i++)
            {
                float upFlow = 0f, downFlow = 0f;
                for (int j = 0; j < period; j++)
                {
                    if (closeSerie[i - j - 1] < closeSerie[i - j])
                    {
                        upFlow += volumeSerie[i - j] * closeSerie[i - j];
                    }
                    else
                    {
                        downFlow += volumeSerie[i - j] * closeSerie[i - j];
                    }
                }
                if (downFlow == 0)
                {
                    mfiSerie[i] = 100f;
                }
                else
                {
                    float ratio = upFlow / downFlow;
                    mfiSerie[i] = 100f - 100f / (1f + ratio);
                }
            }

            this.series[0] = mfiSerie;
            this.series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            float overbought = (float)this.parameters[1];
            float oversold = (float)this.parameters[2];

            for (int i = 2; i < mfiSerie.Count; i++)
            {
                this.eventSeries[0][i] = (mfiSerie[i - 2] < mfiSerie[i - 1] && mfiSerie[i - 1] > mfiSerie[i]);
                this.eventSeries[1][i] = (mfiSerie[i - 2] > mfiSerie[i - 1] && mfiSerie[i - 1] < mfiSerie[i]);
                this.eventSeries[2][i] = mfiSerie[i] >= overbought;
                this.eventSeries[3][i] = mfiSerie[i] <= oversold;
                this.eventSeries[4][i] = this.eventSeries[2][i - 1] && !this.eventSeries[2][i];
                this.eventSeries[5][i] = this.eventSeries[3][i - 1] && !this.eventSeries[3][i];
            }
        }

        public float Max
        {
            get { return 100.0f; }
        }

        public float Min
        {
            get { return 0.0f; }
        }

        static string[] eventNames = new string[] { "Top", "Bottom", "Overbought", "Oversold", "OutOfOverbought", "OutOfOversold" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, false, false, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
