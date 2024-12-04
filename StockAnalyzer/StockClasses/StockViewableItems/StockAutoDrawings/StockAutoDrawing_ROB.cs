using StockAnalyzer.StockDrawing;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    public class StockAutoDrawing_ROB : StockAutoDrawingBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Draws descending resistance and detect breakouts";

        public override string[] ParameterNames => new string[] { "LookbackPeriod", "PivotPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 200, 6 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500), new ParamRangeInt(2, 500) };

        public override string[] SerieNames => new string[] { };
        public override Pen[] SeriePens => seriePens ??= new Pen[] { };

        static readonly Pen resistancePen = new Pen(Brushes.DarkGreen, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Solid };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var lookbackPeriod = (int)this.parameters[0];
            var pivotPeriod = (int)this.parameters[1];
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var highSerie = stockSerie.GetSerie(StockDataType.HIGH);

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var brokenUpEvents = this.Events[Array.IndexOf<string>(this.EventNames, "BrokenUp")];
            var downTrendEvents = this.Events[Array.IndexOf<string>(this.EventNames, "DownTrend")];
            var upTrendEvents = this.Events[Array.IndexOf<string>(this.EventNames, "UpTrend")];

            PointF P = PointF.Empty;
            HalfLine2D previousResistance = null;
            for (int i = stockSerie.LastIndex - pivotPeriod; i >= stockSerie.LastIndex - lookbackPeriod + pivotPeriod; i--)
            {
                if (highSerie.IsTop(i, pivotPeriod))
                {
                    if (P.X == 0)
                    {
                        P = new PointF(i, highSerie[i]);
                    }
                    else
                    {
                        if (P.Y < highSerie[i])
                        {
                            var resistance = new HalfLine2D(new PointF(i, highSerie[i]), P, resistancePen);
                            if (previousResistance == null || previousResistance.Slope > resistance.Slope)
                            {
                                previousResistance = resistance;
                            }
                        }
                    }
                }
            }
            if (previousResistance != null)
            {
                this.DrawingItems.Insert(0, previousResistance);
                brokenUpEvents[stockSerie.LastIndex] = closeSerie[stockSerie.LastIndex] > previousResistance.ValueAtX(stockSerie.LastIndex) && closeSerie[stockSerie.LastIndex - 1] <= previousResistance.ValueAtX(stockSerie.LastIndex - 1);
                upTrendEvents[stockSerie.LastIndex] = closeSerie[stockSerie.LastIndex] > previousResistance.ValueAtX(stockSerie.LastIndex);
            }
        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                eventNames ??= new string[] { "BrokenUp", "DownTrend", "UpTrend" };
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}