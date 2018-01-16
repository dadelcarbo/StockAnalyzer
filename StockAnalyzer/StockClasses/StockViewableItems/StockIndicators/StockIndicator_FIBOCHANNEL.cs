using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_FIBOCHANNEL : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;

        public override string[] ParameterNames => new string[] { "Period", "Ratio" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 0.5f };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 1f) };

        public override string[] SerieNames => new string[] { "HIGH", "FIBHIGH", "FIBLOW", "LOW" };

        public override System.Drawing.Pen[] SeriePens => seriePens ??
                                                          (seriePens =
                                                              new Pen[]
                                                              {
                                                                 new Pen(Color.DarkGreen), new Pen(Color.Black), new Pen(Color.Black), new Pen(Color.DarkRed)
                                                              });

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];
            float upRatio = (float)this.parameters[1];
            float downRatio = 1f - upRatio;

            // Calculate FIBOCHANNEL Channel
            FloatSerie upLine = new FloatSerie(stockSerie.Count);
            FloatSerie midUpLine = new FloatSerie(stockSerie.Count);
            FloatSerie midDownLine = new FloatSerie(stockSerie.Count);
            FloatSerie downLine = new FloatSerie(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            upLine[0] = closeSerie[0];
            downLine[0] = closeSerie[0];
            midUpLine[0] = closeSerie[0];
            midDownLine[0] = closeSerie[0];

            float up, down;
            for (int i = 1; i < stockSerie.Count; i++)
            {
                upLine[i] = up = highSerie.GetMax(Math.Max(0, i - period - 1), i - 1);
                downLine[i] = down = lowSerie.GetMin(Math.Max(0, i - period - 1), i - 1);
                midUpLine[i] = (up * upRatio + down * downRatio);
                midDownLine[i] = (down * upRatio + up * downRatio);
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

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            bool upTrend = true;

            for (int i = period; i < stockSerie.Count; i++)
            {
                count = 0;

                //if (upTrend)
                //{
                //    upTrend = midLine[i] >= midLine[i - 1];
                //}
                //else
                //{
                //    upTrend = midLine[i] > midLine[i - 1];
                //}

                //this.Events[count++][i] = upTrend;
                //this.Events[count++][i] = !upTrend;
                //this.Events[count++][i] = (!this.Events[0][i - 1]) && (this.Events[0][i]);
                //this.Events[count++][i] = (this.Events[0][i - 1]) && (!this.Events[0][i]);

                //this.Events[count++][i] = closeSerie[i] > upLine[i];
                //this.Events[count++][i] = closeSerie[i] > midLine[i];
                //this.Events[count++][i] = closeSerie[i] < midLine[i];
                //this.Events[count++][i] = closeSerie[i] < downLine[i];

                //this.Events[count++][i] = highSerie[i] > upLine[i];
                //this.Events[count++][i] = highSerie[i] > midLine[i];
                //this.Events[count++][i] = highSerie[i] < midLine[i];
                //this.Events[count++][i] = highSerie[i] < downLine[i];

                //this.Events[count++][i] = lowSerie[i] > upLine[i];
                //this.Events[count++][i] = lowSerie[i] > midLine[i];
                //this.Events[count++][i] = lowSerie[i] < midLine[i];
                //this.Events[count++][i] = lowSerie[i] < downLine[i];

                //this.Events[count++][i] = lowSerie[i - 1] <= midLine[i - 1] && lowSerie[i] > midLine[i];
                //this.Events[count++][i] = lowSerie[i - 1] <= downLine[i - 1] && lowSerie[i] > downLine[i];

                //this.Events[count++][i] = highSerie[i - 1] >= upLine[i - 1] && highSerie[i] < upLine[i];
                //this.Events[count][i] = highSerie[i - 1] >= midLine[i - 1] && highSerie[i] < midLine[i];
            }
        }

        private static string[] eventNames = new string[]
        {
            "Uptrend", "DownTrend", "BrokenUp", "BrokenDown",
            "CloseAboveUpLine", "CloseAboveMidLine", "CloseBelowMidLine", "CloseBelowLowLine",
            "HighAboveUpLine", "HighAboveMidLine", "HighBelowMidLine", "HighBelowLowLine",
            "LowAboveUpLine", "LowAboveMidLine", "LowBelowMidLine", "LowBelowLowLine",
            "TouchedDownMidLine", "TouchedDownLowLine",
            "TouchedUpUpLine", "TouchedUpMidLine",
        };
        public override string[] EventNames => eventNames;

        static readonly bool[] isEvent = new bool[]
         {
            false, false, true, true,
            false, false, false, false,
            false, false, false, false,
            false, false, false, false,
            true, true,
            true, true
         };
        public override bool[] IsEvent => isEvent;
    }
}
