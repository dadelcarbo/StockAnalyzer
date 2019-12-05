using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_FIBOCHANNEL : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;

        public override string[] ParameterNames => new string[] { "Period", "Ratio" };

        public override Object[] ParameterDefaultValues => new Object[] { 60, 1.0f };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 5f) };

        public override string[] SerieNames => new string[] { "HIGH", "FIBHIGH", "FIBLOW", "LOW", "MID" };

        public override System.Drawing.Pen[] SeriePens => seriePens ??
                                                          (seriePens =
                                                              new Pen[]
                                                              {
                                                                 new Pen(Color.DarkGreen), new Pen(Color.Black), new Pen(Color.Black), new Pen(Color.DarkRed), new Pen(Color.Black) {DashStyle = DashStyle.Dash}
                                                              });

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];
            float upRatio = (float)this.parameters[1];
            float downRatio = 1f - upRatio;

            // Calculate FIBOCHANNEL Channel
            FloatSerie upLine = new FloatSerie(stockSerie.Count);
            FloatSerie midUpLine = new FloatSerie(stockSerie.Count);
            FloatSerie midLine = new FloatSerie(stockSerie.Count);
            FloatSerie midDownLine = new FloatSerie(stockSerie.Count);
            FloatSerie downLine = new FloatSerie(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            float up, down;
            upLine[0] = up = highSerie[0];
            downLine[0] = down = lowSerie[0];
            midUpLine[0] = (up * upRatio + down * downRatio);
            midDownLine[0] = (down * upRatio + up * downRatio);
            midLine[0] = (up + down) / 2.0f;

            for (int i = 1; i < stockSerie.Count; i++)
            {
                upLine[i] = up = highSerie.GetMax(Math.Max(0, i - period - 1), i - 1);
                downLine[i] = down = lowSerie.GetMin(Math.Max(0, i - period - 1), i - 1);
                midUpLine[i] = (up * upRatio + down * downRatio);
                midDownLine[i] = (down * upRatio + up * downRatio);
                midLine[i] = (up + down) / 2.0f;
            }

            int count = 0;
            this.series[count] = upLine;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = midUpLine;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = midDownLine;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = downLine;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = midLine;
            this.Series[count].Name = this.SerieNames[count];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            bool upTrend = true;

            bool previousBullish = false;
            bool previousBearish = false;
            bool bullish = false;
            bool bearish = false;

            for (int i = 1; i < stockSerie.Count; i++)
            {
                float close = closeSerie[i];
                if (bullish)
                {
                    if (highSerie[i] < midLine[i])
                    {
                        bullish = false;
                    }
                }
                else
                {
                    if (closeSerie[i] > midUpLine[i])
                    {
                        bullish = true;
                    }
                }

                if (bearish)
                {
                    if (lowSerie[i] > midLine[i])
                    {
                        bearish = false;
                    }
                }
                else
                {
                    if (closeSerie[i] < midDownLine[i])
                    {
                        bearish = true;
                    }
                }

                //bool bullish = close > midUpLine[i];
                this.Events[0][i] = bullish;
                this.Events[2][i] = bullish && !previousBullish;
                this.Events[3][i] = !bullish && previousBullish;
                previousBullish = bullish;

                // bool bearish = close < midDownLine[i];
                this.Events[1][i] = bearish;
                this.Events[4][i] = bearish && !previousBearish;
                this.Events[5][i] = !bearish && previousBearish;
                previousBearish = bearish;
            }
        }

        private static readonly string[] eventNames = { "Bullish", "Bearish", "BullStart", "BullStop", "BearStart", "BearStop" };
        public override string[] EventNames => eventNames;

        static readonly bool[] isEvent = { false, false, true, true, true, true };

        public override bool[] IsEvent => isEvent;
    }
}
