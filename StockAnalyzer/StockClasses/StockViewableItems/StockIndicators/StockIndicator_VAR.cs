using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_VAR : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override object[] ParameterDefaultValues => new Object[] { 0.04f, 0.2f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeFloat(0.0f, 1f), new ParamRangeFloat(0.0f, 10f) };
        public override string[] ParameterNames => new string[] { "LowThreshold", "HighThreshold" };

        public override string[] SerieNames => new string[] { "VAR" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Black) };
                return seriePens;
            }
        }
        static HLine[] lines = null;
        public override HLine[] HorizontalLines
        {
            get
            {
                lines ??= new HLine[] { new HLine(0f, new Pen(Color.DarkGray) { DashStyle = DashStyle.Dash }) };
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Set HLine value
            float lowThreshold = (float)this.Parameters[0];
            float highThreshold = (float)this.Parameters[1];

            FloatSerie varSerie = stockSerie.GetSerie(StockDataType.VARIATION);

            varSerie.Name = this.SerieNames[0];
            this.Series[0] = varSerie;

            // Manage events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 1; i < stockSerie.Count; i++)
            {
                var var = Math.Abs(varSerie[i]);
                if (var < lowThreshold)
                {
                    this.eventSeries[0][i] = true;
                }
                else if (var <= highThreshold)
                {
                    this.eventSeries[1][i] = true;
                }
                else
                {
                    this.eventSeries[2][i] = true;
                }
            }
        }

        static string[] eventNames = new string[] { "Low", "InRange", "High" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
