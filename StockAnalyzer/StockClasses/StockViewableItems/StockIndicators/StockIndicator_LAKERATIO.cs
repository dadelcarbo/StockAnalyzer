﻿using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_LAKERATIO : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { 20 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }

        public override string[] SerieNames { get { return new string[] { "LAKERATIO(" + this.Parameters[0].ToString() + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Blue) };
                }
                return seriePens;
            }
        }

        public override HLine[] HorizontalLines
        {
            get { return null; }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.Parameters[0];
            float alpha = 2.0f / (float)(period + 1);

            var lakeRatioSerie = this.series[0] = new FloatSerie(stockSerie.Count);
            this.Series[0].Name = this.Name;

            var highestSerie = stockSerie.GetIndicator($"HIGHEST({period})").Series[0];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie bodyHighSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);
            FloatSerie bodyLowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);

            var bullish = false;
            var solid = 0f;
            var lake = 0f;
            var stop = 0f;
            var previousHigh = 0f;
            var count = 0;
            for (int i = period; i < stockSerie.Count; i++)
            {
                if (bullish)
                {
                    if (closeSerie[i] > stop)
                    {
                        stop = stop + alpha * (bodyLowSerie[i] - stop);
                        count++;
                        previousHigh = Math.Max(previousHigh, bodyHighSerie[i]);
                        lake += previousHigh - closeSerie[i];
                        solid += closeSerie[i] - stop;
                        lakeRatioSerie[i] = (solid - lake) / (stop * count);
                    }
                    else
                    {
                        bullish = false;
                        count = 0;
                    }
                }
                else
                {
                    if (bodyLowSerie[i] > bodyLowSerie[i - 1])
                    {
                        bullish = true;
                        solid = 0f;
                        lake = 0f;
                        stop = bodyLowSerie.GetMin(i - period, i);
                        previousHigh = bodyHighSerie[i];
                    }
                }
            }
        }

        static string[] eventNames = new string[] { };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}