using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_NORMRSI : StockIndicatorBase, IRange
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.RangedIndicator; }
        }
        public float Max
        {
            get { return 1.0f; }
        }

        public float Min
        {
            get { return -1.0f; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "Smoothing", "Norm" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 30, 3, 3 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 99) }; }
        }

        public override string[] SerieNames { get { return new string[] { "RSI(" + this.Parameters[0].ToString() + ")"}; } }

        public override Pen[] SeriePens
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
                HLine[] lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)), new HLine(0.75f, new Pen(Color.Gray)), new HLine(-0.75f, new Pen(Color.Gray)) };
                lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[1].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[2].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                return lines;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            string indicatorName = "RSI(" + this.parameters[0] + "," + this.parameters[1] +")";
            IStockIndicator indicator = stockSerie.GetIndicator(indicatorName);
            FloatSerie serie = indicator.Series[0];
            var range = indicator as IRange;
            if (range != null)
            {
                serie = serie.Normalise(range.Min, range.Max, this.Min, this.Max);
            }
            else
            {
                serie = serie.Normalise(this.Min, this.Max);
            }
            serie = serie.Pow((int)this.parameters[2]);
            
            this.series[0] = serie;
            this.series[0].Name = this.SerieNames[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 2; i < serie.Count; i++)
            {
                this.eventSeries[0][i] = (serie[i - 2] < serie[i - 1] && serie[i - 1] > serie[i]);
                this.eventSeries[1][i] = (serie[i - 2] > serie[i - 1] && serie[i - 1] < serie[i]);
            }
        }

        static string[] eventNames = new string[] { "BullishCrossing", "BearishCrossing", "Bullish", "Bearish" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
