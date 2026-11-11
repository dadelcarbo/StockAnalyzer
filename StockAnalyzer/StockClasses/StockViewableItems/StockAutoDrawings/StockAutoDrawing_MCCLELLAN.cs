using StockAnalyzer.StockMath;
using StockAnalyzer.StockData;
using System;
using System.Drawing;
using StockAnalyzer.StockDrawing;
using StockAnalyzerSettings;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    public class StockAutoDrawing_MCCLELLAN : StockAutoDrawingBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Detect Cup and Handle patterns";

        public override string[] ParameterNames => new string[] { "FastPeriod", "SlowPeriod" };
        public override Object[] ParameterDefaultValues => new Object[] { 35, 75 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { };
        public override Pen[] SeriePens => seriePens ??= new Pen[] { };

        public override void ApplyTo(DataSerie stockSerie)
        {
            var mcClellanIndicator = stockSerie.GetIndicator($"MCCLELLANCONV({this.Parameters[0]},{this.Parameters[1]})");

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            if (mcClellanIndicator == null)
            {
                return;
            }

            var bullishEvents = mcClellanIndicator.GetEvents("Bullish");
            var brokenUpEvents = mcClellanIndicator.GetEvents("Broken Unc Up");

            for (int i = 1; i < stockSerie.Count; i++)
            {
                if (bullishEvents[i] && brokenUpEvents[i])
                {
                    this.Events[0][i] = true;
                    this.DrawingItems.Insert(0, new Line2D(new PointF(i, 0), 0, 1) { Pen = ColorManager.GetPen("Graph.Drawing") });
                }
            }
        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                eventNames ??= new string[] { "BrokenUp" };
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true };
        public override bool[] IsEvent => isEvent;
    }
}