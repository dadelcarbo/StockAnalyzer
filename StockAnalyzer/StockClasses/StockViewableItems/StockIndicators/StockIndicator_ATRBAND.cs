using StockAnalyzer.StockDrawing;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ATRBAND : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string Definition => "Band made of a moving average and border based on adding ATR";
        public override string[] ParameterNames => new string[] { "Period", "ATRPeriod", "NbUpDev", "NbDownDev", "MAType", "SignalPeriod" };
        public override Object[] ParameterDefaultValues => new Object[] { 35, 15, 2.0f, -2.0f, "EMA", 3 };

        static readonly List<string> emaTypes = StockIndicatorMovingAvgBase.MaTypes;
        public override ParamRange[] ParameterRanges => new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(-5.0f, 20.0f),
                new ParamRangeFloat(-20.0f, 5.0f),
                new ParamRangeMA(),
                new ParamRangeInt(1, 500)
                };

        public override string[] SerieNames => new string[] { "ATRBANDUp", "ATRBANDDown", this.parameters[4] + "(" + (int)this.parameters[0] + ")", "Signal" };


        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Blue), new Pen(Color.Blue), new Pen(Color.Blue), new Pen(Color.DarkRed) };

        public override Area[] Areas => areas ??= new StockDrawing.Area[] { new StockDrawing.Area { Color = Color.FromArgb(64, Color.Blue) } };

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Calculate ATR Bands
            var period = (int)this.parameters[0];
            var atrPeriod = (int)this.parameters[1];
            var maSerie = stockSerie.GetIndicator(this.parameters[4] + "(" + period + ")").Series[0];

            var upDev = (float)parameters[2];
            var downDev = (float)parameters[3];

            var atr = stockSerie.GetIndicator("ATR(" + atrPeriod + ")").Series[0];
            var upperBB = maSerie + upDev * atr;
            var lowerBB = maSerie + downDev * atr;
            var signalSerie = stockSerie.GetIndicator($"EMA({(int)this.parameters[5]})").Series[0];

            this.series[0] = upperBB;
            this.Series[0].Name = this.SerieNames[0];

            this.series[1] = lowerBB;
            this.Series[1].Name = this.SerieNames[1];

            this.series[2] = maSerie;
            this.Series[2].Name = this.SerieNames[2];

            this.series[3] = signalSerie;
            this.Series[3].Name = signalSerie.Name;

            this.Areas[0].UpLine = upperBB;
            this.Areas[0].DownLine = lowerBB;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            var signalCrossAboveEvents = this.Events[Array.IndexOf<string>(this.EventNames, "SignalCrossAbove")];
            var signalAboveEvents = this.Events[Array.IndexOf<string>(this.EventNames, "SignalAbove")];
            var signalCrossBelowEvents = this.Events[Array.IndexOf<string>(this.EventNames, "SignalCrossBelow")];
            var signalBelowEvents = this.Events[Array.IndexOf<string>(this.EventNames, "SignalBelow")];

            for (int i = 1; i < upperBB.Count; i++)
            {
                if (signalSerie[i] > upperBB[i])
                {
                    signalAboveEvents[i] = true;
                    signalCrossAboveEvents[i] = !signalAboveEvents[i - 1];
                }
                else
                if (signalSerie[i] < lowerBB[i])
                {
                    signalBelowEvents[i] = true;
                    signalCrossBelowEvents[i] = !signalBelowEvents[i - 1];
                }
            }
        }

        static readonly string[] eventNames = new string[] {
            "SignalCrossAbove", "SignalAbove",
            "SignalCrossBelow", "SignalBelow"
        };

        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, false, true, false };
        public override bool[] IsEvent => isEvent;
    }
}
