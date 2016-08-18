using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ER : StockIndicatorBase, IRange
    {
        public StockIndicator_ER()
        {
        }

        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.RangedIndicator; }
        }

        #region IRange Implementation
        public float Max
        {
            get { return 1.0f; }
        }

        public float Min
        {
            get { return -1.0f; }
        }
        #endregion

        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 1, 1, 0.7f }; }
        }

        public override ParamRange[] ParameterRanges
        {
            get
            {
                return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(0f,1f)  };
            }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "InputSmoothing", "Smoothing", "Overbought" }; }
        }

        public override string[] SerieNames
        {
            get { return new string[] { "ER(" + this.Parameters[0].ToString() + ")" }; }
        }

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

        public override HLine[] HorizontalLines
        {
            get
            {
                HLine[] lines = new HLine[] { new HLine(0, new Pen(Color.Gray)),
                    new HLine((float)this.Parameters[3], new Pen(Color.Gray)),
                    new HLine(-(float)this.Parameters[3], new Pen(Color.Gray))
                };

                lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = Math.Min((int)this.parameters[0], stockSerie.Count - 1);
            int inputSmoothing = (int)this.parameters[1];
            int smoothing = (int)this.parameters[2];
            FloatSerie erSerie = stockSerie.CalculateER(period, inputSmoothing).CalculateEMA(smoothing);
            this.series[0] = erSerie;
            this.Series[0].Name = this.Name;

            this.CreateEventSeries(stockSerie.Count);

            float overbought = (float)this.parameters[3];
            float oversold = -overbought;

            bool isOverSold = false;
            bool isOverBought = false;

            for (int i = period; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = erSerie[i] > 0;
                this.eventSeries[1][i] = erSerie[i] < 0;
                this.eventSeries[2][i] = erSerie[i] > 0 && erSerie[i - 1] < 0;
                this.eventSeries[3][i] = erSerie[i] < 0 && erSerie[i - 1] > 0;



                isOverSold = erSerie[i] <= oversold;
                isOverBought = erSerie[i] >= overbought;
                this.eventSeries[4][i] = isOverBought && erSerie[i] < erSerie[i - 1];
                this.eventSeries[5][i] = isOverSold && erSerie[i] > erSerie[i - 1];
                this.eventSeries[6][i] = (!isOverSold) && this.eventSeries[3][i - 1];
                this.eventSeries[7][i] = (!isOverBought) && this.eventSeries[2][i - 1];
            }
        }

        static string[] eventNames = new string[] { "Positive", "Negative", "TurnedPositive", "TurnedNegative", "Overbought", "Oversold", "OutOfOversold", "OutOfOverbought" };

        public override string[] EventNames
        {
            get { return eventNames; }
        }

        static readonly bool[] isEvent = new bool[] { false, false, true, true, false, false, true, true, };

        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
