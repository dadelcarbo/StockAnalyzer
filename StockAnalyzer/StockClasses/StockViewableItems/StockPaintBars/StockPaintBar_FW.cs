using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_FW : StockPaintBarBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }

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
                eventNames ??= new string[] { "Buy", "Sell", "Hold" };
                return eventNames;
            }
        }

        static readonly bool[] isEvent = new bool[] { true, true, false };
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
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green) };
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

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);
            FloatSerie volumeSerie = stockSerie.GetSerie(StockDataType.VOLUME);
            FloatSerie highestSerie = stockSerie.GetIndicator("HIGHEST(21)").Series[0];
            FloatSerie emaSerie = stockSerie.GetIndicator("EMA(20)").Series[0];
            FloatSerie natrSerie = stockSerie.GetIndicator("NATR(14)").Series[0];

            bool holding = false;
            float trail = float.NaN;
            for (int i = 1; i < stockSerie.Count; i++)
            {
                if (holding)
                {
                    if (closeSerie[i] < trail)
                    {
                        holding = false;
                        this.eventSeries[1][i] = true;
                    }
                    else
                    {
                        trail = Math.Max(trail, closeSerie[i] * 0.8f);
                    }
                }
                else
                {
                    if (closeSerie[i] < 2.0f)
                        continue;
                    if (highestSerie[i] < 21)
                        continue;
                    if (closeSerie[i] * volumeSerie[i] < 1000000)
                        continue;
                    if ((volumeSerie[i] - volumeSerie[i - 1]) / volumeSerie[i - 1] < 0.5f)
                        continue;
                    var variation = (closeSerie[i] - openSerie[i]) / openSerie[i];
                    if (variation < 0.05f || variation > 0.2f)
                        continue;
                    if (closeSerie[i] < emaSerie[i])
                        continue;
                    if (natrSerie[i] > 8.0f)
                        continue;

                    trail = closeSerie[i] * 0.8f;

                    holding = true;
                    this.eventSeries[0][i] = holding;
                }
                this.eventSeries[2][i] = holding;
            }
        }
    }
}
