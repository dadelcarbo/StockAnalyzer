using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_OPTIX2MA : StockIndicatorBase, IRange
    {
        public StockIndicator_OPTIX2MA()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.RangedIndicator; }
        }
        public override string Definition
        {
            get { return "OPTIX(int Period1, int Period2, float Overbought, float Oversold)"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period1", "Period2", "Overbought", "Oversold" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 6, 12, 75f, 25f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 100f), new ParamRangeFloat(0f, 100f) }; }
        }

        public override string[] SerieNames { get { return new string[] { "OPTIX_MA(" + this.Parameters[0].ToString() + ")", "OPTIX_MA(" + this.Parameters[1].ToString() + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.DarkGreen), new Pen(Color.DarkRed) };
                }
                return seriePens;
            }
        }
        public override HLine[] HorizontalLines
        {
            get
            {
                HLine[] lines = new HLine[] { new HLine(50, new Pen(Color.LightGray)), new HLine((float)this.parameters[2], new Pen(Color.Gray)), new HLine((float)this.parameters[3], new Pen(Color.Gray)) };
                lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[1].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[2].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            if (!stockSerie.HasOptix)
            {
                return;
            }

            FloatSerie optixSerie = stockSerie.GetSerie(StockDataType.OPTIX);

            this.series[0] = optixSerie.CalculateMA((int)this.Parameters[0]);
            this.series[0].Name = this.SerieNames[0];

            this.series[1] = optixSerie.CalculateMA((int) this.Parameters[1]);
            this.series[1].Name = this.SerieNames[1];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

        }

        public float Max
        {
            get { return 100.0f; }
        }

        public float Min
        {
            get { return 0.0f; }
        }

        static string[] eventNames = new string[] { "Top", "Bottom", "Overbought", "Oversold" };
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
