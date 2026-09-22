using StockAnalyzer.StockData;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using StockAnalyzerSettings;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_EMABAND : StockIndicatorBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Display the highest, lowest lines for the specified period of a EMA over defined period. This is for InvestingZen Turtle strategy";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string[] ParameterNames => new string[] { "HighPeriod", "LowPeriod", "EMAPeriod", "MidRatio" };
        public override Object[] ParameterDefaultValues => new Object[] { 35, 35, 6, 0.5f };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(0.0f, 1.0f) };

        public override string[] SerieNames => new string[] { "Signal", "High", "Low", "Mid" };

        public override Pen[] SeriePens => seriePens ??= new Pen[] {
            ColorManager.GetPen("Indicator.Band.Signal"),
            ColorManager.GetPen("Indicator.Band.Up",2),
            ColorManager.GetPen("Indicator.Band.Down",2),
            ColorManager.GetPen("Indicator.Band.Mid", 2)
        };

        public override Area[] Areas => areas ??= new Area[]
            {
                new Area {Name="BullConso", Color = ColorManager.GetColor("Indicator.Band.Bull") },
                new Area {Name="BearConso", Color = ColorManager.GetColor("Indicator.Band.Bear") }
            };

        public override void ApplyTo(DataSerie stockSerie)
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

            this.series[count] = emaSerie;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = upLine;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = downLine;
            this.Series[count].Name = this.SerieNames[count];


            var ratio = (float)this.parameters[3];
            FloatSerie midLine = (upLine * ratio + downLine * (1.0f - ratio));
            this.series[++count] = midLine;
            this.Series[count].Name = this.SerieNames[count];

            this.Areas[0].UpLine = new FloatSerie(stockSerie.Count, float.NaN);
            this.Areas[0].DownLine = new FloatSerie(stockSerie.Count, float.NaN);

            this.Areas[1].UpLine = new FloatSerie(stockSerie.Count, float.NaN);
            this.Areas[1].DownLine = new FloatSerie(stockSerie.Count, float.NaN);

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            bool upTrend = false;
            bool downTrend = false;
            bool upTrendConso = false;
            bool downTrendConso = false;
            for (int i = 1; i < stockSerie.Count; i++)
            {
                count = 0;

                if (emaSerie[i] >= upLine[i])
                {
                    upTrend = true;
                    downTrend = false;
                    upTrendConso = downTrendConso = false;
                }
                if (emaSerie[i] <= downLine[i])
                {
                    upTrend = false;
                    downTrend = true;
                    upTrendConso = downTrendConso = false;
                }

                if (upTrend)
                {
                    upTrendConso = emaSerie[i] < upLine[i];
                    if (upTrendConso)
                    {
                        this.Areas[1].UpLine[i] = upLine[i];
                        this.Areas[1].DownLine[i] = emaSerie[i];
                    }
                    this.Areas[0].UpLine[i] = emaSerie[i];
                    this.Areas[0].DownLine[i] = downLine[i];
                }
                if (downTrend)
                {
                    downTrendConso = emaSerie[i] > downLine[i];
                    if (downTrendConso)
                    {
                        this.Areas[0].UpLine[i] = emaSerie[i];
                        this.Areas[0].DownLine[i] = downLine[i];
                    }
                    this.Areas[1].UpLine[i] = upLine[i];
                    this.Areas[1].DownLine[i] = emaSerie[i];
                }

                this.Events[count++][i] = emaSerie[i - 1] < upLine[i - 1] && emaSerie[i] >= upLine[i];  // BrokenUp
                this.Events[count++][i] = emaSerie[i - 1] > downLine[i - 1] && emaSerie[i] <= downLine[i]; // BrokenDown
                this.Events[count++][i] = upTrend;
                this.Events[count++][i] = downTrend;
                this.Events[count++][i] = upTrendConso;
                this.Events[count++][i] = downTrendConso;
                this.Events[count++][i] = emaSerie[i] > midLine[i];
                this.Events[count++][i] = emaSerie[i] < midLine[i];
            }
        }

        static readonly string[] eventNames = new string[] { "BrokenUp", "BrokenDown", "Bullish", "Bearish", "BullishConso", "BearishConso", "AboveMidLine", "BelowMidLine" };
        public override string[] EventNames => eventNames;

        static readonly bool[] isEvent = new bool[] { true, true, false, false, false, false, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
