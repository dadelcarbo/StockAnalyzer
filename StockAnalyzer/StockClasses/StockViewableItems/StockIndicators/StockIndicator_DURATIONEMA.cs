using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_DURATIONEMA : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Number of bars above EMA " + Environment.NewLine + "Positive is price is above EMA, negative if price is below EMA";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override string[] ParameterNames => new string[] { "Period" };
        public override Object[] ParameterDefaultValues => new Object[] { 100 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };
        public override string[] SerieNames => new string[] { "DURATIONEMA(" + this.Parameters[0].ToString() + ")" };

        public override System.Drawing.Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black) };

        static HLine[] lines = null;
        public override HLine[] HorizontalLines => lines ??= new HLine[] { new HLine(0, new Pen(Color.LightGray)) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];
            FloatSerie emaSerie = stockSerie.GetIndicator($"EMA({period})").Series[0];
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var countSerie = new FloatSerie(stockSerie.Count);

            int count = 0;
            for (int i = 0; i < stockSerie.Count; i++)
            {
                if (closeSerie[i] >= emaSerie[i])
                {
                    count = count >= 0 ? count + 1 : 0;
                }
                else
                {
                    count = count <= 0 ? count - 1 : 0;
                }
                countSerie[i] = count;
            }

            this.series[0] = countSerie;
            this.Series[0].Name = this.Name;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
        }
        static string[] eventNames = new string[] { };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { };
        public override bool[] IsEvent => isEvent;
    }
}
