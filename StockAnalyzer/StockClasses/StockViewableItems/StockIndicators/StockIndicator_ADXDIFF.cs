using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ADXDIFF : StockIndicatorBase
    {
        public StockIndicator_ADXDIFF()
        {
        }

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override object[] ParameterDefaultValues => new Object[] { 14, 25f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };
        public override string[] ParameterNames => new string[] { "Period", "InputSmoothing" };


        public override string[] SerieNames => new string[] { "ADXDIFF(" + this.Parameters[0].ToString() + ")" };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Black) };
                return seriePens;
            }
        }
        public override HLine[] HorizontalLines
        {
            get
            {
                if (lines == null)
                {
                    lines = new HLine[] { new HLine(0, new Pen(Color.DarkGray)) };
                    lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                }
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Set HLine value
            float trendThreshold = 25; // Used only for event calculation
            int smoothing = (int)this.Parameters[1];

            int period = (int)this.Parameters[0];
            var adx = stockSerie.GetIndicator("ADX(" + period + "," + trendThreshold + "," + smoothing + ")");

            this.Series[0] = (adx.Series[1] - adx.Series[2]);
            this.Series[0].Name = this.Name;

            this.CreateEventSeries(stockSerie.Count);

            for (int i = period; i < stockSerie.Count; i++)
            {
                this.eventSeries[0][i] = adx.Series[1][i] > adx.Series[2][i];
                this.eventSeries[1][i] = adx.Series[1][i] < adx.Series[2][i];
            }
        }

        static readonly string[] eventNames = new string[] { "Positive", "Negative" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { false, false };
        public override bool[] IsEvent => isEvent;
    }
}
