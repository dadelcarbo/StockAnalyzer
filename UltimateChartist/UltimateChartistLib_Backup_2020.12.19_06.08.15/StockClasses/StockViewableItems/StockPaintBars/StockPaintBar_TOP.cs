using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_TOP : StockPaintBarBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }

        public override bool HasTrendLine { get { return true; } }

        public override string[] ParameterNames
        {
            get { return new string[] { }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { }; }
        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[] { "Top", "Bottom", "LowerHigh", "HigherLow" };
                }
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true, true, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green) };
                    foreach (Pen pen in seriePens)
                    {
                        pen.Width = 2;
                    }
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            int startIndex = 2;
            int endIndex = stockSerie.LastCompleteIndex;

            float previousTop = float.MaxValue;
            float previousBottom = float.MinValue;

            for (int i = startIndex; i <= endIndex; i++)
            {
                float close = closeSerie[i];
                if (closeSerie[i - 2] < closeSerie[i - 1] && closeSerie[i - 1] > close)
                {
                    this.eventSeries[0][i] = true;
                    if (close < previousTop)
                    {
                        this.eventSeries[2][i] = true;
                        this.stockTexts.Add(new StockText
                        {
                            AbovePrice = true,
                            Index = i,
                            Text = "LH"
                        });
                    }
                    previousTop = close;
                }

                if (closeSerie[i - 2] > closeSerie[i - 1] && closeSerie[i - 1] < close)
                {
                    this.eventSeries[1][i] = true;
                    if (close > previousBottom)
                    {
                        this.eventSeries[3][i] = true;
                        this.stockTexts.Add(new StockText
                        {
                            AbovePrice = false,
                            Index = i,
                            Text = "HL"
                        });
                    }
                    previousBottom = close;
                }
            }
        }
    }
}