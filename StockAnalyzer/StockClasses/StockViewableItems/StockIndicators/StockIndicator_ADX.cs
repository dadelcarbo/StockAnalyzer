using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ADX : StockIndicatorBase, IRange
    {
        public StockIndicator_ADX()
        {
        }

        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.RangedIndicator; }
        }
        public float Max
        {
            get { return 100.0f; }
        }

        public float Min
        {
            get { return 0.0f; }
        }

        public override string Definition
        {
            get { return "ADX(int Period, float Threshold, int Smoothing)"; }
        }
        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { 14, 25f, 3 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0.0f, 100.0f), new ParamRangeInt(1, 500) }; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "Threshold", "InputSmoothing" }; }
        }


        public override string[] SerieNames { get { return new string[] { "ADX(" + this.Parameters[0].ToString() + ")", "+DI", "-DI" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black), new Pen(Color.Green), new Pen(Color.Red) };
                }
                return seriePens;
            }
        }
        static HLine[] lines = null;
        public override HLine[] HorizontalLines
        {
            get
            {
                if (lines == null)
                {
                    lines = new HLine[] { new HLine((float)this.ParameterDefaultValues[1], new Pen(Color.DarkGray) { DashStyle = DashStyle.Dash }) };
                }
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Set HLine value
            float trendThreshold = (float)this.Parameters[1];

            int period = (int)this.Parameters[0];
            FloatSerie atrSerie = stockSerie.GetIndicator("ATR(" + period + ")").Series[0];

            FloatSerie pDM = new FloatSerie(stockSerie.Count);
            FloatSerie mDM = new FloatSerie(stockSerie.Count);

            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW).CalculateEMA((int)this.Parameters[2]);
            FloatSerie higherie = stockSerie.GetSerie(StockDataType.HIGH).CalculateEMA((int)this.Parameters[2]);

            // Calculate +DM and -DM
            for (int i = 1; i < stockSerie.Count; i++)
            {
                float rangeUp = higherie[i] - higherie[i - 1];
                float rangeDown = lowSerie[i - 1] - lowSerie[i];

                if (rangeUp > rangeDown)
                {
                    pDM[i] = Math.Max(0, rangeUp);
                }
                else
                {
                    mDM[i] = Math.Max(0, rangeDown);
                }
            }

            // Calclate +DI and -DI
            FloatSerie pDI = pDM.CalculateEMA(period).Div(atrSerie).Mult(100);
            FloatSerie mDI = mDM.CalculateEMA(period).Div(atrSerie).Mult(100);

            FloatSerie ADX = ((pDI - mDI).Abs() / (pDI + mDI)).CalculateEMA(period).Mult(100);

            ADX.Name = this.SerieNames[0];
            pDI.Name = this.SerieNames[1];
            mDI.Name = this.SerieNames[2];

            this.Series[0] = ADX;
            this.Series[1] = pDI;
            this.Series[2] = mDI;

            // Manage events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = period; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = ADX[i] > trendThreshold && ADX[i] > ADX[i - 1] && pDI[i] > mDI[i];
                this.eventSeries[1][i] = ADX[i] > trendThreshold && ADX[i] > ADX[i - 1] && pDI[i] < mDI[i];
            }

        }

        static string[] eventNames = new string[] { "UpTrend", "DownTrend" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
