﻿using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_FIBOCHANNEL : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;

        public override string[] ParameterNames => new string[] { "Period", "Ratio" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 0.75f };

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

            bool previousBullish = false;
            bool previousBearish = false;

            for (int i = period; i < stockSerie.Count; i++)
            {
                float close = closeSerie[i];
                bool bullish = close > midUpLine[i];
                this.Events[0][i] = bullish;
                this.Events[2][i] = bullish && !previousBullish;
                this.Events[4][i] = !bullish && previousBullish;
                previousBullish = bullish;

                bool bearish = close < midDownLine[i];
                this.Events[1][i] = bearish;
                this.Events[3][i] = bearish && !previousBearish;
                this.Events[5][i] = !bearish && previousBearish;
                previousBearish = bearish;
            }
        }

        private static readonly string[] eventNames = { "Bullish", "Bearish", "StartBullish", "StopBullish", "BrokenUp", "BrokenDown" };
        public override string[] EventNames => eventNames;

        static readonly bool[] isEvent = { false, false, true, true, true, true };

        public override bool[] IsEvent => isEvent;
    }
}