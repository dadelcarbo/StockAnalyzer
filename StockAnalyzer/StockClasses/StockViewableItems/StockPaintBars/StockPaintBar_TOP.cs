using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_TOP : StockPaintBarBase
    {
        public StockPaintBar_TOP()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }

        public override bool HasTrendLine { get { return true; } }

        public override string[] ParameterNames
        {
            get { return new string[] { "LeftPeriod", "RightPeriod", "InputSmooting" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 1, 1, 1 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 100), new ParamRangeInt(1, 100), new ParamRangeInt(1, 500) }; }
        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[] { "Top", "Bottom" };
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
                    seriePens = new Pen[] { new Pen(Color.Red), new Pen(Color.Green) };
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
            /// Detecting events
            this.CreateEventSeries(stockSerie.Count);

            if (stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
            {
                stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Clear();
            }
            else
            {
                stockSerie.StockAnalysis.DrawingItems.Add(stockSerie.BarDuration, new StockDrawingItems());
            }
            StockDrawingItems drawingItems = stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration];

            int leftPeriod = (int)this.parameters[0];
            int rightPeriod = (int)this.parameters[1];
            int inputSmoothing = (int)this.parameters[2];

            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH).CalculateEMA(inputSmoothing);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW).CalculateEMA(inputSmoothing);

            int startIndex = leftPeriod;
            int endIndex = stockSerie.Count - 1 - rightPeriod;

            for (int i = startIndex; i < endIndex; i++)
            {
                int maxIndex = highSerie.FindMaxIndex(i - leftPeriod, i + rightPeriod);
                if (maxIndex == i)
                {
                    this.eventSeries[0][i + rightPeriod] = true;
                    drawingItems.Add(new Bullet2D(new PointF(i, highSerie[i]), 2));
                }
                int minIndex = lowSerie.FindMinIndex(i - leftPeriod, i + rightPeriod);
                if (minIndex == i)
                {
                    this.eventSeries[1][i + rightPeriod] = true;
                    drawingItems.Add(new Bullet2D(new PointF(i, lowSerie[i]), 2));
                }
            }
        }
    }
}

