using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_EMACD : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }

        public override string Name
        {
            get { return "EMACD(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + "," + this.Parameters[2].ToString() + ")"; }
        }
        public override string Definition
        {
            get { return "EMACD(int SlowPeriod, int FastPeriod, int SignalPeriod)"; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "SlowPeriod", "FastPeriod", "SignalPeriod" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 26, 12, 9 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { "Histogram", "EMACD", "Signal" }; } }


        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black) { DashStyle = System.Drawing.Drawing2D.DashStyle.Custom }, new Pen(Color.Red), new Pen(Color.Black) };
                }
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
        public override void ApplyTo(StockSerie stockSerie)
        {
            var fastMA = stockSerie.GetIndicator($"EMA({this.parameters[1]})").Series[0];
            var slowMA = stockSerie.GetIndicator($"EMA({this.parameters[0]})").Series[0];

            FloatSerie EMACDSerie = fastMA - slowMA;
            FloatSerie signalSerie = EMACDSerie.CalculateEMA((int)this.parameters[2]);
            this.series[0] = EMACDSerie - signalSerie;
            this.series[0].Name = this.SerieNames[0];
            this.series[1] = EMACDSerie;
            this.series[1].Name = this.SerieNames[1];
            this.series[2] = signalSerie;
            this.series[2].Name = this.SerieNames[2];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 1; i < EMACDSerie.Count; i++)
            {
                this.eventSeries[0][i] = EMACDSerie[i] >= 0;
                this.eventSeries[1][i] = EMACDSerie[i] < 0;
                this.eventSeries[2][i] = signalSerie[i] < EMACDSerie[i];
                this.eventSeries[3][i] = signalSerie[i] > EMACDSerie[i];
            }
        }

        static string[] eventNames = new string[] { "EMACDPositive", "EMACDNegative", "EMACDAboveSignal", "EMACDBelowSignal" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false, false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
