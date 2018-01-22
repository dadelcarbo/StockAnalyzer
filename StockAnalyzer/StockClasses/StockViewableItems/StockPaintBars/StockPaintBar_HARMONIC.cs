using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_HARMONIC : StockPaintBarBase
    {
        public StockPaintBar_HARMONIC()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }

        public override bool HasTrendLine { get { return true; } }

        public override string Definition
        {
            get { return "HARMONIC(int hlPeriod)"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "hlPeriod" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 3 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 100) }; }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            // Do drawing Items cleanup
            if (stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
            {
                stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Clear();
            }
            else
            {
                stockSerie.StockAnalysis.DrawingItems.Add(stockSerie.BarDuration, new StockDrawingItems());
            }

            DrawingItem.CreatePersistent = false;
            int hlPeriod = (int)this.parameters[0];
            var points = stockSerie.generateZigzagPoints(0, stockSerie.Count - 1, hlPeriod);

            IStockIndicator hlTrailSR = stockSerie.GetIndicator("TRAILHLSR(" + hlPeriod + ")");
            
            BoolSerie supportDetected = hlTrailSR.Events[0];
            BoolSerie resistanceDetected = hlTrailSR.Events[1];

            for (int i = 5; i < points.Count; i++)
            {
                var xabcd = new XABCD();
                xabcd.AddPoint(points[i - 4]);
                xabcd.AddPoint(points[i - 3]);
                xabcd.AddPoint(points[i - 2]);
                xabcd.AddPoint(points[i - 1]);
                xabcd.AddPoint(points[i]);

                if (xabcd.GetPatternName() != null)
                {
                    foreach (var p in xabcd.GetLines(SeriePens[0], false))
                    {
                        stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(p);
                    }

                    // Detect Event
                    for(int j = (int)points[i].X; j < stockSerie.Count; j++)
                    {
                        if(xabcd.IsBullish && supportDetected[j])
                        {
                            this.Events[0][j] = true;
                            break;
                        }
                        if (!xabcd.IsBullish && resistanceDetected[j])
                        {
                            this.Events[1][j] = true;
                            break;
                        }
                    }
                }
            }
            DrawingItem.CreatePersistent = true;
        }

        
        static readonly bool[] isEvent = new bool[] { true, true};
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }


        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[] { "BullishPattern", "BearishPattern"};
                }
                return eventNames;
            }
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

    }
}
