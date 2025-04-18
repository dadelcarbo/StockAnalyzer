﻿using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_DONCHIAN : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string[] ParameterNames => new string[] { "Period" };
        public override Object[] ParameterDefaultValues => new Object[] { 60 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "DONCHIANUp", "DONCHIANMidUp", "DONCHIANMid", "DONCHIANMidDown", "DONCHIANDown" };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.DarkGreen), new Pen(Color.DarkGreen), new Pen(Color.Black), new Pen(Color.DarkRed), new Pen(Color.DarkRed) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];

            // Calculate Donchian Channel
            FloatSerie upLine = new FloatSerie(stockSerie.Count);
            FloatSerie midUpLine = new FloatSerie(stockSerie.Count);
            FloatSerie midLine = new FloatSerie(stockSerie.Count);
            FloatSerie midDownLine = new FloatSerie(stockSerie.Count);
            FloatSerie downLine = new FloatSerie(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            var lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            upLine[0] = closeSerie[0];
            downLine[0] = closeSerie[0];
            midLine[0] = closeSerie[0];
            midUpLine[0] = closeSerie[0];
            midDownLine[0] = closeSerie[0];

            for (int i = 0; i < stockSerie.Count; i++)
            {
                upLine[i] = highSerie.GetMax(Math.Max(0, i - period), i);
                downLine[i] = lowSerie.GetMin(Math.Max(0, i - period), i);
                midLine[i] = (upLine[i] + downLine[i]) / 2.0f;
                midUpLine[i] = (upLine[i] + midLine[i]) / 2.0f;
                midDownLine[i] = (midLine[i] + downLine[i]) / 2.0f;
            }

            int count = 0;
            this.series[count] = upLine;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = midUpLine;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = midLine;
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

                if (upTrend)
                {
                    upTrend = midLine[i] >= midLine[i - 1];
                }
                else
                {
                    upTrend = midLine[i] > midLine[i - 1];
                }

                this.Events[count++][i] = upTrend;
                this.Events[count++][i] = !upTrend;
                this.Events[count++][i] = (!this.Events[0][i - 1]) && (this.Events[0][i]);
                this.Events[count++][i] = (this.Events[0][i - 1]) && (!this.Events[0][i]);

                this.Events[count++][i] = closeSerie[i] > upLine[i];
                this.Events[count++][i] = closeSerie[i] > midUpLine[i];
                this.Events[count++][i] = closeSerie[i] > midLine[i];
                this.Events[count++][i] = closeSerie[i] < midLine[i];
                this.Events[count++][i] = closeSerie[i] < midDownLine[i];
                this.Events[count++][i] = closeSerie[i] < downLine[i];

                this.Events[count++][i] = highSerie[i] > upLine[i];
                this.Events[count++][i] = highSerie[i] > midUpLine[i];
                this.Events[count++][i] = highSerie[i] > midLine[i];
                this.Events[count++][i] = highSerie[i] < midLine[i];
                this.Events[count++][i] = highSerie[i] < midDownLine[i];
                this.Events[count++][i] = highSerie[i] < downLine[i];

                this.Events[count++][i] = lowSerie[i] > upLine[i];
                this.Events[count++][i] = lowSerie[i] > midUpLine[i];
                this.Events[count++][i] = lowSerie[i] > midLine[i];
                this.Events[count++][i] = lowSerie[i] < midLine[i];
                this.Events[count++][i] = lowSerie[i] < midDownLine[i];
                this.Events[count++][i] = lowSerie[i] < downLine[i];

                this.Events[count++][i] = lowSerie[i - 1] <= midUpLine[i - 1] && lowSerie[i] > midUpLine[i];
                this.Events[count++][i] = lowSerie[i - 1] <= midLine[i - 1] && lowSerie[i] > midLine[i];
                this.Events[count++][i] = lowSerie[i - 1] <= midDownLine[i - 1] && lowSerie[i] > midDownLine[i];
                this.Events[count++][i] = lowSerie[i - 1] <= downLine[i - 1] && lowSerie[i] > downLine[i];

                this.Events[count++][i] = highSerie[i - 1] >= upLine[i - 1] && highSerie[i] < upLine[i];
                this.Events[count++][i] = highSerie[i - 1] >= midUpLine[i - 1] && highSerie[i] < midUpLine[i];
                this.Events[count++][i] = highSerie[i - 1] >= midLine[i - 1] && highSerie[i] < midLine[i];
                this.Events[count++][i] = highSerie[i - 1] >= midDownLine[i - 1] && highSerie[i] < midDownLine[i];
            }
        }

        static readonly string[] eventNames = new string[]
          {
            "Uptrend", "DownTrend", "BrokenUp","BrokenDown",
            "CloseAboveUpLine", "CloseAboveMidUpLine", "CloseAboveMidLine", "CloseBelowMidLine", "CloseBelowMidLowLine", "CloseBelowLowLine",
            "HighAboveUpLine", "HighAboveMidUpLine", "HighAboveMidLine", "HighBelowMidLine", "HighBelowMidLowLine", "HighBelowLowLine",
            "LowAboveUpLine", "LowAboveMidUpLine", "LowAboveMidLine", "LowBelowMidLine", "LowBelowMidLowLine", "LowBelowLowLine",
            "TouchedDownMidUpLine", "TouchedDownMidLine", "TouchedDownMidLowLine", "TouchedDownLowLine",
            "TouchedUpUpLine",   "TouchedUpMidUpLine", "TouchedUpMidLine", "TouchedUpMidLowLine"
          };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[]
          {
            false, false, true, true,
            false, false, false, false, false, false,
            false, false, false, false, false, false,
            false, false, false, false, false, false,
            true, true, true, true,
            true, true, true, true
          };
        public override bool[] IsEvent => isEvent;
    }
}
