using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ADX : StockIndicatorBase, IRange
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.RangedIndicator;
        public float Max => 100.0f;

        public float Min => 0.0f;

        public override string Definition => "ADX(int Period, float Threshold, int Smoothing)";
        public override object[] ParameterDefaultValues => new Object[] { 14, 25f, 3 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0.0f, 100.0f), new ParamRangeInt(1, 500) };
        public override string[] ParameterNames => new string[] { "Period", "Threshold", "InputSmoothing" };


        public override string[] SerieNames => new string[] { "ADX(" + this.Parameters[0].ToString() + ")", "+DI", "-DI" };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Black), new Pen(Color.Green), new Pen(Color.Red) };
                return seriePens;
            }
        }

        public override HLine[] HorizontalLines
        {
            get
            {
                lines ??= new HLine[] { new HLine((float)this.ParameterDefaultValues[1], new Pen(Color.DarkGray) { DashStyle = DashStyle.Dash }) };
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
                var bullish = pDI[i] > mDI[i];
                this.eventSeries[0][i] = ADX[i] > trendThreshold && ADX[i] > ADX[i - 1] && bullish;
                this.eventSeries[1][i] = ADX[i] > trendThreshold && ADX[i] > ADX[i - 1] && !bullish;
                this.eventSeries[2][i] = ADX[i] > trendThreshold && ADX[i - 1] < trendThreshold && bullish;
                this.eventSeries[3][i] = ADX[i] > trendThreshold && ADX[i - 1] < trendThreshold && !bullish;
            }
        }

        static readonly string[] eventNames = new string[] { "Bullish", "Bearish", "UpTrend", "DownTrend" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { false, false, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
