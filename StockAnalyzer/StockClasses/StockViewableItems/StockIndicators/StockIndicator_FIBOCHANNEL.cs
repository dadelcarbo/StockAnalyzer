using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_FIBOCHANNEL : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;

        public override string[] ParameterNames => new string[] { "Period", "Ratio" };

        public override Object[] ParameterDefaultValues => new Object[] { 60, 0.61f };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 5f) };

        public override string[] SerieNames => new string[] { "HIGH", "FiboUp", "MID", "FiboLow", "LOW" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] {
            new Pen(Color.DarkGreen, 2),
            new Pen(Color.DarkGreen),
            new Pen(Color.Black) { DashStyle = DashStyle.Dash },
            new Pen(Color.DarkRed),
            new Pen(Color.DarkRed,2)
        };

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];

            // Calculate FIBOCHANNEL Channel
            FloatSerie upLine = new FloatSerie(stockSerie.Count);
            FloatSerie downLine = new FloatSerie(stockSerie.Count);

            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);

            for (int i = 0; i < period; i++)
            {
                upLine[i] = float.NaN;
                downLine[i] = float.NaN;
            }
            for (int i = period; i < stockSerie.Count; i++)
            {
                upLine[i] = highSerie.GetMax(i - period, i);
                downLine[i] = lowSerie.GetMin(i - period, i);
            }

            int count = 0;
            this.series[count] = upLine;
            this.Series[count].Name = this.SerieNames[count];

            float fiboRatio = (float)this.parameters[1];
            this.series[++count] = fiboRatio * upLine + (1.0f - fiboRatio) * downLine;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = (upLine + downLine) / 2.0f;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = (1.0f - fiboRatio) * upLine + fiboRatio * downLine;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = downLine;
            this.Series[count].Name = this.SerieNames[count];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
        }

        private static readonly string[] eventNames = { };
        public override string[] EventNames => eventNames;

        static readonly bool[] isEvent = { };

        public override bool[] IsEvent => isEvent;
    }
}
