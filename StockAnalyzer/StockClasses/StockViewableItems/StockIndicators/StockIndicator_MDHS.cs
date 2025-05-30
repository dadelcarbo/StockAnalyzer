﻿using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_MDHS : StockIndicatorBase
    {
        public override string Definition => "Display the highest, lowest and mid lines for the specified period of a EMA.";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string[] ParameterNames => new string[] { "Period", "EMAPeriod" };
        public override Object[] ParameterDefaultValues => new Object[] { 20, 3 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "Highest", "Mid", "Lowest" };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.DarkGreen), new Pen(Color.Black), new Pen(Color.DarkRed) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];

            // Calculate MDH Channel
            FloatSerie upLine = new FloatSerie(stockSerie.Count);
            FloatSerie midLine = new FloatSerie(stockSerie.Count);
            FloatSerie downLine = new FloatSerie(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE).CalculateEMA((int)this.parameters[1]);

            upLine[0] = closeSerie[0];
            downLine[0] = closeSerie[0];
            midLine[0] = closeSerie[0];

            for (int i = 1; i < Math.Min(period + 1, stockSerie.Count); i++)
            {
                upLine[i] = closeSerie.GetMax(0, i);
                downLine[i] = closeSerie.GetMin(0, i);
                midLine[i] = (upLine[i] + downLine[i]) / 2.0f;
            }
            for (int i = period + 1; i < stockSerie.Count; i++)
            {
                upLine[i] = closeSerie.GetMax(i - period - 1, i);
                downLine[i] = closeSerie.GetMin(i - period - 1, i);
                midLine[i] = (upLine[i] + downLine[i]) / 2.0f;
            }

            int count = 0;
            this.series[count] = upLine;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = midLine;
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
                    upTrend = closeSerie[i] > midLine[i];
                }
                else
                {
                    upTrend = closeSerie[i] > midLine[i];
                }

                this.Events[count++][i] = upTrend;
                this.Events[count++][i] = !upTrend;
                this.Events[count++][i] = (!this.Events[0][i - 1]) && (this.Events[0][i]);
                this.Events[count++][i] = (this.Events[0][i - 1]) && (!this.Events[0][i]);
            }
        }

        static readonly string[] eventNames = new string[]
          {
            "Uptrend", "DownTrend", "BrokenUp","BrokenDown"
          };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[]
          {
            false, false, true, true
          };
        public override bool[] IsEvent => isEvent;
    }
}
