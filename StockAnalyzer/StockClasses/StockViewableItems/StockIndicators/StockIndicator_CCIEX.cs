using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_CCIEX : StockIndicatorBase, IRange
    {
        public StockIndicator_CCIEX()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.RangedIndicator; }
        }

        public override string Definition
        {
            get { return "CCIEX(int Period, int SmoothPeriod1, int SmoothPeriod2, float SigmoidFactor, float Overbought, float Oversold)"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "SmoothPeriod1", "SmoothPeriod2", "SigmoidFactor", "Overbought", "Oversold" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 14, 1, 10, 0.0195f, 75f, -75f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 1f), new ParamRangeFloat(0f, 100f), new ParamRangeFloat(-100f, 0f) }; }
        }

        public override string[] SerieNames { get { return new string[] { "CCIEX(" + this.Parameters[0].ToString() + ")", "Signal" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red) };
                }
                return seriePens;
            }
        }
        public override HLine[] HorizontalLines
        {
            get
            {
                HLine[] lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)) };
                lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {

            int period1 = ((int)this.parameters[1]);
            int period2 = ((int)this.parameters[2]);
            float sigma = ((float)this.parameters[3]);

            FloatSerie cciSerie = stockSerie.CalculateCCI((int)this.parameters[0]).CalculateEMA(period1);

            this.series[0] = cciSerie = cciSerie.CalculateSigmoid(100f, sigma);
            this.series[0].Name = this.Name;

            FloatSerie signalSerie = cciSerie.CalculateEMA(period2);
            this.series[1] = signalSerie;
            this.series[1].Name = this.series[0].Name + "_SIGNAL";

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            for (int i = 2; i < cciSerie.Count; i++)
            {
                this.eventSeries[0][i] = (cciSerie[i] >= signalSerie[i]);
                this.eventSeries[1][i] = (cciSerie[i] < signalSerie[i]);
                this.eventSeries[2][i] = eventSeries[0][i] & !eventSeries[0][i - 1];
                this.eventSeries[3][i] = eventSeries[1][i] & !eventSeries[1][i - 1];
                this.eventSeries[4][i] = cciSerie[i] >= 0;
                this.eventSeries[5][i] = cciSerie[i] < 0;
            }
        }

        static string[] eventNames = new string[] { "UpTrend", "DownTrend", "BullishCrossing", "BearishCrossing", "Positive", "Negative" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false, true, true, false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }

        public float Max
        {
            get { return 100f; }
        }

        public float Min
        {
            get { return -100f; }
        }
    }
}
