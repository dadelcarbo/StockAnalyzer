using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_VAR : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override string Definition => base.Definition + Environment.NewLine + "Plots the variation over the period";

        public override string Name => "VAR(" + this.Parameters[0].ToString() + ")";

        public override object[] ParameterDefaultValues => new Object[] { 20 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };
        public override string[] ParameterNames => new string[] { "Period" };

        public override string[] SerieNames => new string[] { "VAR(" + this.Parameters[0].ToString() + ")" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black) { DashStyle = DashStyle.Custom } };
                }
                return seriePens;
            }
        }

        public override HLine[] HorizontalLines
        {
            get
            {
                HLine[] lines = new HLine[] { new HLine(0, new Pen(Color.Black)) };
                return lines;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];
            FloatSerie varSerie = stockSerie.GetSerie(StockDataType.VARIATION) * 100f;
            this.series[0] = varSerie;
            this.Series[0].Name = this.SerieNames[0];

            if (period > 1)
            {
                var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                for(int i = 0; i<=period;i++)
                {
                    varSerie[i] = 0f;
                }
                for (int i = period; i < stockSerie.Count; i++)
                {
                    var periodClose = closeSerie[i - period];
                    varSerie[i] = 100f*(closeSerie[i] - periodClose)/periodClose;
                }
            }

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            bool bull = true, previousBull = true;
            for (int i = 2; i < stockSerie.Count; i++)
            {
                bull = varSerie[i] > 0;
                this.eventSeries[0][i] = bull && ! previousBull;
                this.eventSeries[1][i] = !bull & previousBull;
                this.eventSeries[2][i] = bull;
                this.eventSeries[3][i] = !bull;

                previousBull = bull;
            }
        }

        static readonly string[] eventNames = new string[] { "CrossUp", "CrossDown", "Bullish", "Bearish" };
        public override string[] EventNames => eventNames;

        static readonly bool[] isEvent = new bool[] { true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
