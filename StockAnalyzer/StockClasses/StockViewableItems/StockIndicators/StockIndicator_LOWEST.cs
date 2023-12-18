using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_LOWEST : StockIndicatorBase
    {
        public override string Definition => "Calculate the number of bars the current bar is the lowest.\r\nEvent is raised when gap with previous value exceeds the trigger parameter.";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;
        public override object[] ParameterDefaultValues => new Object[] { 20 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };
        public override string[] ParameterNames => new string[] { "Trigger" };
        public override string[] SerieNames => new string[] { "LOWEST(" + this.Parameters[0].ToString() + ")" };
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Black) };
                return seriePens;
            }
        }
        public override HLine[] HorizontalLines => null;
        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);
            FloatSerie indexSerie = new FloatSerie(stockSerie.Count);
            this.series[0] = indexSerie;
            this.Series[0].Name = this.Name;

            int trigger = (int)this.Parameters[0];
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);

            var bodyLowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);

            for (int i = trigger; i < stockSerie.Count; i++)
            {
                int count = 0;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (closeSerie[i] < bodyLowSerie[j])
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
                indexSerie[i] = count;

                if (indexSerie[i] - indexSerie[i - 1] >= trigger)
                {
                    this.eventSeries[0][i] = true;
                }
            }
        }
        static string[] eventNames = new string[] { "NewLow" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true };
        public override bool[] IsEvent => isEvent;
    }
}
