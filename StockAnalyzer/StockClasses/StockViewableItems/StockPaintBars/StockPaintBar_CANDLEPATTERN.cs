using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_CANDLEPATTERN : StockPaintBarBase
    {
        public StockPaintBar_CANDLEPATTERN()
        {
        }
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
                if (eventNames == null)
                {
                    eventNames = new string[] { "Doji" };
                }
                return eventNames;
            }
        }

        static readonly bool[] isEvent = new bool[] { true, true, true };
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
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Purple) };
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
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            float accuracy = 0.1f; // 10%
            for (int i = 1; i < stockSerie.Count; i++)
            {
                // Check Gaps
                this.eventSeries[0][i] = highSerie[i - 1] < lowSerie[i];
                this.eventSeries[1][i] = lowSerie[i - 1] > highSerie[i];

                float range = highSerie[i] - lowSerie[i];
                float rangeMiddle = lowSerie[i] + range/2.0f;
                float body = closeSerie[i] - openSerie[i];
                float bodyRatio = body/range;
                float bodyMiddle = openSerie[i] + body/2.0f;
                float bodyLow = Math.Min(closeSerie[i], openSerie[i]);
                float bodyHigh = Math.Max(closeSerie[i], openSerie[i]);

                if (range > 0.0)
                { 
                    if (Math.Abs(bodyRatio) < accuracy)
                    {
                        if (Math.Abs(rangeMiddle-bodyMiddle) < accuracy)
                        {
                            // Dojis
                            this.eventSeries[2][i] = true;
                        }
                    }
                }
                else
                {
                    // Flat bars not taken into account.
                }
            }
        }
    }
}
