using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_MACD : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override string[] ParameterNames => new string[] { "SlowPeriod", "FastPeriod", "SignalPeriod" };
        public override Object[] ParameterDefaultValues => new Object[] { 26, 12, 9 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "EMACD", "Signal", "Histogram" };


        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Red), new Pen(Color.Black), new Pen(Color.Black) { DashStyle = System.Drawing.Drawing2D.DashStyle.Custom } };
                return seriePens;
            }
        }
        static HLine[] lines = null;
        public override HLine[] HorizontalLines
        {
            get
            {
                if (lines == null)
                {
                    lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)) };
                    lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                }
                return lines;
            }
        }
        public override string[] SerieFormats => serieFormats ??= new string[] { "P2", "P2", "P2" };
        public override void ApplyTo(StockSerie stockSerie)
        {
            var fastMA = stockSerie.GetIndicator($"EMA({this.parameters[1]})").Series[0];
            var slowMA = stockSerie.GetIndicator($"EMA({this.parameters[0]})").Series[0];

            FloatSerie MACDSerie = (fastMA - slowMA) / fastMA;
            FloatSerie signalSerie = MACDSerie.CalculateEMA((int)this.parameters[2]);
            this.series[0] = MACDSerie;
            this.series[0].Name = this.SerieNames[1];
            this.series[1] = signalSerie;
            this.series[1].Name = this.SerieNames[2];
            this.series[2] = MACDSerie - signalSerie;
            this.series[2].Name = this.SerieNames[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 1; i < MACDSerie.Count; i++)
            {
                this.eventSeries[0][i] = MACDSerie[i] >= 0;
                this.eventSeries[1][i] = MACDSerie[i] < 0;
                this.eventSeries[2][i] = signalSerie[i] < MACDSerie[i];
                this.eventSeries[3][i] = signalSerie[i] > MACDSerie[i];
            }
        }

        static readonly string[] eventNames = new string[] { "MACDPositive", "MACDNegative", "MACDAboveSignal", "MACDBelowSignal" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { false, false, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
