using System;
using System.Drawing;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_SUPPORT : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public override string Name
        {
            get { return "SUPPORT(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")"; }
        }
        public override string Definition
        {
            get { return "Detect most uptrending line during period"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period1", "Period2" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 100, 20 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }
        public override string[] SerieNames { get { return new string[] { "SUPPORT(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" }; } }


        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black) };
                }
                return seriePens;
            }
        }
        static HLine[] lines = null;
        public override HLine[] HorizontalLines
        {
            get
            {
                if (lines == null)
                {
                    lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)) };
                }
                return lines;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            var period1 = (int)this.parameters[0];
            var period2 = (int)this.parameters[1];
            var lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            var highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie gradientSerie = new FloatSerie(stockSerie.Count);
            // Create events
            this.CreateEventSeries(stockSerie.Count);

            try
            {
                if (!stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
                {
                    stockSerie.StockAnalysis.DrawingItems.Add(stockSerie.BarDuration, new StockDrawingItems());
                }
                DrawingItem.CreatePersistent = false;
                float supportGradient = float.MinValue;
                Line2D support = null;
                float resistanceGradient = float.MaxValue;
                Line2D resistance = null;
                for (int i = stockSerie.Count - period1 - 1; i < stockSerie.Count - period2 - 1; i++)
                {
                    for (int j = i + period2; j < stockSerie.Count - 1; j++)
                    {
                        var line = new Line2D(new PointF { X = i, Y = lowSerie[i] }, new PointF { X = j, Y = lowSerie[j] });
                        // Check if all points are above the support
                        var isSupport = true;
                        for (int k = stockSerie.Count - period1; k < stockSerie.Count - 1; k++)
                        {
                            if (k != i & k != j && line.IsAbovePoint(new PointF { X = k, Y = lowSerie[k] }))
                            {
                                isSupport = false;
                                break;
                            }
                        }
                        if (isSupport)
                        {
                            if (line.a > supportGradient)
                            {
                                supportGradient = line.a;
                                support = line;
                            }
                        }// Check if all points are below the resistance
                        line = new Line2D(new PointF { X = i, Y = highSerie[i] }, new PointF { X = j, Y = highSerie[j] });
                        var isResistance = true;
                        for (int k = stockSerie.Count - period1; k < stockSerie.Count - 1; k++)
                        {
                            if (k != i & k != j && !line.IsAbovePoint(new PointF { X = k, Y = highSerie[k] }))
                            {
                                isResistance = false;
                                break;
                            }
                        }
                        if (isResistance)
                        {
                            if (line.a < resistanceGradient)
                            {
                                resistanceGradient = line.a;
                                resistance = line;
                            }
                        }
                    }
                }
                if (support != null)
                {
                    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(support);
                    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(new Bullet2D(support.Point1, 3, Brushes.Black));
                    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(new Bullet2D(support.Point2, 3, Brushes.Black));
                    gradientSerie[stockSerie.Count - 1] = 100f * supportGradient / lowSerie[(int)support.Point1.X];
                    this.Events[0][stockSerie.Count - 1] = supportGradient > 0;
                    this.Events[1][stockSerie.Count - 1] = supportGradient < 0;
                }
                if (resistance != null)
                {
                    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(resistance);
                    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(new Bullet2D(resistance.Point1, 3, Brushes.Black));
                    stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Add(new Bullet2D(resistance.Point2, 3, Brushes.Black));
                }
                this.series[0] = gradientSerie;
                this.Series[0].Name = this.Name;
            }
            finally
            {
                DrawingItem.CreatePersistent = true;
            }

        }

        static string[] eventNames = new string[] { "Bull", "Bear", "TurnedPositive", "TurnedNegative" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
