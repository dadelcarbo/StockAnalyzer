﻿using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_BBWIDTH : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "Period", "MAType" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, "MA" };
        public override ParamRange[] ParameterRanges => new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeMA()
                };

        public override string[] SerieNames => new string[] { "BBWIDTH" };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Black) };
                return seriePens;
            }
        }

        public override HLine[] HorizontalLines
        {
            get
            {
                HLine[] lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)), new HLine(2f, new Pen(Color.Gray)), new HLine(-2f, new Pen(Color.Gray)) };
                lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[1].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[2].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Calculate Bollinger Bands
            FloatSerie upperBB = null;
            FloatSerie lowerBB = null;

            var ema = stockSerie.GetIndicator(this.parameters[1] + "(" + (int)this.parameters[0] + ")").Series[0];
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            closeSerie.CalculateBB(ema, (int)this.parameters[0], 1f, -1f, ref upperBB, ref lowerBB);

            FloatSerie emaWidth = closeSerie - ema;
            FloatSerie bbWidth = upperBB - ema;

            this.series[0] = emaWidth / bbWidth;
            this.Series[0].Name = this.SerieNames[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
        }

        static readonly string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
