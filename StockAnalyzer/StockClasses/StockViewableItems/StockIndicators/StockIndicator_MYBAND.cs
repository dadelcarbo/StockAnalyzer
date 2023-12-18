using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_MYBAND : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string Definition => "MYBAND(int Period, float NbUpDev, float NbDownDev)";
        public override string[] ParameterNames => new string[] { "Period", "NbUpDev", "NbDownDev" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 2.0f, -2.0f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(-5.0f, 20.0f), new ParamRangeFloat(-20.0f, 5.0f) };

        public override string[] SerieNames => new string[] { "MYBANDUp", "MYBANDDown", "EMA" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Blue), new Pen(Color.Blue), new Pen(Color.Blue) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Calculate Bands
            int period = (int)this.parameters[0];
            FloatSerie ema = stockSerie.GetIndicator("EMA(" + period + ")").Series[0];
            FloatSerie diff = ema * 0.1f;

            float upCoef = (float)this.parameters[1];
            float downCoef = (float)this.parameters[2];

            FloatSerie upperBand = ema + diff * upCoef;
            this.series[0] = upperBand;
            this.Series[0].Name = this.SerieNames[0];

            FloatSerie lowerBand = ema + diff * downCoef;
            this.series[1] = lowerBand;
            this.Series[1].Name = this.SerieNames[1];

            this.series[2] = ema;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            bool upTrend = closeSerie[period - 1] > upperBand[period - 1];

            for (int i = period; i < upperBand.Count; i++)
            {
                if (upTrend && closeSerie[i] < lowerBand[i])
                {
                    // BearishSignal
                    this.eventSeries[1][i] = true;
                    upTrend = false;
                }
                if (!upTrend && closeSerie[i] > upperBand[i])
                {
                    upTrend = true;
                    this.eventSeries[0][i] = true;
                }
            }
        }

        static string[] eventNames = new string[] { "BrokenUp", "BrokenDown" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true };
        public override bool[] IsEvent => isEvent;
    }
}
