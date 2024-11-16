using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TRIX : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 12 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };
        public override string[] SerieNames => new string[] { "TRIX(" + this.Parameters[0].ToString() + ")" };


        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Black) { DashStyle = System.Drawing.Drawing2D.DashStyle.Custom } };
                return seriePens;
            }
        }
        static HLine[] lines = null;
        public override HLine[] HorizontalLines
        {
            get
            {
                lines ??= new HLine[] { new HLine(0, new Pen(Color.LightGray)) };
                return lines;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.parameters[0];
            var ema3 = stockSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(period).CalculateEMA(period).CalculateEMA(period);

            var trixSerie = new FloatSerie(stockSerie.Count);
            for (int i = 1; i < stockSerie.Count; i++)
            {
                trixSerie[i] = (ema3[i] - ema3[i - 1]) / ema3[i - 1];
            }
            this.series[0] = trixSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var bodyHighSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);
            var bodyLowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);

            bool waitForFirstHighInBull = false;
            bool waitFirstLowInBear = false;
            float previousHigh = float.MinValue, previousLow = float.MaxValue;
            for (int i = 2; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = (trixSerie[i - 2] < trixSerie[i - 1] && trixSerie[i - 1] > trixSerie[i]);
                this.eventSeries[1][i] = (trixSerie[i - 2] > trixSerie[i - 1] && trixSerie[i - 1] < trixSerie[i]);
                this.eventSeries[2][i] = (trixSerie[i - 1] < 0 && trixSerie[i] >= 0);
                this.eventSeries[3][i] = (trixSerie[i - 1] > 0 && trixSerie[i] <= 0);
                this.eventSeries[4][i] = trixSerie[i] >= 0;
                this.eventSeries[5][i] = trixSerie[i] < 0;

                if (this.eventSeries[2][i]) // Turned positive
                {
                    waitForFirstHighInBull = true;
                    waitFirstLowInBear = false;
                    previousHigh = bodyHighSerie[i];
                }
                else if (waitForFirstHighInBull)
                {
                    if (closeSerie[i] > previousHigh)
                    {
                        this.eventSeries[6][i] = true;
                        waitForFirstHighInBull = false;
                    }
                }

                if (this.eventSeries[3][i]) // Turned negative
                {
                    waitFirstLowInBear = true;
                    waitForFirstHighInBull = false;
                    previousLow = bodyLowSerie[i];
                }
                else if (waitFirstLowInBear)
                {
                    if (closeSerie[i] < previousLow)
                    {
                        this.eventSeries[7][i] = true;
                        waitFirstLowInBear = false;
                    }
                }
            }
        }

        static readonly string[] eventNames = new string[] { "Top", "Bottom", "TurnedPositive", "TurnedNegative", "Bullish", "Bearish", "FirstHighInBull", "FirstLowInBear" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}