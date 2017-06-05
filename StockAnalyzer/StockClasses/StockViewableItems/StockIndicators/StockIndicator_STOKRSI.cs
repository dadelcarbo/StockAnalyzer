using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_STOKRSI : StockIndicatorBase, IRange
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.RangedIndicator; }
        }
        public float Max
        {
            get { return 1f; }
        }

        public float Min
        {
            get { return -1.0f; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "rsiPeriod", "stockPeriod", "smoothing", "inputSmoothing" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 14, 14, 3, 3 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { "STOKRSI(" + this.Parameters[0].ToString() + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green) };
                }
                return seriePens;
            }
        }
        public override HLine[] HorizontalLines
        {
            get
            {
                HLine[] lines = new HLine[] { new HLine(50, new Pen(Color.LightGray)), new HLine(75f, new Pen(Color.Gray)), new HLine(25f, new Pen(Color.Gray)) };
                lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[1].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[2].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                return lines;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            int rsiPeriod = (int)this.parameters[0];
            int stokPeriod = (int)this.parameters[1];
            int smoothing = (int)this.parameters[2];
            int inputSmoothing = (int)this.parameters[3];

            FloatSerie rsiSerie = stockSerie.GetIndicator("RSI(" + rsiPeriod + "," + inputSmoothing + ")").Series[0].CalculateEMA(smoothing);
            FloatSerie stokSerie = rsiSerie.CalculateStochastik(stokPeriod, smoothing);

            this.series[0] = stokSerie;
            this.series[0].Name = this.SerieNames[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

        }

        static string[] eventNames = new string[] { };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] {  };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
