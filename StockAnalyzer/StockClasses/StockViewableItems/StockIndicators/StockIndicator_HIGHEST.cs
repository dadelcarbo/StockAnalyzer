using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_HIGHEST : StockIndicatorBase
    {
        public override string Definition => "Calculate the number of bars the current bar is the highest.\r\nEvent is raised when gap with previous value exceeds the trigger parameter.";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override object[] ParameterDefaultValues => new Object[] { 0 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500) };
        public override string[] ParameterNames => new string[] { "Trigger" };

        public override string[] SerieNames => new string[] { "HIGHEST(" + this.Parameters[0].ToString() + ")" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black) };

        public override HLine[] HorizontalLines => null;

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);
            FloatSerie highestSerie = new FloatSerie(stockSerie.Count);
            this.series[0] = highestSerie;
            this.Series[0].Name = this.Name;

            int maxIn = (int)this.Parameters[0];
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            for (int i = maxIn + 1; i < stockSerie.Count; i++)
            {
                int count = 0;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (closeSerie[i] > closeSerie[j])
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
                highestSerie[i] = count;

                if (highestSerie[i] > highestSerie[i - 1])
                {
                    this.eventSeries[0][i] = true;
                }
            }
        }
        static readonly string[] eventNames = new string[] { "NewHigh" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true };
        public override bool[] IsEvent => isEvent;
    }
}
