using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_OPTIX : StockIndicatorBase, IRange
    {
        public StockIndicator_OPTIX()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.RangedIndicator; }
        }
        public override string Name
        {
            get { return "OPTIX(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")"; }
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
            get { return new Object[] { 1, 10, 75f, 25f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 100f), new ParamRangeFloat(0f, 100f) }; }
        }

        public override string[] SerieNames { get { return new string[] { "OPTIX", "OPTIX(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" }; } }

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
            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            if (!stockSerie.HasOptix)
            {
                this.series[0] = new FloatSerie(stockSerie.Count);
                this.series[0].Name = this.SerieNames[0];

                this.series[1] = new FloatSerie(stockSerie.Count);
                this.series[1].Name = this.SerieNames[1];
                return;
            }

            FloatSerie optixSerie = stockSerie.GetSerie(StockDataType.OPTIX);

            this.series[0] = optixSerie.CalculateMA((int)this.Parameters[0]);
            this.series[0].Name = this.SerieNames[0];

            this.series[1] = optixSerie.CalculateMA((int) this.Parameters[1]);
            this.series[1].Name = this.SerieNames[1];

            //
            float overbought = (float)this.Parameters[2];
            float oversold = (float)this.Parameters[3];
            for (int i = (int)this.Parameters[1]; i < stockSerie.Count; i++)
            {
                float optix = this.series[0][i];
                float optixSlow = this.series[1][i];
                this.eventSeries[0][i] = (optix >= overbought);
                this.eventSeries[1][i] = (optix <= oversold);
                this.eventSeries[2][i] = (optix > optixSlow);
                this.eventSeries[3][i] = (optix < optixSlow);
            }
        }

        public float Max
        {
            get { return 100.0f; }
        }

        public float Min
        {
            get { return 0.0f; }
        }

        static string[] eventNames = new string[] { "Overbought", "Oversold", "Bullish", "Bearsish" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
