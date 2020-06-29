using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_DRAWING : StockPaintBarBase
    {
        public override string Definition => "Generate event in case on manual drawing is broken up or down";
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
                    eventNames = new string[] { "ResistanceBroken", "SupportBroken" };
                }
                return eventNames;
            }
        }

        static readonly bool[] isEvent = new bool[] { true, true };
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
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red) };
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

            if (stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
            {
                var drawingItems = stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Where(di => di is Line2DBase);
                foreach (Line2DBase item in drawingItems)
                {
                    for (int i = (int)Math.Max(item.Point1.X, item.Point2.X); i < stockSerie.Count; i++)
                    {
                        if (item.ContainsAbsciss(i) && item.ContainsAbsciss(i - 1))
                        {
                            float itemValue = item.ValueAtX(i);
                            float itemPreviousValue = item.ValueAtX(i - 1);
                            if (closeSerie[i - 1] < itemPreviousValue && closeSerie[i] > itemValue)
                            {
                                // Resistance Broken
                                this.Events[0][i] = true;
                                break;
                            }
                            else if (closeSerie[i - 1] > itemPreviousValue && closeSerie[i] < itemValue)
                            {
                                // Support Broken
                                this.Events[1][i] = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
