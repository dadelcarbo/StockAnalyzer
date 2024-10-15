using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockClouds
{
    public class StockCloud_TRENDBODY2 : StockCloudBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string[] ParameterNames => new string[] { "UpPeriod", "DownPeriod" };
        public override string Definition => "Paint cloud based on HL with different up and down periods";

        public override Object[] ParameterDefaultValues => new Object[] { 50, 25 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };
        public override string[] SerieNames => new string[] { "Bull", "Bear", "Mid" };
        public override void ApplyTo(StockSerie stockSerie)
        {
            var upPeriod = (int)this.parameters[0];
            var downPeriod = (int)this.parameters[1];

            var bodyHighSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);
            var bodyLowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var bullSerie = new FloatSerie(stockSerie.Count);
            var bearSerie = new FloatSerie(stockSerie.Count);

            var high = bullSerie[0] = bullSerie[1] = Math.Max(bodyHighSerie[0], bodyHighSerie[1]);
            var low = bearSerie[0] = bearSerie[1] = Math.Min(bodyLowSerie[0], bodyLowSerie[1]);
            int i = 2;
            bool broken = false;
            bool bullish = false;
            while (!broken && i < stockSerie.Count) // Prepare trend
            {
                if (closeSerie[i] > high) // Broken up
                {
                    bullish = true;
                    broken = true;
                    high = bullSerie[i] = bodyHighSerie[i - 1];
                    low = bearSerie[i] = bodyLowSerie[i - 1];
                }
                else if (closeSerie[i] < low) // Broken Down
                {
                    bullish = false;
                    broken = true;
                    high = bullSerie[i] = bodyHighSerie[i - 1];
                    low = bearSerie[i] = bodyLowSerie[i - 1];
                }
                else
                {
                    bullSerie[i] = bullSerie[i - 1];
                    bearSerie[i] = bearSerie[i - 1];
                    i++;
                }
            }

            for (; i < stockSerie.Count; i++)
            {
                if (bullish)
                {
                    if (closeSerie[i] < low) // Bull run broken
                    {
                        bullish = false;
                        low = bodyLowSerie[i];
                        bullSerie[i] = low;
                        bearSerie[i] = high;
                    }
                    else // Bull run continues
                    {
                        low = Math.Max(low, bodyLowSerie.GetMin(Math.Max(0, i - downPeriod), i));
                        high = Math.Max(high, bodyHighSerie[i]);
                        bullSerie[i] = high;
                        bearSerie[i] = low;
                    }
                }
                else
                {
                    if (closeSerie[i] > high) // Bear run broken
                    {
                        bullish = true;
                        high = bodyHighSerie[i];
                        bullSerie[i] = high;
                        bearSerie[i] = low;
                    }
                    else // Bear run continues
                    {
                        low = Math.Min(low, bodyLowSerie[i]);
                        high = Math.Min(high, bodyHighSerie.GetMax(Math.Max(0, i - upPeriod), i));
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
