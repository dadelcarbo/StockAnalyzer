using System;
using System.Collections.Generic;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ATRBANDWIDTH : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "MAType" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, "MA" }; }
        }
        static List<string> emaTypes = new List<string>() { "EMA", "HMA", "MA", "EA" };
        public override ParamRange[] ParameterRanges
        {
            get
            {
                return new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeStringList( emaTypes)
                };
            }
        }
        public override string[] SerieNames { get { return new string[] { "ATRBANDWidth" }; } }
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black) };
                }
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
            // Calculate ATR Bands
            var ema = stockSerie.GetIndicator(this.parameters[1] + "(" + (int)this.parameters[0] + ")").Series[0];
            var atr = stockSerie.GetIndicator("ATR(" + this.parameters[0] + ")").Series[0];
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            FloatSerie emaWidth = closeSerie - ema;

            this.series[0] = emaWidth / atr;
            this.Series[0].Name = this.SerieNames[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
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
