using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_HMACD : StockIndicatorBase, IRange
    {
        public StockIndicator_HMACD()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public float Max
        {
            get { return 100.0f; }
        }

        public float Min
        {
            get { return 0.0f; }
        }

        public override string Name
        {
            get { return "HMACD(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + "," + this.Parameters[2].ToString() + ")"; }
        }
        public override string Definition
        {
            get { return "HMACD(int SlowPeriod, int FastPeriod, int SignalPeriod)"; }
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

        public override string[] SerieNames { get { return new string[] { "HMACD", "Signal" }; } }


        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black), new Pen(Color.Red) };
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
            FloatSerie HMACDSerie = stockSerie.GetIndicator("HOSC(" + this.parameters[1] + "," + this.parameters[0] + ")").Series[0];
            FloatSerie signalSerie = HMACDSerie.CalculateMA((int)this.parameters[2]);
            this.series[0] = HMACDSerie;
            this.series[0].Name = this.SerieNames[0];
            this.series[1] = signalSerie;
            this.series[1].Name = this.SerieNames[1];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 1; i < HMACDSerie.Count; i++)
            {
                this.eventSeries[0][i] = HMACDSerie[i] >= 0;
                this.eventSeries[1][i] = HMACDSerie[i] < 0;
                this.eventSeries[2][i] = signalSerie[i] < HMACDSerie[i];
                this.eventSeries[3][i] = signalSerie[i] > HMACDSerie[i];
            }
        }

        static string[] eventNames = new string[] { "HMACDPositive", "HMACDNegative", "HMACDAboveSignal", "HMACDBelowSignal" };
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
