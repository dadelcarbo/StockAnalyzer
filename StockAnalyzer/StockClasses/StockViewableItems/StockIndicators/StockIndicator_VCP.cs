﻿using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_VCP : StockIndicatorBase
    {
        public override string Definition => $"Calculate the ratio between a long term and short range, it's usefull to find volatility contraction.";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override bool RequiresVolumeData => false;

        public override string[] ParameterNames => new string[] { "LongPeriod", "ShortPeriod", "ROR Trigger" };

        public override Object[] ParameterDefaultValues => new Object[] { 35, 3, 0.2f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 10f) };
        public override string[] SerieNames => new string[] { "VCP(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black, 1) };

        HLine[] lines;
        public override HLine[] HorizontalLines => lines ??= new HLine[] { new HLine(.25f, new Pen(Color.Gray) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var longPeriod = (int)parameters[0];
            var shortPeriod = (int)parameters[1];
            var rorTrigger = (float)parameters[2];
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var rorSerie = stockSerie.GetIndicator($"ROR({longPeriod})").Series[0];

            FloatSerie vcpSerie = new FloatSerie(stockSerie.Count);
            for (int i = longPeriod; i < stockSerie.Count; i++)
            {
                if (rorSerie[i] > rorTrigger)
                {
                    var longRangeHigh = highSerie.GetMax(i - longPeriod + 1, i);
                    var longRangeLow = lowSerie.GetMin(i - longPeriod + 1, i);
                    var shortRangeHigh = highSerie.GetMax(i - shortPeriod + 1, i);
                    var shortRangeLow = lowSerie.GetMin(i - shortPeriod + 1, i);
                    vcpSerie[i] = (longRangeHigh - longRangeLow) / (shortRangeHigh - shortRangeLow);
                }
            }

            this.Series[0] = vcpSerie;
            this.Series[0].Name = SerieNames[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
        }

        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
