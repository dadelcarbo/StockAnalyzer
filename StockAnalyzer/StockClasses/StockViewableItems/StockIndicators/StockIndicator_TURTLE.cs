using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TURTLE : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Display the highest, lowest lines for the specified period of a EMA over defined period. This is for InvestingZen Turtle strategy";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string[] ParameterNames => new string[] { "HighPeriod", "LowPeriod", "EMAPeriod" };
        public override Object[] ParameterDefaultValues => new Object[] { 36, 12, 3 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "High", "Low", "EMA" };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] {
                    new Pen(Color.DarkGreen) { Width = 2},
                    new Pen(Color.DarkRed)  { Width = 2},
                    new Pen(Color.DarkBlue) { Width = 2}};
                return seriePens;
            }
        }
        public override Area[] Areas => areas ??= new StockDrawing.Area[]
            {
                new Area {Name="Bull", Color = Color.FromArgb(128, Color.LightGreen) },
                new Area {Name="Bear", Color = Color.FromArgb(128, Color.Red) }
            };

        public override void ApplyTo(StockSerie stockSerie)
        {
            int highPeriod = (int)this.parameters[0];
            int lowPeriod = (int)this.parameters[1];
            int emaPeriod = (int)this.parameters[2];

            // Calculate MDH Channel
            FloatSerie upLine = new FloatSerie(stockSerie.Count, float.NaN);
            FloatSerie downLine = new FloatSerie(stockSerie.Count, float.NaN);

            FloatSerie emaSerie = stockSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(emaPeriod);

            upLine[0] = emaSerie[0];
            downLine[0] = emaSerie[0];

            for (int i = 1; i <= Math.Max(highPeriod, lowPeriod) && i < stockSerie.Count; i++)
            {
                upLine[i] = emaSerie.GetMax(0, i);
                downLine[i] = emaSerie.GetMin(0, i);
            }
            for (int i = Math.Max(highPeriod, lowPeriod) + 1; i < stockSerie.Count; i++)
            {
                upLine[i] = emaSerie.GetMax(i - highPeriod - 1, i);
                downLine[i] = emaSerie.GetMin(i - lowPeriod - 1, i);
            }

            int count = 0;
            this.series[count] = upLine;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = downLine;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = emaSerie;
            this.Series[count].Name = this.SerieNames[count];

            this.Areas[0].UpLine = new FloatSerie(stockSerie.Count, float.NaN);
            this.Areas[0].DownLine = new FloatSerie(stockSerie.Count, float.NaN);

            this.Areas[1].UpLine = new FloatSerie(stockSerie.Count, float.NaN);
            this.Areas[1].DownLine = new FloatSerie(stockSerie.Count, float.NaN);

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            bool upTrend = false;
            for (int i = 1; i < stockSerie.Count; i++)
            {
                count = 0;
                if (upTrend)
                {
                    upTrend = !(emaSerie[i] <= downLine[i]);
                }
                else
                {
                    upTrend = emaSerie[i] >= upLine[i];
                }
                this.Areas[upTrend ? 0 : 1].UpLine[i] = upLine[i];
                this.Areas[upTrend ? 0 : 1].DownLine[i] = downLine[i];

                this.Events[count++][i] = emaSerie[i - 1] < upLine[i - 1] && emaSerie[i] >= upLine[i];  // BrokenUp
                this.Events[count++][i] = emaSerie[i - 1] > downLine[i - 1] && emaSerie[i] <= downLine[i]; // BrokenDown
            }

        }

        static readonly string[] eventNames = new string[] { "BrokenUp", "BrokenDown" };
        public override string[] EventNames => eventNames;

        static readonly bool[] isEvent = new bool[] { true, true };
        public override bool[] IsEvent => isEvent;
    }
}
