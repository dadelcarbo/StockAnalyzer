using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public abstract class StockIndicatorMovingAvgBase : StockIndicatorBase
    {
        public static List<string> MaTypes => new List<string> { "EMA", "MA", "XMA", "MID", "WMA" };

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override object[] ParameterDefaultValues => new Object[] { 35 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };
        public override string[] ParameterNames => new string[] { "Period" };
        public override string[] SerieNames => new string[] { $"{this.ShortName}({this.Parameters[0]})" };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.GreenYellow) };
                return seriePens;
            }
        }
        public override Area[] Areas => areas ??= new Area[]
            {
                new Area {Name="Bull", Color = Color.FromArgb(20, Color.Green), Visibility = false },
                new Area {Name="Bear", Color = Color.FromArgb(20, Color.Red), Visibility = false }
            };

        protected void CalculateEvents(StockSerie stockSerie)
        {
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie bodyHighSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);
            FloatSerie bodyLowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);

            FloatSerie maSerie = this.series[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            this.Areas[0].UpLine = new FloatSerie(stockSerie.Count, float.NaN);
            this.Areas[0].DownLine = new FloatSerie(stockSerie.Count, float.NaN);
            this.Areas[1].UpLine = new FloatSerie(stockSerie.Count, float.NaN);
            this.Areas[1].DownLine = new FloatSerie(stockSerie.Count, float.NaN);
            for (int i = 2; i < maSerie.Count; i++)
            {
                this.eventSeries[0][i] = (maSerie[i - 2] > maSerie[i - 1] && maSerie[i - 1] < maSerie[i]);  // Bottom
                this.eventSeries[1][i] = (maSerie[i - 2] < maSerie[i - 1] && maSerie[i - 1] > maSerie[i]);  // Top
                this.eventSeries[2][i] = closeSerie[i - 1] < maSerie[i - 1] && closeSerie[i] > maSerie[i];  // CrossAbove
                this.eventSeries[3][i] = closeSerie[i - 1] > maSerie[i - 1] && closeSerie[i] < maSerie[i];  // CrossBelow
                this.eventSeries[4][i] = lowSerie[i] > maSerie[i] && lowSerie[i - 1] < maSerie[i - 1];      // FirstBarAbove
                this.eventSeries[5][i] = highSerie[i] < maSerie[i] && highSerie[i - 1] > maSerie[i - 1];    // FirstBarBelow
                this.eventSeries[6][i] = closeSerie[i] > maSerie[i];                                        // PriceAbove
                this.eventSeries[7][i] = closeSerie[i] < maSerie[i];                                        // PriceBelow
                this.eventSeries[8][i] = lowSerie[i] > maSerie[i];                                          // BarAbove
                this.eventSeries[9][i] = highSerie[i] < maSerie[i];                                         // BarBelow
                this.eventSeries[10][i] = maSerie[i - 1] < maSerie[i];                                      // Rising
                this.eventSeries[11][i] = maSerie[i - 1] > maSerie[i];                                      // Falling
                this.eventSeries[10][i] = maSerie[i - 1] < maSerie[i];                                      // Rising
                this.eventSeries[11][i] = maSerie[i - 1] > maSerie[i];                                      // Falling
                this.eventSeries[12][i] = closeSerie[i] > openSerie[i] && bodyLowSerie[i] > maSerie[i] && bodyLowSerie[i - 1] < maSerie[i - 1];      // FirstBarAbove
                this.eventSeries[13][i] = closeSerie[i] < openSerie[i] && bodyHighSerie[i] < maSerie[i] && bodyHighSerie[i - 1] > maSerie[i - 1];    // FirstBarBelow

                if (closeSerie[i] > maSerie[i])
                {
                    this.areas[0].UpLine[i] = closeSerie[i];
                    this.areas[0].DownLine[i] = maSerie[i];
                }
                else
                {
                    this.areas[1].UpLine[i] = maSerie[i];
                    this.areas[1].DownLine[i] = closeSerie[i];
                }
            }
        }

        static readonly string[] eventNames = new string[] { "Bottom", "Top", "CrossAbove", "CrossBelow", "FirstBarAbove", "FirstBarBelow", "PriceAbove", "PriceBelow", "BarAbove", "BarBelow", "Rising", "Falling", "FirstUpBodyAbove", "FirstDownBodyBelow" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, true, true, true, true, false, false, false, false, false, false, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
