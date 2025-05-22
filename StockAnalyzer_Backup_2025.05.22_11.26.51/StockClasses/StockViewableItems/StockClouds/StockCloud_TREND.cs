using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockClouds
{
    public class StockCloud_TREND : StockCloudBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string[] ParameterNames => new string[] { "Period" };
        public override string Definition => "Paint cloud based on HL";

        public override Object[] ParameterDefaultValues => new Object[] { 50 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };
        public override string[] SerieNames => new string[] { "Bull", "Bear", "Mid" };
        public override void ApplyTo(StockSerie stockSerie)
        {
            var fastPeriod = (int)this.parameters[0];

            var lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            var highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var bullSerie = new FloatSerie(stockSerie.Count);
            var bearSerie = new FloatSerie(stockSerie.Count);

            var high = bullSerie[0] = bullSerie[1] = Math.Max(highSerie[0], highSerie[1]);
            var low = bearSerie[0] = bearSerie[1] = Math.Min(lowSerie[0], lowSerie[1]);
            int i = 2;
            bool broken = false;
            bool bullish = false;
            while (i < stockSerie.Count && !broken) // Prepare trend
            {
                if (closeSerie[i] > high) // Broken up
                {
                    bullish = true;
                    broken = true;
                    high = bullSerie[i] = highSerie[i - 1];
                    low = bearSerie[i] = lowSerie[i - 1];
                }
                else if (closeSerie[i] < low) // Broken Down
                {
                    bullish = false;
                    broken = true;
                    high = bullSerie[i] = highSerie[i - 1];
                    low = bearSerie[i] = lowSerie[i - 1];
                }
                else
                {
                    bullSerie[i] = bullSerie[i - 1];
                    bearSerie[i] = bearSerie[i - 1];
                    i++;
                }
            }

            bool waitForText = false;
            for (; i < stockSerie.Count; i++)
            {
                if (bullish)
                {
                    if (closeSerie[i] < low) // Bull run broken
                    {
                        bullish = false;
                        low = lowSerie[i];
                        bullSerie[i] = low;
                        bearSerie[i] = high;
                    }
                    else // Bull run continues
                    {
                        if (highSerie[i] > high)
                        {
                            waitForText = true;
                        }
                        else if (waitForText && highSerie[i - 1] > highSerie[i - 2] && highSerie[i] < highSerie[i - 1])
                        {
                            this.stockTexts.Add(new StockText
                            {
                                AbovePrice = true,
                                Index = i - 1,
                                Text = "Top"
                            });
                            waitForText = false;
                        }
                        low = Math.Max(low, lowSerie.GetMin(Math.Max(0, i - fastPeriod), i));
                        high = Math.Max(high, highSerie[i]);
                        bullSerie[i] = high;
                        bearSerie[i] = low;
                    }
                }
                else
                {
                    if (closeSerie[i] > high) // Bear run broken
                    {
                        bullish = true;
                        high = highSerie[i];
                        bullSerie[i] = high;
                        bearSerie[i] = low;
                    }
                    else // Bear run continues
                    {
                        if (lowSerie[i] < low)
                        {
                            waitForText = true;
                        }
                        else if (waitForText && lowSerie[i - 1] < lowSerie[i - 2] && lowSerie[i] > lowSerie[i - 1])
                        {
                            this.stockTexts.Add(new StockText
                            {
                                AbovePrice = false,
                                Index = i - 1,
                                Text = "Bot"
                            });
                            waitForText = false;
                        }
                        low = Math.Min(low, lowSerie[i]);
                        high = Math.Min(high, highSerie.GetMax(Math.Max(0, i - fastPeriod), i));
                        bullSerie[i] = low;
                        bearSerie[i] = high;
                    }
                }
            }

            this.Series[0] = bullSerie;
            this.Series[1] = bearSerie;
            this.Series[2] = (bullSerie + bearSerie) / 2.0f;

            // Detecting events
            this.GenerateEvents(stockSerie);
        }
    }
}
