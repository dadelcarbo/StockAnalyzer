using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_EMA2Lines : StockIndicatorBase
    {
        public StockIndicator_EMA2Lines()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string Name => "EMA2Lines(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")";
        public override string Definition => "EMA2Lines(int FastPeriod, int SlowPeriod)";
        public override string[] ParameterNames => new string[] { "FastPeriod", "SlowPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 50 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };
        public override string[] SerieNames => new string[] { "EMA(" + this.Parameters[0].ToString() + ")", "EMA(" + this.Parameters[1].ToString() + ")" };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Green, 2), new Pen(Color.DarkRed, 2) };
                return seriePens;
            }
        }
        public override Area[] Areas => areas ??= new Area[]
            {
                new Area {Name="Bull", Color = Color.FromArgb(64, Color.Green), Visibility = true },
                new Area {Name="Bear", Color = Color.FromArgb(64, Color.Red), Visibility = true }
            };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie fastSerie = closeSerie.CalculateEMA((int)this.parameters[0]);
            FloatSerie slowSerie = closeSerie.CalculateEMA((int)this.parameters[1]);

            this.Series[0] = fastSerie;
            this.Series[1] = slowSerie;

            this.Areas[0].UpLine = new FloatSerie(stockSerie.Count, float.NaN);
            this.Areas[0].DownLine = new FloatSerie(stockSerie.Count, float.NaN);
            this.Areas[1].UpLine = new FloatSerie(stockSerie.Count, float.NaN);
            this.Areas[1].DownLine = new FloatSerie(stockSerie.Count, float.NaN);

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 2; i < stockSerie.Count; i++)
            {
                if (fastSerie[i] > slowSerie[i])
                {
                    if (closeSerie[i] > fastSerie[i])
                    {
                        this.Events[2][i] = true;
                    }
                    else
                    {
                        this.Events[4][i] = true;
                    }
                    if (fastSerie[i - 1] < slowSerie[i - 1])
                    {
                        this.Events[0][i] = true;
                    }
                }
                else
                {
                    if (closeSerie[i] < fastSerie[i])
                    {
                        this.Events[3][i] = true;
                    }
                    else
                    {
                        this.Events[5][i] = true;
                    }
                    if (fastSerie[i - 1] > slowSerie[i - 1])
                    {
                        this.Events[1][i] = true;
                    }
                }


                if (fastSerie[i] > slowSerie[i])
                {
                    this.areas[0].UpLine[i] = fastSerie[i];
                    this.areas[0].DownLine[i] = slowSerie[i];
                }
                else
                {
                    this.areas[1].UpLine[i] = slowSerie[i];
                    this.areas[1].DownLine[i] = fastSerie[i];
                }
            }
        }

        static readonly string[] eventNames = new string[] {
          "BullishCrossing", "BearishCrossing",
          "UpTrend", "DownTrend",
          "UpTrendConso", "DownTrendConso" };

        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, false, false, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
