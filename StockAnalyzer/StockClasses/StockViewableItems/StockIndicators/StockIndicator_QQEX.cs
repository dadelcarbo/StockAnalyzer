using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_QQEX : StockIndicatorBase, IRange
    {
        public StockIndicator_QQEX()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.RangedIndicator; }
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
            get { return "QQE(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + "," + this.Parameters[2].ToString() + ")"; }
        }
        public override string Definition
        {
            get { return "QQE(int rsiPeriod, int rsiSmoothing, int trailPeriod, float Overbought, float Oversold)"; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "rsiPeriod", "rsiSmoothing", "trailPeriod", "Overbought", "Oversold" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 14, 3, 3, 75f, 25f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 50f), new ParamRangeFloat(50f, 100f) }; }
        }

        public override string[] SerieNames { get { return new string[] { "RSI(" + this.Parameters[0].ToString() + ")", "TRAIL(" + this.Parameters[2].ToString() + ")" }; } }


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
                HLine[] lines = new HLine[] { new HLine(50, new Pen(Color.LightGray)), new HLine(80, new Pen(Color.Gray)), new HLine(20, new Pen(Color.Gray)) };
                lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[1].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                lines[2].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                return lines;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie rsiSerie;
            if (closeSerie.Min <= 0.0f)
            {
                rsiSerie = closeSerie.CalculateRSI((int)this.parameters[0], false);
            }
            else
            {
                rsiSerie = closeSerie.CalculateRSI((int)this.parameters[0], true);
            }
            rsiSerie = rsiSerie.CalculateEMA((int) this.parameters[1]);

            FloatSerie trailSerie = rsiSerie.CalculateHLTrail((int)this.parameters[2]);

            this.series[0] = rsiSerie;
            this.series[0].Name = this.SerieNames[0];
            this.series[1] = trailSerie;
            this.series[1].Name = this.SerieNames[1];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            float overbought = (float)this.parameters[3];
            float oversold = (float)this.parameters[4];

            bool isOverSold = false;
            bool isOverBought = false;

            for (int i = 1; i < rsiSerie.Count; i++)
            {
                int j = 0;
                this.eventSeries[j++][i] = rsiSerie[i] > trailSerie[i];
                this.eventSeries[j++][i] = rsiSerie[i] < trailSerie[i];
                this.eventSeries[j++][i] = (rsiSerie[i - 1] < trailSerie[i - 1] && rsiSerie[i] > trailSerie[i]);
                this.eventSeries[j++][i] = (rsiSerie[i - 1] > trailSerie[i - 1] && rsiSerie[i] < trailSerie[i]);
                isOverSold = rsiSerie[i] <= oversold;
                isOverBought = rsiSerie[i] >= overbought;
                this.eventSeries[j++][i] = isOverBought;
                this.eventSeries[j++][i] = isOverSold;
                this.eventSeries[j++][i] = (!isOverSold) && this.eventSeries[j-2][i - 1];
                this.eventSeries[j++][i] = (!isOverBought) && this.eventSeries[j-4][i - 1];
                this.eventSeries[j++][i] = rsiSerie[i] >= 50;
                this.eventSeries[j++][i] = rsiSerie[i] < 50;
            }
        }

        static string[] eventNames = new string[] { "Bullish", "Bearish", "BullishCrossing", "BearishCrossing", "Overbought", "Oversold", "OutOfOversold", "OutOfOverbought", "OverFifty", "BelowFifty" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false, true, true, false, false, true, true, false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
