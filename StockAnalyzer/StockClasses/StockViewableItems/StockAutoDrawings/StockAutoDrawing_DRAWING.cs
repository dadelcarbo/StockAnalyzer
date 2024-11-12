using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    public class StockAutoDrawing_DRAWING : StockAutoDrawingBase
    {
        public override string Definition => "Generate event in case on manual drawing is broken up or down";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string[] ParameterNames => new string[] { };

        public override Object[] ParameterDefaultValues => new Object[] { };
        public override ParamRange[] ParameterRanges => new ParamRange[] { };

        static string[] eventNames = null;
        public override string[] EventNames => eventNames ??= new string[] { "ResistanceBroken", "SupportBroken", "HasDrawing" };

        static readonly bool[] isEvent = new bool[] { true, true, false };
        public override bool[] IsEvent => isEvent;

        public override System.Drawing.Pen[] SeriePens => new Pen[] { };

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            if (stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
            {
                var drawingItems = stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Where(di => di.IsPersistent && di is Line2DBase).ToList();
                this.Events[2][stockSerie.LastIndex] = drawingItems.Count > 0;
                this.Events[2][stockSerie.LastCompleteIndex] = drawingItems.Count > 0;

                foreach (Line2DBase item in drawingItems)
                {
                    for (int i = (int)Math.Max(item.Point1.X, item.Point2.X); i < stockSerie.Count; i++)
                    {
                        if (i < 1)
                            continue;
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
