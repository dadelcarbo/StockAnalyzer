using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_BBMA : StockIndicatorBase
    {
        public StockIndicator_BBMA()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string[] ParameterNames => new string[] { "Period1", "Period2" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 3 };
        public override ParamRange[] ParameterRanges => new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeInt(1, 500)
                };

        public override string[] SerieNames => new string[] { "BBMAUp", "BBMADown", "EMA(" + (int)this.parameters[0] + ")" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Blue) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Calculate Bollinger Bands
            FloatSerie upperBB = new FloatSerie(stockSerie.Count);
            FloatSerie lowerBB = new FloatSerie(stockSerie.Count);

            int period1 = (int)this.parameters[0];
            int period2 = (int)this.parameters[1];

            FloatSerie emaSerie = stockSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(period1);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW).CalculateEMA(period2);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH).CalculateEMA(period2);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var distHigh = highSerie - emaSerie;
            var distLow = emaSerie - lowSerie;

            for (int i = period1; i < stockSerie.Count; i++)
            {
                var width = Math.Max(distHigh.GetMax(i - period1, i), distLow.GetMax(i - period1, i));

                upperBB[i] = emaSerie[i] + width;
                lowerBB[i] = emaSerie[i] - width;
            }

            this.series[0] = upperBB;
            this.Series[0].Name = this.SerieNames[0];

            this.series[1] = lowerBB;
            this.Series[1].Name = this.SerieNames[1];

            this.series[2] = emaSerie;
            this.Series[2].Name = this.SerieNames[2];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            // Detecting events
            bool bullish = false;
            bool bearish = false;

            for (int i = (int)this.parameters[0]; i < closeSerie.Count; i++)
            {
                if (bullish)
                {
                    // Check if uptrend broken
                    if (closeSerie[i] < emaSerie[i])
                    {
                        this.eventSeries[4][i] = true; // EndOfBull
                        bullish = false;
                    }
                }
                else if (bearish)
                {
                    // Check if downtrend broken
                    if (closeSerie[i] > emaSerie[i])
                    {
                        this.eventSeries[5][i] = true; // EndOfBear
                        bearish = false;
                    }
                }
                else
                {
                    // Check if new trend starting
                    if (closeSerie[i] > upperBB[i])
                    {
                        this.eventSeries[0][i] = bullish = true;
                        bearish = false;
                    }
                    else if (closeSerie[i] < lowerBB[i])
                    {
                        this.eventSeries[1][i] = bearish = true;
                        bullish = false;
                    }
                }

                this.eventSeries[2][i] = bullish;
                this.eventSeries[3][i] = bearish;
            }
        }

        static string[] eventNames = new string[] { "NewHigh", "NewLow", "Bullish", "Bearish", "EndOfBull", "EndOfBear" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, false, false, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
