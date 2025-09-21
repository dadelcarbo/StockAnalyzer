using StockAnalyzer.StockDrawing;
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
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "OSC UNCH", $"EMA({parameters[0]})", $"EMA({parameters[1]})", "SUM/10" };
        public override Pen[] SeriePens => seriePens ??= new Pen[] { new Pen(Color.Blue), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.DarkRed) };

        public override Area[] Areas => areas ??= new Area[]
            {
                new Area {Name="Bull", Color = Color.FromArgb(64, Color.Green), Visibility = true },
                new Area {Name="Bear", Color = Color.FromArgb(64, Color.Red), Visibility = true }
            };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var fastEma = stockSerie.GetIndicator($"EMA({parameters[0]})").Series[0];
            var slowEma = stockSerie.GetIndicator($"EMA({parameters[1]})").Series[0];
            var oscSerie = fastEma - slowEma;
            var unchSerie = fastEma + oscSerie;
            var lowSerie = slowEma - oscSerie;

            this.Series[0] = unchSerie;
            this.Series[0].Name = this.SerieNames[0];
            this.Series[1] = fastEma;
            this.Series[2] = slowEma;
            this.Series[3] = lowSerie;
            this.Series[3].Name = this.SerieNames[3];

            this.Areas[0].UpLine = new FloatSerie(stockSerie.Count, float.NaN);
            this.Areas[0].DownLine = new FloatSerie(stockSerie.Count, float.NaN);

            this.Areas[1].UpLine = new FloatSerie(stockSerie.Count, float.NaN);
            this.Areas[1].DownLine = new FloatSerie(stockSerie.Count, float.NaN);

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            for (int i = 2; i < stockSerie.Count; i++)
            {
                int eventIndex = 0;
                bool bullish = fastEma[i] > slowEma[i];
                this.Events[eventIndex++][i] = bullish; // Bullish
                this.Events[eventIndex++][i] = !bullish;// Bearish
                if (bullish)
                {
                    this.Events[eventIndex++][i] = closeSerie[i] > unchSerie[i]; // Strong
                    this.Events[eventIndex++][i] = false;                        // Weak

                    this.areas[0].UpLine[i] = unchSerie[i];
                    this.areas[0].DownLine[i] = lowSerie[i];
                }
                else
                {
                    this.Events[eventIndex++][i] = false;                        // Strong
                    this.Events[eventIndex++][i] = closeSerie[i] < unchSerie[i]; // Weak

                    this.areas[1].UpLine[i] = lowSerie[i];
                    this.areas[1].DownLine[i] = unchSerie[i];
                }

                this.Events[eventIndex++][i] = closeSerie[i - 1] <= unchSerie[i - 1] && closeSerie[i] > unchSerie[i];   // Broken Unc Up
                this.Events[eventIndex++][i] = closeSerie[i - 1] <= fastEma[i - 1] && closeSerie[i] > fastEma[i];       // Broken fast Up
                this.Events[eventIndex++][i] = closeSerie[i - 1] <= slowEma[i - 1] && closeSerie[i] > slowEma[i];       // Broken Slow Up
                this.Events[eventIndex][i] = closeSerie[i - 1] <= lowSerie[i - 1] && closeSerie[i] > lowSerie[i];       // Broken Low Up
            }
        }

        static readonly string[] eventNames = new string[] { "Bullish", "Bearish", "Strong", "Weak", "Broken Unc Up", "Broken fast Up", "Broken Slow Up", "Broken Low Up" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { false, false, false, false, true, true, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
