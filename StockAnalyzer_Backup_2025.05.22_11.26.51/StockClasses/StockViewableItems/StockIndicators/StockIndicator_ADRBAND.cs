using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ADRBAND : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string Definition => "Band made of a moving average and border based on adding ADR";
        public override string[] ParameterNames => new string[] { "Period", "ADRPeriod", "NbUpDev", "NbDownDev", "MAType" };
        public override Object[] ParameterDefaultValues => new Object[] { 20, 10, 3.0f, -3.0f, "EMA" };
        static readonly List<string> emaTypes = StockIndicatorMovingAvgBase.MaTypes;
        public override ParamRange[] ParameterRanges => new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(-5.0f, 20.0f),
                new ParamRangeFloat(-20.0f, 5.0f),
                new ParamRangeMA()
                };
        public override string[] SerieNames => new string[] { "ADRBANDUp", "ADRBANDDown", this.parameters[4] + "(" + (int)this.parameters[0] + ")" };
        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Blue), new Pen(Color.Blue), new Pen(Color.Blue) };
                return seriePens;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            // Calculate ADR Bands
            var period = (int)this.parameters[0];
            var adrPeriod = (int)this.parameters[1];
            var emaIndicator = stockSerie.GetIndicator(this.parameters[4] + "(" + period + ")").Series[0];

            var upDev = (float)parameters[2];
            var downDev = (float)parameters[3];

            var adr = stockSerie.GetIndicator("ADR(" + adrPeriod + ")").Series[0];
            var upperBB = emaIndicator + upDev * adr;
            var lowerBB = emaIndicator + downDev * adr;

            this.series[0] = upperBB;
            this.Series[0].Name = this.SerieNames[0];

            this.series[1] = lowerBB;
            this.Series[1].Name = this.SerieNames[1];

            this.series[2] = emaIndicator;
            this.Series[2].Name = this.SerieNames[2];

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
