using System;
using System.Collections.Generic;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ATRBAND : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string Definition
        {
            get { return "BB(int Period, float NbUpDev, float NbDownDev, string MAType)"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "NbUpDev", "NbDownDev", "MAType" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 3.0f, -3.0f, "MA" }; }
        }
        static List<string> emaTypes = new List<string>() { "EMA", "HMA", "MA", "EA" };
        public override ParamRange[] ParameterRanges
        {
            get
            {
                return new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(0f, 20.0f),
                new ParamRangeFloat(-20.0f, 0.0f),
                new ParamRangeMA()
                };
            }
        }
        public override string[] SerieNames { get { return new string[] { "ATRBANDUp", "ATRBANDDown", this.parameters[3] + "(" + (int)this.parameters[0] + ")" }; } }
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Blue), new Pen(Color.Blue), new Pen(Color.Blue) };
                }
                return seriePens;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            // Calculate ATR Bands
            var period = (int)this.parameters[0];
            var emaIndicator = stockSerie.GetIndicator(this.parameters[3] + "(" + period + ")").Series[0];

            var upDev = (float)parameters[1];
            var downDev = (float)parameters[2];

            var atr = stockSerie.GetIndicator("ATR(" + this.parameters[0] + ")").Series[0];
            var upperBB = emaIndicator + upDev * atr;
            var lowerBB = emaIndicator + downDev * atr;

            this.series[0] = upperBB;
            this.Series[0].Name = this.SerieNames[0];

            this.series[1] = lowerBB;
            this.Series[1].Name = this.SerieNames[1];

            this.series[2] = emaIndicator;
            this.Series[2].Name = this.SerieNames[2];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            for (int i = period; i < upperBB.Count; i++)
            {
                if (closeSerie[i - 1] < upperBB[i - 1] && closeSerie[i] > upperBB[i])
                {
                    // BrokenUp
                    this.eventSeries[0][i] = true;
                }
                else if (closeSerie[i - 1] > lowerBB[i - 1] && closeSerie[i] < lowerBB[i])
                {
                    // BrokenDown
                    this.eventSeries[1][i] = true;
                }
            }
        }
        static string[] eventNames = new string[] { "BrokenUp", "BrokenDown" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true };
        public override bool[] IsEvent => isEvent;
    }
}
