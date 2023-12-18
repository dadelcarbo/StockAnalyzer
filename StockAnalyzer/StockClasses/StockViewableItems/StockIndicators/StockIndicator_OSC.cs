using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_OSC : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "FastPeriod", "SlowPeriod", "Percent", "MAType" };

        public override Object[] ParameterDefaultValues => new Object[] { 12, 26, true, "EMA" };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeBool(), new ParamRangeMA() };
        public override string[] SerieNames => new string[] { "OSC(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" };


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
            IStockIndicator fastSerie = stockSerie.GetIndicator($"{this.parameters[3]}({this.parameters[0]})");
            IStockIndicator slowSerie = stockSerie.GetIndicator($"{this.parameters[3]}({this.parameters[1]})");
            bool relative = (bool)this.parameters[2];

            FloatSerie oscSerie = fastSerie.Series[0].Sub(slowSerie.Series[0]);
            if (relative)
            {
                oscSerie = 100.0f * (oscSerie / slowSerie.Series[0]);
            }
            this.series[0] = oscSerie;
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
                this.eventSeries[0][i] = (oscSerie[i - 2] < oscSerie[i - 1] && oscSerie[i - 1] > oscSerie[i]);
                this.eventSeries[1][i] = (oscSerie[i - 2] > oscSerie[i - 1] && oscSerie[i - 1] < oscSerie[i]);
                this.eventSeries[2][i] = (oscSerie[i - 1] < 0 && oscSerie[i] >= 0);
                this.eventSeries[3][i] = (oscSerie[i - 1] > 0 && oscSerie[i] <= 0);
                this.eventSeries[4][i] = oscSerie[i] >= 0;
                this.eventSeries[5][i] = oscSerie[i] < 0;

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

        static string[] eventNames = new string[] { "Top", "Bottom", "TurnedPositive", "TurnedNegative", "Bullish", "Bearish", "FirstHighInBull", "FirstLowInBear" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
