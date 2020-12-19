using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_SCREENER : StockPaintBarBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Screener detecting cup and handle figure";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string[] ParameterNames => new string[] { "LongPeriod", "ShortPeriod", "RangeSize" };
        public override Object[] ParameterDefaultValues => new Object[] { 100, 50, 30 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[] { "Break Out" };
                }
                return eventNames;
            }
        }

        static readonly bool[] isEvent = new bool[] { true };
        public override bool[] IsEvent => isEvent;

        public override Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red) };
                    foreach (Pen pen in seriePens)
                    {
                        pen.Width = 1;
                    }
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            DrawingItem.CreatePersistent = false;
            try
            {
                if (stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
                {
                    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].RemoveAll(d => !d.IsPersistent);
                }
                else
                {
                    stockSerie.StockAnalysis.DrawingItems.Add(stockSerie.BarDuration, new StockDrawingItems());
                }
                StockDrawingItems drawingItems = stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration];

                // Detecting events
                this.CreateEventSeries(stockSerie.Count);

                FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
                FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
                FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
                FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);

                int longPeriod = (int)this.parameters[0];
                int shortPeriod = (int)this.parameters[1];
                float range = (int)this.parameters[2] / 100.0f;

                //FloatSerie RORSerie = stockSerie.GetIndicator($"ROR({longPeriod},1)").Series[0];
                //FloatSerie RODSerie = stockSerie.GetIndicator($"ROD({shortPeriod},1)").Series[0];
                FloatSerie STOCKSSerie = stockSerie.GetIndicator($"STOKS({shortPeriod},1,1)").Series[0];

                for (int i = stockSerie.Count - 1; i > longPeriod + 1; i--)
                {
                    // Latest close is higher
                    if (closeSerie[i] < closeSerie[i - 1])
                        continue;

                    // Calculate englobing box
                    int startIndex = i - longPeriod;
                    int pivotIndex = i - shortPeriod;
                    var maxIndexBox = highSerie.FindMaxIndex(startIndex, i);
                    if (maxIndexBox > pivotIndex)
                        continue;

                    var minIndexBox = lowSerie.FindMinIndex(startIndex, i);
                    if (minIndexBox > maxIndexBox)
                        continue;

                    // Detect cup
                    var minCup = lowSerie[minIndexBox];
                    var maxCup = highSerie[maxIndexBox];
                    var range1 = (maxCup - minCup) / minCup;
                    if (range1 < range)
                        continue;

                    // check if highest in short period
                    var maxIndexHandle = closeSerie.FindMaxIndex(pivotIndex, i);
                    if (maxIndexHandle != i) continue;

                    var minIndexHandle = lowSerie.FindMinIndex(pivotIndex, i);
                    float minHandle = lowSerie[minIndexHandle];
                    float maxHandle = highSerie[maxIndexHandle];

                    if (minCup > minHandle) continue;

                    drawingItems.Add(new Rectangle2D(new PointF(startIndex - 1, minCup * 0.99f), new PointF(i + 1, maxCup * 1.01f)));
                    drawingItems.Add(new Rectangle2D(new PointF(startIndex, minCup), new PointF(maxIndexBox, maxCup)) { Pen = this.SeriePens[0] });
                    drawingItems.Add(new Rectangle2D(new PointF(pivotIndex, minHandle), new PointF(i, maxHandle)) { Pen = this.SeriePens[1] });

                    this.eventSeries[0][i] = true;
                    break;
                }
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }
        }
    }
}