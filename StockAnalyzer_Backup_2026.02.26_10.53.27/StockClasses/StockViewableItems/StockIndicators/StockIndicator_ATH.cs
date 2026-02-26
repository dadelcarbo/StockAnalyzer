using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ATH : StockIndicatorBase
    {
        public override string Definition => "Defines if a stock has been at highest in n bar in the last m bars.";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override object[] ParameterDefaultValues => new Object[] { 175, 35 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(0, 500) };
        public override string[] ParameterNames => new string[] { "Trigger", "Period" };

        public override string[] SerieNames => new string[] { $"ATH({this.Parameters[0]},{this.Parameters[1]})" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Black) };

        public override HLine[] HorizontalLines => null;

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);
            BoolSerie athBoolSerie = new BoolSerie(stockSerie.Count);
            FloatSerie athSerie = new FloatSerie(stockSerie.Count);
            this.series[0] = athSerie;
            this.Series[0].Name = this.Name;

            int trigger = (int)this.Parameters[0];
            int period = (int)this.Parameters[1];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            for (int i = trigger; i < stockSerie.Count; i++)
            {
                bool highest = true;
                for (int j = i - 1; j >= i - trigger; j--)
                {
                    if (closeSerie[i] < closeSerie[j])
                    {
                        highest = false;
                        break;
                    }
                }
                athBoolSerie[i] = highest;
            }

            for (int i = trigger + period; i < stockSerie.Count; i++)
            {
                athSerie[i] = athBoolSerie.Or(i - period, i) ? 1.0f : 0.0f;

                if (athSerie[i] > athSerie[i - 1])
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
