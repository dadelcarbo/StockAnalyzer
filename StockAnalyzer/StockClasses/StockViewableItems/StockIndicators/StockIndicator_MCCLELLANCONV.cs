using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_MCCLELLANCONV : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string[] ParameterNames => new string[] { "FastPeriod", "SlowPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 19, 39 };
        public override ParamRange[] ParameterRanges => new ParamRange[] {
             new ParamRangeInt(1, 500), new ParamRangeInt(1, 500)  };

        public override string[] SerieNames => new string[] { "OSC UNCH", $"EMA({parameters[1]})", $"EMA({parameters[0]})", "SUM/10" };
        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Blue), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.DarkRed) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var fastEma = stockSerie.GetIndicator($"EMA({parameters[0]})").Series[0];
            var slowEma = stockSerie.GetIndicator($"EMA({parameters[1]})").Series[0];
            var oscSerie = fastEma - slowEma;
            var unchSerie = fastEma + oscSerie;
            var lowSerie = slowEma - oscSerie;

            this.Series[0] = unchSerie;
            this.Series[1] = slowEma;
            this.Series[2] = fastEma;
            this.Series[3] = lowSerie;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            for (int i = 2; i < stockSerie.Count; i++)
            {
                int eventIndex = 0;
                this.Events[eventIndex++][i] = fastEma[i] > slowEma[i];
                this.Events[eventIndex++][i] = fastEma[i] <= slowEma[i];

                this.Events[eventIndex++][i] = closeSerie[i - 1] <= unchSerie[i - 1] && closeSerie[i] > unchSerie[i];
                this.Events[eventIndex++][i] = closeSerie[i - 1] <= fastEma[i - 1] && closeSerie[i] > fastEma[i];
                this.Events[eventIndex++][i] = closeSerie[i - 1] <= slowEma[i - 1] && closeSerie[i] > slowEma[i];
                this.Events[eventIndex][i] = closeSerie[i - 1] <= lowSerie[i - 1] && closeSerie[i] > lowSerie[i];
            }
        }

        static readonly string[] eventNames = new string[] { "Bullish", "Bearish", "Broken Unc Up", "Broken fast Up", "Broken Slow Up", "Broken Low Up" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { false, false, true, true, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
