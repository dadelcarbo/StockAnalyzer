using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TSI : StockIndicatorBase, IRange
    {
        public StockIndicator_TSI()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public float Max
        {
            get { return 1.0f; }
        }

        public float Min
        {
            get { return -1.0f; }
        }

        public override string Definition
        {
            get { return "TSI(int FirstPeriod, int SecondPeriod, int SignalPeriod, float Overbought, float Oversold)"; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "FirstPeriod", "FirstPeriod", "SignalPeriod", "Overbought", "Oversold" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 14, 3, 3, -0.75f, 0.75f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(-1f, 0), new ParamRangeFloat(0f, 1f) }; }
        }

        public override string[] SerieNames { get { return new string[] { "ITR(" + this.Parameters[0].ToString() + ")", "ITRS(" + this.Parameters[2].ToString() + ")" }; } }


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
            StockDailyValue previousValue = null;
            FloatSerie varSerie = new FloatSerie(stockSerie.Count);
            FloatSerie varAbsSerie = new FloatSerie(stockSerie.Count);
            int i = 0;
            float var = 0;
            foreach (StockDailyValue value in stockSerie.Values)
            {
                if (previousValue != null)
                {
                    var = (value.CLOSE - previousValue.CLOSE)/previousValue.CLOSE;
                    varSerie[i] = var;
                    varAbsSerie[i] = Math.Abs(var);
                }
                previousValue = value;
                i++;
            }

            FloatSerie smoothedVarSerie = varSerie.CalculateEMA((int)this.parameters[0]).CalculateEMA((int)this.parameters[1]);
            FloatSerie smoothedVarAbsSerie = varAbsSerie.CalculateEMA((int)this.parameters[0]).CalculateEMA((int)this.parameters[1]);
            FloatSerie TSISerie = smoothedVarSerie / smoothedVarAbsSerie;

            this.series[0] = TSISerie;
            this.series[0].Name = this.SerieNames[0];
            this.series[1] = TSISerie.CalculateEMA((int)this.parameters[2]);
            this.series[1].Name = this.SerieNames[1];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

        }

        static string[] eventNames = new string[] {  };
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
