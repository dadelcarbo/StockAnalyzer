using StockAnalyzer.StockDrawing;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockAutoDrawings
{
    public class StockAutoDrawing_HLBODY : StockAutoDrawingBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string Definition => "Draws support/resistance based on HL Body";

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 20 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(2, 500) };

        public override string[] SerieNames => new string[] { };
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { };
                }
                return seriePens;
            }
        }

        static Pen supportPen = new Pen(Brushes.DarkRed, 3) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };
        static Pen resistancePen = new Pen(Brushes.DarkGreen, 3) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.parameters[0];
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var bodyHighSerie = stockSerie.GetSerie(StockDataType.BODYHIGH);
            var bodyLowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);

            var highestInSerie = stockSerie.GetIndicator($"HIGHEST({period})").Series[0];
            var lowestInSerie = stockSerie.GetIndicator($"LOWEST({period})").Series[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            var brokenUpEvents = this.Events[Array.IndexOf<string>(this.EventNames, "BrokenUp")];
            var brokenDownEvents = this.Events[Array.IndexOf<string>(this.EventNames, "BrokenDown")];
            var upTrendEvents = this.Events[Array.IndexOf<string>(this.EventNames, "UpTrend")];
            var downTrendEvents = this.Events[Array.IndexOf<string>(this.EventNames, "DownTrend")];
            var longReentryEvents = this.Events[Array.IndexOf<string>(this.EventNames, "LongReentry")];
            var shortReentryEvents = this.Events[Array.IndexOf<string>(this.EventNames, "ShortReentry")];

            bool upTrend = false;
            bool upSwing = false;
            int i = period;
            PointF supportP1 = PointF.Empty, resistanceP1 = PointF.Empty;
            int upSwingStartIndex = 0, downSwingStartIndex = 0;
            for (; i < stockSerie.Count; i++)
            {
                if (highestInSerie[i] >= period)
                {
                    upTrend = true;
                    upSwing = true;
                    supportP1 = bodyLowSerie.GetMinPoint(0, i);
                    break;
                }
                else if (lowestInSerie[i] >= period)
                {
                    upTrend = false;
                    upSwing = false;
                    resistanceP1 = bodyHighSerie.GetMaxPoint(0, i);
                    break;
                }
            }
            for (; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] >= supportP1.Y)
                    {
                        if (upSwing)
                        {
                            if (lowestInSerie[i] > period) // End of up swing
                            {
                                upSwing = false;
                                downSwingStartIndex = i;
                                resistanceP1 = bodyHighSerie.GetMaxPoint(upSwingStartIndex, i);
                            }
                        }
                        else // down swing in up trend
                        {
                            if (highestInSerie[i] >= period)
                            {
                                upSwingStartIndex = i;
                                upSwing = true;
                                var support = new Segment2D(supportP1, new PointF(i, supportP1.Y), supportPen);
                                this.DrawingItems.Insert(0, support);
                                supportP1 = bodyLowSerie.GetMinPoint(downSwingStartIndex, i);
                                longReentryEvents[i] = true;
                            }
                        }
                    }
                    else
                    {
                        brokenDownEvents[i] = true;
                        upTrend = false;
                        resistanceP1 = bodyHighSerie.GetMaxPoint((int)supportP1.X, i);
                        var support = new Segment2D(supportP1, new PointF(i, supportP1.Y), supportPen);
                        this.DrawingItems.Insert(0, support);
                        supportP1 = PointF.Empty;
                    }
                }
                else
                {
                    if (closeSerie[i] <= resistanceP1.Y)
                    {
                        if (!upSwing)
                        {
                            if (highestInSerie[i] > period) // End of down swing
                            {
                                upSwing = true;
                                upSwingStartIndex = i;
                                supportP1 = bodyLowSerie.GetMinPoint(downSwingStartIndex, i);
                            }
                        }
                        else // up swing in down trend
                        {
                            if (lowestInSerie[i] >= period)  // End of upSwing
                            {
                                downSwingStartIndex = i;
                                upSwing = false;
                                var resistance = new Segment2D(resistanceP1, new PointF(i, resistanceP1.Y), resistancePen);
                                this.DrawingItems.Insert(0, resistance);
                                resistanceP1 = bodyHighSerie.GetMaxPoint(upSwingStartIndex, i);
                                shortReentryEvents[i] = true;
                            }
                        }
                    }
                    else
                    {
                        brokenUpEvents[i] = true;
                        upTrend = true;
                        supportP1 = bodyLowSerie.GetMinPoint((int)resistanceP1.X, i);
                        var resistance = new Segment2D(resistanceP1, new PointF(i, resistanceP1.Y), resistancePen);
                        this.DrawingItems.Insert(0, resistance);
                        resistanceP1 = PointF.Empty;
                    }
                }
                upTrendEvents[i] = upTrend;
                downTrendEvents[i] = !upTrend;
            }
            if (upTrend)
            {
                var support = new Segment2D(supportP1, new PointF(stockSerie.Count, supportP1.Y), supportPen);
                this.DrawingItems.Insert(0, support);
            }
            else
            {
                var resistance = new Segment2D(resistanceP1, new PointF(stockSerie.Count, resistanceP1.Y), resistancePen);
                this.DrawingItems.Insert(0, resistance);
            }
        }

        static string[] eventNames = null;
        public override string[] EventNames
        {
            get
            {
                if (eventNames == null)
                {
                    eventNames = new string[] { "BrokenUp", "BrokenDown", "LongReentry", "ShortReentry", "UpTrend", "DownTrend" };
                }
                return eventNames;
            }
        }
        static readonly bool[] isEvent = new bool[] { true, true, true, true, false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}