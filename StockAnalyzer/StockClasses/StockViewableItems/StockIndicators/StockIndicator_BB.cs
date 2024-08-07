﻿using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_BB : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string[] ParameterNames => new string[] { "Period", "NbUpDev", "NbDownDev", "MAType" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 2.0f, -2.0f, "MA" };

        public override ParamRange[] ParameterRanges => new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(-5.0f, 20.0f),
                new ParamRangeFloat(-20.0f, 5.0f),
                new ParamRangeMA()
                };

        public override string[] SerieNames => new string[] { "BBUp", "BBDown", this.parameters[3] + "(" + (int)this.parameters[0] + ")" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Blue), new Pen(Color.Blue), new Pen(Color.Blue) };
                return seriePens;
            }
        }

        public override Area[] Areas => areas ??= new StockDrawing.Area[] {
            new StockDrawing.Area {Color = Color.FromArgb(64, Color.Blue) }
            };

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Calculate Bollinger Bands
            FloatSerie upperBB = null;
            FloatSerie lowerBB = null;
            IStockIndicator emaIndicator = stockSerie.GetIndicator(this.parameters[3] + "(" + (int)this.parameters[0] + ")");

            stockSerie.GetSerie(StockDataType.CLOSE).CalculateBB(emaIndicator.Series[0], (int)this.parameters[0], (float)this.parameters[1], (float)this.parameters[2], ref upperBB, ref lowerBB);

            this.series[0] = upperBB;
            this.Series[0].Name = this.SerieNames[0];

            this.series[1] = lowerBB;
            this.Series[1].Name = this.SerieNames[1];

            this.series[2] = emaIndicator.Series[0];
            this.Series[2].Name = this.SerieNames[2];

            this.Areas[0].UpLine = upperBB;
            this.Areas[0].DownLine = lowerBB;

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

        static readonly string[] eventNames = new string[] { "UpBandOvershot", "DownBandOvershot", "BullishSignal", "BearishSignal" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { false, false, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
