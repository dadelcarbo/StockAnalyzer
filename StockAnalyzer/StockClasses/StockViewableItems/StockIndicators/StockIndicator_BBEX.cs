﻿using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_BBEX : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string[] ParameterNames => new string[] { "Period", "NbUpDev", "NbDownDev", "MAType" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 2.0f, -2.0f, "MA" };
        static readonly List<string> emaTypes = StockIndicatorMovingAvgBase.MaTypes;
        public override ParamRange[] ParameterRanges => new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(-5.0f, 20.0f),
                new ParamRangeFloat(-20.0f, 5.0f),
                new ParamRangeMA()
                };

        public override string[] SerieNames => new string[] { "BBEXUp", "BBEXDown", this.parameters[3] + "(" + (int)this.parameters[0] + ")" };

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
            // Calculate Bollinger Bands
            FloatSerie upperBB = null;
            FloatSerie lowerBB = null;
            var emaSerie = stockSerie.GetIndicator(this.parameters[3] + "(" + (int)this.parameters[0] + ")").Series[0];

            stockSerie.GetSerie(StockDataType.CLOSE).CalculateBBEX(emaSerie, (int)this.parameters[0], (float)this.parameters[1], (float)this.parameters[2], ref upperBB, ref lowerBB);

            this.series[0] = upperBB;
            this.Series[0].Name = this.SerieNames[0];

            this.series[1] = lowerBB;
            this.Series[1].Name = this.SerieNames[1];

            this.series[2] = emaSerie;
            this.Series[2].Name = this.SerieNames[2];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie bodyHighSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);
            FloatSerie bodyLowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);

            // Detecting events
            bool bullish = false;
            bool bearish = false;
            for (int i = (int)this.parameters[0]; i < closeSerie.Count; i++)
            {
                if (closeSerie[i - 1] < upperBB[i - 1] && closeSerie[i] > upperBB[i])
                {
                    this.eventSeries[0][i] = true;
                }
                else if (bodyLowSerie[i] > upperBB[i])
                {
                    if (!bullish)
                    {
                        this.eventSeries[2][i] = true;
                        bullish = true;
                    }
                }
                else if (bullish && closeSerie[i] < emaSerie[i])
                {
                    bullish = false;
                }
                else if (closeSerie[i - 1] > lowerBB[i - 1] && closeSerie[i] < lowerBB[i])
                {
                    if (!bearish)
                    {
                        this.eventSeries[1][i] = true;
                        bearish = true;
                    }
                }
                else if (bodyHighSerie[i] < lowerBB[i])
                {
                    this.eventSeries[3][i] = true;
                }
                else if (bearish && closeSerie[i] > emaSerie[i])
                {
                    bearish = false;
                }

                this.eventSeries[4][i] = bullish;
                this.eventSeries[5][i] = bearish;
            }
        }

        static readonly string[] eventNames = new string[] { "NewHigh", "NewLow", "FirstBodyAboveBand", "FirstBodyBelowBand", "Bullish", "Bearish" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
