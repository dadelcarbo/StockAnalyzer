using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ATRBAND : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string Definition => "Band made of a moving average and border based on adding ATR";
        public override string[] ParameterNames => new string[] { "Period", "ATRPeriod", "NbUpDev", "NbDownDev", "MAType" };
        public override Object[] ParameterDefaultValues => new Object[] { 35, 15, 2.0f, -2.0f, "EMA" };
        static readonly List<string> emaTypes = StockIndicatorMovingAvgBase.MaTypes;
        public override ParamRange[] ParameterRanges => new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(-5.0f, 20.0f),
                new ParamRangeFloat(-20.0f, 5.0f),
                new ParamRangeMA()
                };
        public override string[] SerieNames => new string[] { "ATRBANDUp", "ATRBANDDown", this.parameters[4] + "(" + (int)this.parameters[0] + ")" };
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
            // Calculate ATR Bands
            var period = (int)this.parameters[0];
            var atrPeriod = (int)this.parameters[1];
            var emaIndicator = stockSerie.GetIndicator(this.parameters[4] + "(" + period + ")").Series[0];

            var upDev = (float)parameters[2];
            var downDev = (float)parameters[3];

            var atr = stockSerie.GetIndicator("ATR(" + atrPeriod + ")").Series[0];
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
        static readonly string[] eventNames = new string[] { "BrokenUp", "BrokenDown" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true };
        public override bool[] IsEvent => isEvent;
    }
}
