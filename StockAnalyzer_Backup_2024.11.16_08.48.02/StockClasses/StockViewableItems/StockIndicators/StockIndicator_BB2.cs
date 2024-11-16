using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_BB2 : StockIndicatorBase
    {
        public StockIndicator_BB2()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string Definition
        {
            get { return "BB2(int Period)"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get
            {
                return new ParamRange[]
                {
                new ParamRangeInt(1, 500)
                };
            }
        }

        public override string[] SerieNames { get { return new string[] { "BBUp", "BBDown", "MA(" + (int)this.parameters[0] + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Blue), new Pen(Color.Blue), new Pen(Color.Blue) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Calculate Bollinger Bands
            FloatSerie upperBB = new FloatSerie(stockSerie.Count);
            FloatSerie lowerBB = new FloatSerie(stockSerie.Count);

            int period = (int)this.parameters[0];
            var coef = (float)Math.Sqrt(period);

            var emaIndicator = stockSerie.GetIndicator("MA(" + period + ")").Series[0];
            var variance = stockSerie.GetSerie(StockDataType.CLOSE).CalculateVariance(period);

            //for (int i = 0; i < stockSerie.Count; i++)
            //{
            //    var ma = emaIndicator[i];
            //    var offset = coef * variance[i];
            //    upperBB[i] = ma + offset;
            //    lowerBB[i] = ma - offset;
            //}
            for (int i = stockSerie.Count - 1; i > period; i--)
            {
                var ma = emaIndicator[i - period];
                var offset = coef * variance[i - period];
                upperBB[i] = ma + offset;
                lowerBB[i] = ma - offset;
            }
            for (int i = 0; i <= period; i++)
            {
                upperBB[i] = emaIndicator[i];
                lowerBB[i] = emaIndicator[i];
            }

            this.series[0] = upperBB;
            this.Series[0].Name = this.SerieNames[0];

            this.series[1] = lowerBB;
            this.Series[1].Name = this.SerieNames[1];

            this.series[2] = emaIndicator;
            this.Series[2].Name = this.SerieNames[2];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);

            bool waitingForBearSignal = false;
            bool waitingForBullSignal = false;

            for (int i = 1; i < upperBB.Count; i++)
            {
                if (waitingForBearSignal && highSerie[i - 1] >= highSerie[i] && closeSerie[i - 1] >= closeSerie[i])
                {
                    // BearishSignal
                    this.eventSeries[3][i] = true;
                    waitingForBearSignal = false;
                }
                if (highSerie[i] >= upperBB[i])
                {
                    waitingForBearSignal = true;
                    this.eventSeries[0][i] = true;
                }
                if (waitingForBullSignal && lowSerie[i - 1] <= lowSerie[i] && closeSerie[i - 1] <= closeSerie[i])
                {
                    // BullishSignal
                    this.eventSeries[2][i] = true;
                    waitingForBullSignal = false;
                }
                if (lowSerie[i] <= lowerBB[i])
                {
                    waitingForBullSignal = true;
                    this.eventSeries[1][i] = lowSerie[i] <= lowerBB[i];
                }
            }
        }

        static string[] eventNames = new string[] { "UpBandOvershot", "DownBandOvershot", "BullishSignal", "BearishSignal" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
