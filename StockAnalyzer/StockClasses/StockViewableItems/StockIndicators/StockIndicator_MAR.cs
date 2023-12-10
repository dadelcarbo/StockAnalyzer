using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_MAR : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "MAR" + Environment.NewLine + "Plots growth rate divided by MaxDD";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "Period" };
        public override Object[] ParameterDefaultValues => new Object[] { 100 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };
        public override string[] SerieNames => new string[] { "MAR(" + this.Parameters[0].ToString() + ")" };

        public override System.Drawing.Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black) };

        static HLine[] lines = null;
        public override HLine[] HorizontalLines => lines ??= new HLine[] { new HLine(0, new Pen(Color.LightGray)) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];
            FloatSerie marSerie = new FloatSerie(stockSerie.Count);

            this.series[0] = marSerie;
            this.Series[0].Name = this.Name;

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            for (int i = period; i < stockSerie.Count; i++)
            {
                float roc = (closeSerie[i] - closeSerie[i - period]) / closeSerie[i];

                float maxDD = 0f;
                float high = closeSerie[i - period];
                float low = closeSerie[i - period];

                float drawdown = (high - low) / high;

                // Evaluate Max drawdown
                for (int j = i - period + 1; j <= i; j++)
                {
                    if (closeSerie[j] < low)
                    {
                        low = lowSerie[j];
                        drawdown = (high - low) / high;
                        maxDD = Math.Max(drawdown, maxDD);
                    }
                    else if (closeSerie[j] > high)
                    {
                        low = high = closeSerie[j];
                        drawdown = 0;
                    }
                }

                marSerie[i] = maxDD == 0 ? 0 : roc / maxDD;
            }
        }
        static string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
