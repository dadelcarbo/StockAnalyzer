using StockAnalyzer.Portofolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockPortfolio;
using StockAnalyzer.StockBinckPortfolio;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.GraphControls
{
    public partial class GraphCloseControl : GraphControl
    {
        #region DRAWING MEMBERS AND TYPES

        GraphDrawMode drawingMode = GraphDrawMode.Normal;
        override public GraphDrawMode DrawingMode
        {
            get { return drawingMode; }
            set
            {
                this.drawingMode = value;
                this.andrewPitchFork = null;
                this.XABCD = null;
            }
        }

        public bool Magnetism { get; set; }
        public bool HideIndicators { get; set; }
        public StockPortofolio Portofolio { get; set; }
        public StockAnalyzer.StockBinckPortfolio.StockPortfolio BinckPortofolio { get; set; }

        private FloatSerie secondaryFloatSerie;
        public FloatSerie SecondaryFloatSerie
        {
            get { return this.secondaryFloatSerie; }
            set
            {
                this.secondaryFloatSerie = value;
            }
        }

        private PointF selectedValuePoint;

        private AndrewPitchFork andrewPitchFork;
        private XABCD XABCD;

        protected bool ShowOrders { get { return StockAnalyzerSettings.Properties.Settings.Default.ShowOrders; } }
        protected bool ShowEventMarquee { get { return StockAnalyzerSettings.Properties.Settings.Default.ShowEventMarquee; } }
        protected bool ShowCommentMarquee { get { return StockAnalyzerSettings.Properties.Settings.Default.ShowCommentMarquee; } }
        protected AgendaEntryType ShowAgenda { get { return (AgendaEntryType)Enum.Parse(typeof(AgendaEntryType), StockAnalyzerSettings.Properties.Settings.Default.ShowAgenda); } }
        protected bool ShowIndicatorDiv { get { return StockAnalyzerSettings.Properties.Settings.Default.ShowIndicatorDiv; } }
        protected bool ShowIndicatorText { get { return StockAnalyzerSettings.Properties.Settings.Default.ShowIndicatorText; } }


        GraphCurveType closeCurveType = null;
        GraphCurveType openCurveType = null;
        GraphCurveType highCurveType = null;
        GraphCurveType lowCurveType = null;

        public override GraphCurveTypeList CurveList
        {
            get
            {
                return this.curveList;
            }
            set
            {
                this.curveList = value;
                if (value != null)
                {
                    closeCurveType = CurveList.Find(c => c.DataSerie.Name == "CLOSE");
                    openCurveType = CurveList.Find(c => c.DataSerie.Name == "OPEN");
                    highCurveType = CurveList.Find(c => c.DataSerie.Name == "HIGH");
                    lowCurveType = CurveList.Find(c => c.DataSerie.Name == "LOW");
                }
            }
        }
        public Dictionary<DateTime, String> Comments { get; set; }
        public StockAgenda Agenda { get; set; }

        // Secondary serie management
        protected System.Drawing.Drawing2D.Matrix matrixSecondaryScreenToValue;
        protected System.Drawing.Drawing2D.Matrix matrixSecondaryValueToScreen;
        public Pen SecondaryPen { get; set; }

        #endregion

        public delegate void PointPickEventHandler(int index, DateTime date);
        public event PointPickEventHandler PointPick;

        override protected bool InitializeTransformMatrix()
        {
            if (base.InitializeTransformMatrix())
            {
                this.InitializeSecondaryTransformMatrix();
                return true;
            }
            return false;
        }
        private void InitializeSecondaryTransformMatrix()
        {
            if (this.SecondaryFloatSerie != null)
            {
                if (this.GraphRectangle.Height > 0)
                {
                    float minValue = float.MaxValue, maxValue = float.MinValue;
                    SecondaryFloatSerie.GetMinMax(StartIndex, EndIndex, ref minValue, ref maxValue);

                    // Add a margin
                    minValue -= (maxValue - minValue) * 0.05f;
                    maxValue += (maxValue - minValue) * 0.05f;

                    float coefX = (this.GraphRectangle.Width * 0.96f) / (EndIndex - StartIndex);
                    float coefY = this.GraphRectangle.Height / (maxValue - minValue);

                    matrixSecondaryValueToScreen = new System.Drawing.Drawing2D.Matrix();
                    matrixSecondaryValueToScreen.Translate(this.GraphRectangle.X - (StartIndex - 0.5f) * coefX, maxValue * coefY + this.GraphRectangle.Y);
                    matrixSecondaryValueToScreen.Scale(coefX, -coefY);

                    matrixSecondaryScreenToValue = (System.Drawing.Drawing2D.Matrix)matrixValueToScreen.Clone();
                    matrixSecondaryScreenToValue.Invert();
                }
            }
        }
        override protected void SetFrameMargin()
        {
            this.XMargin = MARGIN_SIZE * 2;
            this.YMargin = MARGIN_SIZE;
        }
        #region PAINT METHODS
        override protected void PaintCopyright(Graphics aGraphic)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                string graphCopyright = "Copyright © " + DateTime.Today.Year + " www.ultimatechartist.com";

                Size size = TextRenderer.MeasureText(graphCopyright, this.axisFont);
                PointF point = new PointF(aGraphic.VisibleClipBounds.Right - size.Width + 10, 5);

                this.DrawString(aGraphic, graphCopyright, this.axisFont, Brushes.Black, this.backgroundBrush, point, false);
            }
        }

        protected override void PaintTmpGraph(Graphics aGraphic)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                #region Draw Grid

                // Draw grid
                if (this.ShowGrid)
                {
                    #region Draw Horizontal lines

                    float step = (float)Math.Pow(10, Math.Floor(Math.Log10((maxValue - minValue))));
                    if ((maxValue - minValue) / step < 3)
                    {
                        step /= 4;
                    }
                    else if ((maxValue - minValue) / step < 6)
                    {
                        step /= 2;
                    }
                    else
                    {
                        if ((maxValue - minValue) / step > 13)
                        {
                            step *= 4;
                        }
                        else if ((maxValue - minValue) / step > 7)
                        {
                            step *= 2;
                        }
                    }
                    float val;
                    if (minValue < 0)
                    {
                        val = -(float)Math.Pow(10, Math.Ceiling(Math.Log10((Math.Abs(minValue)))));
                    }
                    else
                    {
                        val = (float)Math.Pow(10, Math.Floor(Math.Log10((minValue))));
                    }

                    if (val > 0 && step > val) val = step;
                    if (val < 0 && step > Math.Abs(val)) val = -step;

                    PointF p1;
                    while (val < maxValue)
                    {
                        if (val > minValue)
                        {
                            p1 = GetScreenPointFromValuePoint(this.StartIndex, val);
                            aGraphic.DrawLine(gridPen, GraphRectangle.X, p1.Y, GraphRectangle.X + GraphRectangle.Width, p1.Y);
                            aGraphic.DrawString(val.ToString("0.##"), axisFont, Brushes.Black, 0, p1.Y - 8);
                        }
                        val += step;
                    }

                    #endregion

                    #region Draw vertical lines

                    DrawVerticalGridLines(aGraphic, true, this.StartIndex, this.EndIndex);

                    #endregion
                }
                aGraphic.DrawString(this.dateSerie[this.EndIndex].ToString("dd/MM"), axisFont, Brushes.Black,
                   GraphRectangle.Right - 3, GraphRectangle.Y + GraphRectangle.Height);
                aGraphic.DrawString(this.dateSerie[this.EndIndex].ToString("yyyy"), axisFont, Brushes.Black,
                   GraphRectangle.Right - 1, GraphRectangle.Y + GraphRectangle.Height + 8);

                #endregion

                #region Draw HLine
                // Paint horizontal lines first
                this.PaintHorizontalLines(aGraphic);
                #endregion

                #region Draw orders

                if (ShowOrders && this.BinckPortofolio != null)
                {
                    PaintBinckOrders(aGraphic);
                }

                #endregion

                #region Draw values and curves

                PointF[] tmpPoints = null;
                PointF[] tmpOpenPoints = null;
                PointF[] tmpHighPoints = null;
                PointF[] tmpLowPoints = null;
                // Draw indicator1Name first not to hide the value
                if (!HideIndicators)
                {
                    #region DISPLAY CLOUD
                    if (this.CurveList.Cloud != null && this.CurveList.Cloud.Series[0].Count > 0)
                    {
                        var bullColor = Color.FromArgb(127, this.CurveList.Cloud.SeriePens[0].Color.R, this.CurveList.Cloud.SeriePens[0].Color.G, this.CurveList.Cloud.SeriePens[0].Color.B);
                        var bullBrush = new SolidBrush(bullColor);
                        var bullPen = this.CurveList.Cloud.SeriePens[0];

                        var bearColor = Color.FromArgb(127, this.CurveList.Cloud.SeriePens[1].Color.R, this.CurveList.Cloud.SeriePens[1].Color.G, this.CurveList.Cloud.SeriePens[1].Color.B);
                        var bearBrush = new SolidBrush(bearColor);
                        var bearPen = this.CurveList.Cloud.SeriePens[1];

                        var bullPoints = GetScreenPoints(StartIndex, EndIndex, this.CurveList.Cloud.Series[0]);
                        var bearPoints = GetScreenPoints(StartIndex, EndIndex, this.CurveList.Cloud.Series[1]);

                        bool isBull = bullPoints[0].Y >= bearPoints[0].Y;
                        var nbPoints = bullPoints.Length;
                        var upPoints = new List<PointF>() { bullPoints[0] };
                        var downPoints = new List<PointF>() { bearPoints[0] };
                        for (int i = 1; i < nbPoints; i++)
                        {
                            if (isBull && bullPoints[i].Y >= bearPoints[i].Y) // Bull cloud continuing
                            {
                                upPoints.Add(bullPoints[i]);
                                downPoints.Insert(0, bearPoints[i]);
                            }
                            else if (!isBull && bullPoints[i].Y < bearPoints[i].Y) // Bear cloud continuing
                            {
                                upPoints.Add(bullPoints[i]);
                                downPoints.Insert(0, bearPoints[i]);
                            }
                            else // Cloud reversing, need a draw
                            {
                                if (upPoints.Count > 0)
                                {
                                    upPoints.Add(bullPoints[i]);
                                    downPoints.Insert(0, bearPoints[i]);
                                    aGraphic.DrawLines(isBull ? bullPen : bearPen, upPoints.ToArray());
                                    aGraphic.DrawLines(isBull ? bullPen : bearPen, downPoints.ToArray());
                                    upPoints.AddRange(downPoints);
                                    aGraphic.FillPolygon(isBull ? bullBrush : bearBrush, upPoints.ToArray());
                                }
                                isBull = !isBull;
                                upPoints.Clear();
                                downPoints.Clear();
                                upPoints = new List<PointF>() { bullPoints[i] };
                                downPoints = new List<PointF>() { bearPoints[i] };
                            }
                        }
                        if (upPoints.Count > 0)
                        {
                            aGraphic.DrawLines(isBull ? bullPen : bearPen, upPoints.ToArray());
                            aGraphic.DrawLines(isBull ? bullPen : bearPen, downPoints.ToArray());
                            upPoints.AddRange(downPoints);
                            aGraphic.FillPolygon(isBull ? bullBrush : bearBrush, upPoints.ToArray());
                            upPoints.Clear();
                            downPoints.Clear();
                        }
                        bullBrush.Dispose();
                        bearBrush.Dispose();
                    }
                    #endregion
                    #region DISPLAY TRAIL STOPS

                    if (this.CurveList.TrailStop != null && this.CurveList.TrailStop.Series[0].Count > 0)
                    {
                        FloatSerie longStopSerie = this.CurveList.TrailStop.Series[0];
                        FloatSerie shortStopSerie = this.CurveList.TrailStop.Series[1];
                        Pen longPen = this.CurveList.TrailStop.SeriePens[0];
                        Pen shortPen = this.CurveList.TrailStop.SeriePens[1];

                        using (Brush longBrush = new SolidBrush(longPen.Color))
                        {
                            using (Brush shortBrush = new SolidBrush(shortPen.Color))
                            {
                                PointF srPoint1;
                                PointF srPoint2;
                                if (float.IsNaN(shortStopSerie[this.StartIndex]))
                                {
                                    srPoint1 = GetScreenPointFromValuePoint(this.StartIndex, longStopSerie[this.StartIndex]);
                                }
                                else
                                {
                                    srPoint1 = GetScreenPointFromValuePoint(this.StartIndex, shortStopSerie[this.StartIndex]);
                                }
                                for (int i = StartIndex + 1; i <= this.EndIndex; i++)
                                {
                                    bool isSupport = float.IsNaN(shortStopSerie[i]);
                                    if (isSupport) // upTrend
                                    {
                                        srPoint2 = GetScreenPointFromValuePoint(i, longStopSerie[i]);
                                        aGraphic.DrawLine(longPen, srPoint1, srPoint2);
                                    }
                                    else
                                    {
                                        srPoint2 = GetScreenPointFromValuePoint(i, shortStopSerie[i]);
                                        aGraphic.DrawLine(shortPen, srPoint1, srPoint2);
                                    }
                                    srPoint1 = srPoint2;

                                    if (this.ShowIndicatorText)
                                    {
                                        const int textOffset = 4;

                                        float yPos = float.IsNaN(shortStopSerie[i])
                                           ? srPoint1.Y + 2
                                           : srPoint1.Y - 2 * 2 - 12;

                                        // Draw PB and EndOfTrend text
                                        if (this.CurveList.TrailStop.Events[2][i])
                                        {
                                            if (isSupport)
                                            {
                                                // Pullback in up trend detected
                                                this.DrawString(aGraphic, "PB", axisFont, longBrush,
                                                   this.backgroundBrush, srPoint1.X - textOffset, yPos, false);
                                            }
                                            else
                                            {
                                                // Pullback in down trend detected
                                                this.DrawString(aGraphic, "PB", axisFont, shortBrush,
                                                   this.backgroundBrush, srPoint1.X - textOffset, yPos, false);
                                            }
                                        }
                                        else if (this.CurveList.TrailStop.Events[3][i])
                                        {
                                            if (isSupport)
                                            {
                                                // End of down trend detected
                                                this.DrawString(aGraphic, "End", axisFont, shortBrush,
                                               this.backgroundBrush, srPoint1.X - textOffset,
                                               yPos, false);
                                            }
                                            else
                                            {
                                                // End of down trend detected
                                                this.DrawString(aGraphic, "End", axisFont, longBrush,
                                               this.backgroundBrush, srPoint1.X - textOffset,
                                               yPos, false);

                                            }
                                        }
                                        else
                                        {
                                            if (this.CurveList.TrailStop.Events[4][i])
                                            {
                                                this.DrawString(aGraphic, "HL", axisFont, longBrush,
                                                   this.backgroundBrush,
                                                   srPoint1.X - textOffset, yPos, false);

                                            }
                                            if (this.CurveList.TrailStop.Events[5][i])
                                            {
                                                this.DrawString(aGraphic, "LH", axisFont, shortBrush,
                                                   this.backgroundBrush,
                                                   srPoint1.X - textOffset, yPos, false);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region DISPLAY INDICATORS

                    foreach (var stockIndicator in CurveList.Indicators)
                    {
                        for (int i = 0; i < stockIndicator.SeriesCount; i++)
                        {
                            if (stockIndicator.SerieVisibility[i] && stockIndicator.Series[i].Count > 0)
                            {
                                bool isHilbertSR = stockIndicator.Name.StartsWith("HILBERT");
                                bool isTrailSR = stockIndicator.Name.StartsWith("TRAILHLSR");
                                bool isSupport = stockIndicator.SerieNames[i].EndsWith(".S");
                                bool isResistance = stockIndicator.SerieNames[i].EndsWith(".R");
                                if (isSupport || isResistance)
                                {
                                    PointF srPoint = PointF.Empty;
                                    FloatSerie srSerie = stockIndicator.Series[i];
                                    float pointSize = stockIndicator.SeriePens[i].Width;
                                    using (Brush srBrush = new SolidBrush(stockIndicator.SeriePens[i].Color))
                                    {
                                        for (int index = this.StartIndex; index <= this.EndIndex; index++)
                                        {
                                            float sr = srSerie.Values[index];
                                            if (float.IsNaN(sr))
                                            {
                                                continue;
                                            }
                                            srPoint = GetScreenPointFromValuePoint(index, sr);
                                            aGraphic.FillEllipse(srBrush, srPoint.X - pointSize, srPoint.Y - pointSize,
                                               2 * pointSize, 2 * pointSize);


                                            if (this.ShowIndicatorText)
                                            {
                                                const int textOffset = 4;

                                                float yPos = isSupport
                                                   ? srPoint.Y + pointSize
                                                   : srPoint.Y - 2 * pointSize - 12;

                                                // Draw PB and EndOfTrend text
                                                if (stockIndicator.Events[2][index])
                                                {
                                                    // Pullback in trend detected
                                                    this.DrawString(aGraphic, "PB", axisFont, srBrush,
                                                       this.backgroundBrush, srPoint.X - textOffset, yPos, false);
                                                }
                                                else if (stockIndicator.Events[3][index])
                                                {
                                                    // End of trend detected
                                                    this.DrawString(aGraphic, "End", axisFont, srBrush,
                                                       this.backgroundBrush, srPoint.X - textOffset,
                                                       yPos, false);
                                                }
                                                else if (!isHilbertSR)
                                                {
                                                    if (isSupport && stockIndicator.Events[4][index])
                                                    {
                                                        this.DrawString(aGraphic, "HL", axisFont, srBrush,
                                                           this.backgroundBrush,
                                                           srPoint.X - textOffset, yPos, false);

                                                    }
                                                    if (isResistance && stockIndicator.Events[5][index])
                                                    {
                                                        this.DrawString(aGraphic, "LH", axisFont, srBrush,
                                                           this.backgroundBrush,
                                                           srPoint.X - textOffset, yPos, false);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    tmpPoints = GetScreenPoints(StartIndex, EndIndex, stockIndicator.Series[i]);
                                    if (tmpPoints != null)
                                    {
                                        aGraphic.DrawLines(stockIndicator.SeriePens[i], tmpPoints);
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region DISPLAY DECORATORS
                    if (this.ShowIndicatorDiv && CurveList.ShowMes.Count > 0)
                    {
                        for (int j = 0; j < CurveList.ShowMes.Count; j++)
                        {
                            IStockDecorator decorator = CurveList.ShowMes[j];
                            for (int i = 0; i < decorator.EventCount; i++)
                            {
                                if (decorator.EventVisibility[i] && decorator.IsEvent[i] && decorator.Events[i].Count > 0)
                                {
                                    FloatSerie dataSerie = (i % 2 == 0) ? highCurveType.DataSerie : lowCurveType.DataSerie;
                                    Pen pen = decorator.EventPens[i];
                                    using (Brush brush = new SolidBrush(pen.Color))
                                    {
                                        BoolSerie decoSerie = decorator.Events[i];
                                        for (int index = this.StartIndex; index <= this.EndIndex; index++)
                                        {
                                            if (decoSerie[index])
                                            {
                                                PointF point = new PointF(index, dataSerie[index]);
                                                PointF point2 = GetScreenPointFromValuePoint(point);
                                                aGraphic.FillEllipse(brush, point2.X - pen.Width * 1.5f, point2.Y - pen.Width * 1.5f,
                                                   pen.Width * 3f, pen.Width * 3f);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }

                #endregion

                #region Display drawing items

                if (this.ShowDrawings && this.drawingItems != null)
                {
                    foreach (DrawingItem item in this.drawingItems)
                    {
                        item.Draw(aGraphic, this.matrixValueToScreen, new Rectangle2D(this.GraphRectangle), this.IsLogScale);
                        // display support résistance value
                        if (item.GetType() == typeof(Line2D))
                        {
                            Line2D line = (Line2D)item;
                            if (line.IsHorizontal)
                            {
                                PointF textLocation = GetScreenPointFromValuePoint(new PointF(StartIndex, line.Point1.Y));
                                this.DrawString(aGraphic, line.Point1.Y.ToString("0.##"), axisFont, textBrush, backgroundBrush,
                                   new PointF(1, textLocation.Y - 8), true);
                            }
                        }
                    }
                }

                #endregion

                #region Display the stock value

                // Then draw the value
                if (closeCurveType != null && closeCurveType.IsVisible)
                {
                    tmpPoints = GetScreenPoints(StartIndex, EndIndex, closeCurveType.DataSerie);

                    // Store in member for future use (Display mouse marquee)
                    //this.points = tmpPoints;
                    switch (this.ChartMode)
                    {
                        case GraphChartMode.Line:
                            aGraphic.DrawLines(closeCurveType.CurvePen, tmpPoints);
                            break;
                        case GraphChartMode.BarChart:
                            {
                                FloatSerie openSerie = openCurveType.DataSerie;
                                FloatSerie highSerie = highCurveType.DataSerie;
                                FloatSerie lowSerie = lowCurveType.DataSerie;

                                tmpOpenPoints = GetScreenPoints(StartIndex, EndIndex, openSerie);
                                tmpHighPoints = GetScreenPoints(StartIndex, EndIndex, highSerie);
                                tmpLowPoints = GetScreenPoints(StartIndex, EndIndex, lowSerie);

                                OHLCBar bar = new OHLCBar();
                                bar.Width = 0.40f * aGraphic.VisibleClipBounds.Width / tmpPoints.Count();
                                Pen barPen;
                                for (int i = 0; i < tmpPoints.Count(); i++)
                                {
                                    barPen = this.closeCurveType.CurvePen;
                                    if (!this.HideIndicators && this.CurveList.PaintBar != null)
                                    {
                                        // Get pen from paintBar
                                        IStockPaintBar pb = this.CurveList.PaintBar;
                                        if (pb.Events[0].Count == this.dateSerie.Length)
                                        {
                                            for (int pbIndex = 0; pbIndex < pb.SeriesCount; pbIndex++)
                                            {
                                                if (pb.SerieVisibility[pbIndex] && pb.Events[pbIndex][i + this.StartIndex])
                                                {
                                                    barPen = pb.SeriePens[pbIndex];
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    bar.X = tmpPoints[i].X;
                                    bar.Close = tmpPoints[i].Y;
                                    bar.High = tmpHighPoints[i].Y;
                                    bar.Open = tmpOpenPoints[i].Y;
                                    bar.Low = tmpLowPoints[i].Y;

                                    bar.Draw(aGraphic, barPen);
                                }
                            }
                            break;
                        case GraphChartMode.CandleStick:
                            {
                                FloatSerie openSerie = openCurveType.DataSerie;
                                FloatSerie highSerie = highCurveType.DataSerie;
                                FloatSerie lowSerie = lowCurveType.DataSerie;

                                tmpOpenPoints = GetScreenPoints(StartIndex, EndIndex, openSerie);
                                tmpHighPoints = GetScreenPoints(StartIndex, EndIndex, highSerie);
                                tmpLowPoints = GetScreenPoints(StartIndex, EndIndex, lowSerie);

                                CandleStick candleStick = new CandleStick();
                                candleStick.Width = (int)(0.40f * aGraphic.VisibleClipBounds.Width / tmpPoints.Count());
                                for (int i = 0; i < tmpPoints.Count(); i++)
                                {
                                    candleStick.X = (int)tmpPoints[i].X;
                                    candleStick.Close = (int)tmpPoints[i].Y;
                                    candleStick.High = (int)tmpHighPoints[i].Y;
                                    candleStick.Open = (int)tmpOpenPoints[i].Y;
                                    candleStick.Low = (int)tmpLowPoints[i].Y;

                                    Color? color = null;
                                    if (!this.HideIndicators && this.CurveList.PaintBar != null)
                                    {
                                        // Get pen from paintBar
                                        IStockPaintBar pb = this.CurveList.PaintBar;
                                        if (pb.Events[0].Count == this.dateSerie.Length)
                                        {
                                            for (int pbIndex = 0; pbIndex < pb.SeriesCount; pbIndex++)
                                            {
                                                if (pb.SerieVisibility[pbIndex] && pb.Events[pbIndex][i + this.StartIndex])
                                                {
                                                    color = pb.SeriePens[pbIndex].Color;
                                                    break;
                                                }
                                            }
                                        }
                                        if (color == null)
                                        {
                                            candleStick.Draw(aGraphic, closeCurveType.CurvePen, backgroundBrush);
                                        }
                                        else
                                        {
                                            using (Brush brush = new SolidBrush(color.Value))
                                            {
                                                candleStick.Draw(aGraphic, closeCurveType.CurvePen, brush);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        candleStick.Draw(aGraphic, closeCurveType.CurvePen, null);
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                #endregion

                #region Display the secondary stock value

                if (this.SecondaryFloatSerie != null)
                {
                    PointF[] secondaryPoints = GetSecondaryScreenPoints(StartIndex, EndIndex);
                    if (secondaryPoints.Length > 0)
                    {
                        aGraphic.DrawLines(SecondaryPen, secondaryPoints);
                    }
                }

                #endregion

                #region Draw frame, axis and axis values

                // Draw main frame
                aGraphic.DrawRectangle(framePen, GraphRectangle.X, GraphRectangle.Y, GraphRectangle.Width,
                   GraphRectangle.Height);

                // Display values and dates
                string lastValue = closeCurveType.DataSerie[EndIndex].ToString();
                aGraphic.DrawString(lastValue, axisFont, Brushes.Black, GraphRectangle.Right + 1,
                   tmpPoints[tmpPoints.Count() - 1].Y - 8);

                #endregion

                #region Display event marquee

                if (this.ShowEventMarquee)
                {
                    bool eventFound = false;
                    for (int i = this.StartIndex; i <= this.EndIndex; i++)
                    {
                        eventFound = false;
                        // Indicators
                        foreach (IStockIndicator indicator in this.CurveList.Indicators.Where(indic => indic.Events != null))
                        {
                            for (int j = 0; j < indicator.EventCount; j++)
                            {
                                if (indicator.IsEvent[j] && indicator.Events[j] != null && indicator.Events.Count() > 0)
                                {
                                    BoolSerie eventSerie = indicator.Events[j];

                                    if (eventSerie[i])
                                    {
                                        eventFound = true;
                                        break;
                                    }
                                }
                            }
                            if (eventFound) break;
                        }
                        // Trail Stops
                        if (!eventFound && this.CurveList.TrailStop != null && this.CurveList.TrailStop.EventCount > 0)
                        {
                            for (int j = 0; j < this.CurveList.TrailStop.EventCount; j++)
                            {
                                if (this.CurveList.TrailStop.IsEvent[j] && this.CurveList.TrailStop.Events[j] != null &&
                                    this.CurveList.TrailStop.Events.Count() > 0)
                                {
                                    BoolSerie eventSerie = this.CurveList.TrailStop.Events[j];
                                    if (eventSerie[i])
                                    {
                                        eventFound = true;
                                        break;
                                    }
                                }
                            }
                        }
                        // Paint bars
                        if (!eventFound && this.CurveList.PaintBar != null && this.CurveList.PaintBar.EventCount > 0)
                        {
                            int j = 0;
                            foreach (
                               BoolSerie eventSerie in this.CurveList.PaintBar.Events.Where(ev => ev != null && ev.Count > 0))
                            {
                                if (this.CurveList.PaintBar.SerieVisibility[j] && this.CurveList.PaintBar.IsEvent != null &&
                                    this.CurveList.PaintBar.IsEvent[j] && eventSerie[i])
                                {
                                    eventFound = true;
                                    break;
                                }
                                j++;
                            }
                        }
                        // Cloud
                        if (eventFound)
                        {
                            PointF[] marqueePoints = GetEventMarqueePointsAtIndex(i);
                            aGraphic.FillPolygon(Brushes.DarkBlue, marqueePoints);
                        }
                    }
                }

                #endregion

                #region Display comment marquee

                if (this.ShowCommentMarquee)
                {
                    for (int i = StartIndex; i <= EndIndex; i++)
                    {
                        DateTime commentDate = this.dateSerie[i];
                        if (this.Comments.ContainsKey(commentDate))
                        {
                            string comment = this.Comments[commentDate];
                            if (!string.IsNullOrWhiteSpace(comment))
                            {
                                PointF[] marqueePoints = GetCommentMarqueePointsAtIndex(i);
                                aGraphic.FillPolygon(Brushes.DarkBlue, marqueePoints);
                            }
                        }
                    }
                }
                if (this.Agenda != null && this.ShowAgenda != AgendaEntryType.No)
                {
                    for (int i = StartIndex; i <= EndIndex; i++)
                    {
                        DateTime agendaDate = this.dateSerie[i];
                        if (this.Agenda.ContainsKey(agendaDate))
                        {
                            var agendaEntry = this.Agenda[agendaDate];
                            if (agendaEntry.IsOfType(this.ShowAgenda))
                            {
                                PointF[] marqueePoints = GetCommentMarqueePointsAtIndex(i);
                                aGraphic.FillPolygon(Brushes.DarkCyan, marqueePoints);
                            }
                        }
                    }
                }
                #endregion
            }
        }

        // Input point are in Value Units
        protected override void PaintDailyBox(PointF mousePoint)
        {
            if (lastMouseIndex == -1) return;
            string value = string.Empty;
            value += BuildTabbedString("DATE", this.dateSerie[lastMouseIndex].ToString("dd/MM/yy"), 12) + "\r\n";
            if (this.dateSerie[lastMouseIndex].Hour != 0)
            {
                value += BuildTabbedString("TIME", this.dateSerie[lastMouseIndex].ToShortTimeString(), 12) + "\r\n";
            }
            float closeValue = float.NaN;
            foreach (GraphCurveType curveType in this.CurveList)
            {
                if (!float.IsNaN(curveType.DataSerie[this.lastMouseIndex]))
                {
                    if (closeCurveType.DataSerie.Name == "CLOSE")
                        closeValue = curveType.DataSerie[this.lastMouseIndex];
                    if (curveType.DataSerie.Name.Length > 6)
                    {
                        value += BuildTabbedString(curveType.DataSerie.Name, curveType.DataSerie[this.lastMouseIndex], 12) + "\r\n";
                    }
                    else
                    {
                        value += BuildTabbedString(curveType.DataSerie.Name, curveType.DataSerie[this.lastMouseIndex], 12) + "\r\n";
                    }
                }
            }
            // Add indicators
            foreach (IStockIndicator stockIndicator in CurveList.Indicators)
            {
                for (int i = 0; i < stockIndicator.SeriesCount; i++)
                {
                    if (stockIndicator.SerieVisibility[i] && stockIndicator.Series[i].Count > 0 && !float.IsNaN(stockIndicator.Series[i][this.lastMouseIndex]))
                    {
                        value += BuildTabbedString(stockIndicator.Series[i].Name, stockIndicator.Series[i][this.lastMouseIndex], 12) + "\r\n";
                    }
                }
            }                       
            // Cloud

            // Add Trail Stops
            var trailValue = float.NaN;
            var trailName = string.Empty;
            if (CurveList.TrailStop != null)
            {
                for (int i = 0; i < CurveList.TrailStop.SeriesCount; i++)
                {
                    if (CurveList.TrailStop.Series[i] != null && CurveList.TrailStop.Series[i].Count > 0 && !float.IsNaN(CurveList.TrailStop.Series[i][this.lastMouseIndex]))
                    {
                        trailName = CurveList.TrailStop.Series[i].Name;
                        trailValue = CurveList.TrailStop.Series[i][this.lastMouseIndex];
                        value += BuildTabbedString(trailName, trailValue, 12) + "\r\n";
                    }
                }
            }
            if (!float.IsNaN(trailValue))
            {
                value += BuildTabbedString(trailName, (Math.Abs(trailValue - closeValue) / closeValue).ToString("P2"), 12) + "\r\n";
            }
            // Add secondary serie
            if (this.secondaryFloatSerie != null)
            {
                value += BuildTabbedString(this.secondaryFloatSerie.Name, this.secondaryFloatSerie[this.lastMouseIndex], 12) + "\r\n";
            }

            if (StockAnalyzerForm.MainFrame.CurrentStockSerie != null && StockAnalyzerForm.MainFrame.CurrentStockSerie.IsInitialised && StockAnalyzerForm.MainFrame.CurrentStockSerie.LastIndex == this.lastMouseIndex)
            {
                value += BuildTabbedString("COMPLETE", StockAnalyzerForm.MainFrame.CurrentStockSerie.ValueArray[this.lastMouseIndex].IsComplete.ToString(), 12) + "\r\n";
            }
            // Remove last new line.
            if (value.Length != 0)
            {
                value = value.Remove(value.LastIndexOf("\r\n"));
            }

            // Draw it now
            Size size = TextRenderer.MeasureText(value, toolTipFont);

            PointF point = new PointF(Math.Min(mousePoint.X + 10, GraphRectangle.Right - size.Width), Math.Min(mousePoint.Y + 10, GraphRectangle.Bottom - size.Height));

            this.DrawString(this.foregroundGraphic, value, toolTipFont, Brushes.Black, this.backgroundBrush, point, true);

        }
        protected override void PaintGraphTitle(Graphics gr)
        {
            string graphTitle = this.serieName;

            // Add PaintBars, SR, Trail...
            foreach (IStockIndicator indicator in this.CurveList.Indicators)
            {
                graphTitle += " " + (indicator.Name);
            }
            if (this.CurveList.Cloud != null)
            {
                graphTitle += " " + (this.CurveList.Cloud.Name);
            }
            if (this.CurveList.PaintBar != null)
            {
                graphTitle += " " + (this.CurveList.PaintBar.Name);
            }
            if (this.CurveList.TrailStop != null)
            {
                graphTitle += " " + (this.CurveList.TrailStop.Name);
            }
            float right = this.DrawString(gr, graphTitle, this.axisFont, Brushes.Black, this.backgroundBrush, new PointF(1, 1), true);
            if (this.SecondaryFloatSerie != null)
            {
                graphTitle = this.SecondaryFloatSerie.Name;
                using (Brush textBrush = new SolidBrush(SecondaryPen.Color))
                {
                    this.DrawString(gr, graphTitle, this.axisFont, textBrush, this.backgroundBrush, new PointF(right + 16, 1), true);
                }
            }
        }
        private void PaintBinckOrders(Graphics graphic)
        {
            if (this.BinckPortofolio == null)
            {
                return;
            }
            var name = this.serieName.ToUpper();
            PointF valuePoint2D = PointF.Empty;
            PointF screenPoint2D = PointF.Empty;
            foreach (var operation in this.BinckPortofolio.Operations.Where(p => p.Date >= this.dateSerie[this.StartIndex] && p.Date <= this.dateSerie[this.EndIndex] && p.StockName.ToUpper() == name && p.IsOrder))
            {
                DateTime orderDate = serieName.StartsWith("INT_") ? operation.Date : operation.Date.Date;
                int index = this.IndexOf(orderDate);
                valuePoint2D.X = index;
                if (valuePoint2D.X < 0)
                {
                    StockLog.Write("Order date not found: " + operation.Date);
                }
                else
                {
                    if (operation.OperationType == StockOperation.BUY)
                    {
                        valuePoint2D.Y = operation.IsShort ? this.highCurveType.DataSerie[index] : this.lowCurveType.DataSerie[index];
                        screenPoint2D = this.GetScreenPointFromValuePoint(valuePoint2D);
                        this.DrawArrow(graphic, screenPoint2D, true, operation.IsShort);
                    }
                    else
                    {
                        valuePoint2D.Y = operation.IsShort ? this.lowCurveType.DataSerie[index] : this.highCurveType.DataSerie[index];
                        screenPoint2D = this.GetScreenPointFromValuePoint(valuePoint2D);
                        this.DrawArrow(graphic, screenPoint2D, false, operation.IsShort);
                    }
                }
            }
        }

        private void PaintOrders(Graphics graphic)
        {
            if (this.Portofolio == null || this.Portofolio.OrderList.Count == 0)
            {
                return;
            }
            PointF valuePoint2D = PointF.Empty;
            PointF screenPoint2D = PointF.Empty;
            foreach (StockOrder stockOrder in this.Portofolio.OrderList.FindAll(order => order.StockName == this.serieName && order.ExecutionDate >= this.dateSerie[this.StartIndex] && order.ExecutionDate.Date <= this.dateSerie[this.EndIndex]))
            {
                DateTime orderDate = serieName.StartsWith("INT_") ? stockOrder.ExecutionDate : stockOrder.ExecutionDate.Date;
                valuePoint2D.X = this.IndexOf(orderDate);
                if (valuePoint2D.X < 0)
                {
                    StockLog.Write("Order date not found: " + stockOrder.ExecutionDate);
                }
                else
                {
                    screenPoint2D = GetScreenPointFromOrder(stockOrder);

                    this.DrawArrow(graphic, screenPoint2D, stockOrder.IsBuyOrder(), stockOrder.IsShortOrder);
                }
            }
        }

        static Pen buyLongPen = new Pen(Color.Green, 5) { StartCap = LineCap.Square, EndCap = LineCap.ArrowAnchor };
        static Pen buyShortPen = new Pen(Color.Red, 5) { StartCap = LineCap.RoundAnchor, EndCap = LineCap.ArrowAnchor };
        static Pen sellLongPen = new Pen(Color.Red, 5) { StartCap = LineCap.Square, EndCap = LineCap.ArrowAnchor };
        static Pen sellShortPen = new Pen(Color.Green, 5) { StartCap = LineCap.RoundAnchor, EndCap = LineCap.ArrowAnchor };

        private void DrawArrow(Graphics g, PointF point, bool isBuy, bool isShort)
        {
            int arrowLengh = 15;
            float offset = 10;
            isBuy = isShort ? !isBuy : isBuy;
            Pen p = buyLongPen;
            if (isBuy)
            {
                if (isShort)
                {
                    p = buyShortPen;
                }
                else
                {
                    p = buyLongPen;
                }
                g.DrawLine(p, point.X, point.Y + arrowLengh + offset, point.X, point.Y + offset);
            }
            else
            {
                if (isShort)
                {
                    p = sellShortPen;
                }
                else
                {
                    p = sellLongPen;
                }
                g.DrawLine(p, point.X, point.Y - arrowLengh - offset, point.X, point.Y - offset);
            }
        }
        #endregion

        private PointF[] GetSecondaryScreenPoints(int startIndex, int endIndex)
        {
            List<PointF> points = new List<PointF>();
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (!float.IsNaN(this.secondaryFloatSerie[i]))
                {
                    points.Add(GetSecondaryScreenPointFromValuePoint(new PointF(i, this.secondaryFloatSerie[i])));
                }
            }
            return points.ToArray();
        }
        private PointF GetSecondaryScreenPointFromValuePoint(PointF point2D)
        {
            PointF[] points = new PointF[] { point2D };
            this.matrixSecondaryValueToScreen.TransformPoints(points);
            return points[0];
        }
        private PointF[] GetEventMarqueePointsAtIndex(int index)
        {
            PointF[] marqueePoints = new PointF[3];

            PointF basePoint = this.GetScreenPointFromValuePoint(new PointF(index, 0.0f));
            basePoint.Y = this.GraphRectangle.Top + EVENT_MARQUEE_SIZE * 2;

            marqueePoints[0] = new PointF(basePoint.X - EVENT_MARQUEE_SIZE, this.GraphRectangle.Top);
            marqueePoints[1] = new PointF(basePoint.X + EVENT_MARQUEE_SIZE, this.GraphRectangle.Top);
            marqueePoints[2] = new PointF(basePoint.X, basePoint.Y);

            return marqueePoints;
        }
        private PointF[] GetCommentMarqueePointsAtIndex(int index)
        {
            PointF[] marqueePoints = new PointF[3];

            PointF basePoint = this.GetScreenPointFromValuePoint(new PointF(index, 0.0f));
            basePoint.Y = this.GraphRectangle.Bottom - EVENT_MARQUEE_SIZE * 2;

            marqueePoints[0] = new PointF(basePoint.X - EVENT_MARQUEE_SIZE, this.GraphRectangle.Bottom);
            marqueePoints[1] = new PointF(basePoint.X + EVENT_MARQUEE_SIZE, this.GraphRectangle.Bottom);
            marqueePoints[2] = new PointF(basePoint.X, basePoint.Y);

            return marqueePoints;
        }
        #region MOUSE EVENTS
        override public void MouseMoveOverControl(System.Windows.Forms.MouseEventArgs e, Keys key, bool mouseOverThis)
        {
            if (this.IsInitialized && this.CurveList != null)
            {
                if (mouseDown)
                {
                    DrawSelectionZone(e, key);
                }
                else
                {
                    PointF mousePoint = new PointF(e.X, e.Y);
                    PointF mouseValuePoint;
                    if (this.Magnetism)
                    {
                        mouseValuePoint = FindClosestExtremum(GetValuePointFromScreenPoint(mousePoint));
                    }
                    else
                    {
                        mouseValuePoint = GetValuePointFromScreenPoint(mousePoint);
                    }
                    int index = Math.Max(Math.Min((int)Math.Round(mouseValuePoint.X), this.EndIndex), this.StartIndex);
                    if (this.DrawingMode == GraphDrawMode.Normal)
                    {
                        if ((key & Keys.Control) != 0)
                        {
                            DrawMouseCross(mouseValuePoint, mouseOverThis);
                            this.RaiseDateChangedEvent(null, this.dateSerie[index], mouseValuePoint.Y, true);
                        }
                        else
                        {
                            RefreshMouseMarquee(index, e.Location, false);
                            this.RaiseDateChangedEvent(null, this.dateSerie[index], 0, false);
                        }
                    }
                    else
                    {
                        RefreshMouseMarquee(index, e.Location, true);
                        ManageMouseMoveDrawing(e, mouseValuePoint);
                    }
                    #region Display Event text box
                    // Display events if required
                    if (mouseOverThis && this.ShowEventMarquee &&
                        (mousePoint.Y >= this.GraphRectangle.Top) &&
                        (mousePoint.Y <= this.GraphRectangle.Top + EVENT_MARQUEE_SIZE * 2))
                    {
                        string eventTypeString = string.Empty;
                        int i = this.RoundToIndex(mousePoint);
                        foreach (IStockIndicator indicator in this.CurveList.Indicators.Where(indic => indic.Events != null))
                        {
                            for (int j = 0; j < indicator.EventCount; j++)
                            {
                                BoolSerie eventSerie = indicator.Events[j];
                                if (indicator.IsEvent[j] && eventSerie != null && indicator.Events.Count() > 0)
                                {
                                    if (eventSerie[i])
                                    {
                                        eventTypeString += indicator.Name + " - " + eventSerie.Name +
                                                           System.Environment.NewLine;
                                    }
                                }
                            }
                        }
                        // Cloud
                        // Trail Stops
                        if (this.CurveList.TrailStop != null && this.CurveList.TrailStop.EventCount > 0)
                        {
                            for (int j = 0; j < CurveList.TrailStop.EventCount; j++)
                            {
                                BoolSerie eventSerie = CurveList.TrailStop.Events[j];
                                if (CurveList.TrailStop.IsEvent[j] && eventSerie != null && CurveList.TrailStop.Events.Count() > 0)
                                {
                                    if (eventSerie[i])
                                    {
                                        eventTypeString += CurveList.TrailStop.Name + " - " + eventSerie.Name +
                                                           System.Environment.NewLine;
                                    }
                                }
                            }
                        }
                        // Paint Bars
                        if (this.CurveList.PaintBar != null && this.CurveList.PaintBar.EventCount > 0)
                        {

                            int j = 0;
                            foreach (BoolSerie eventSerie in this.CurveList.PaintBar.Events.Where(ev => ev != null && ev.Count > 0))
                            {
                                if (this.CurveList.PaintBar.SerieVisibility[j] && this.CurveList.PaintBar.IsEvent != null && this.CurveList.PaintBar.IsEvent[j] && eventSerie[i])
                                {
                                    eventTypeString += this.CurveList.PaintBar.Name + " - " + eventSerie.Name + System.Environment.NewLine;
                                }
                                j++;
                            }
                        }

                        if ((this.GraphRectangle.Right - mousePoint.X) < 100.0f)
                        {
                            mousePoint.X -= 100.0f;
                        }
                        this.DrawString(this.foregroundGraphic, eventTypeString, axisFont, Brushes.Black, backgroundBrush, mousePoint.X, mousePoint.Y, true);
                    }
                    #endregion
                    #region Display comment text box
                    if (mouseOverThis && this.ShowCommentMarquee && this.Comments != null &&
                        (mousePoint.Y <= this.GraphRectangle.Bottom) &&
                        (mousePoint.Y >= this.GraphRectangle.Bottom - EVENT_MARQUEE_SIZE * 2))
                    {

                        int i = this.RoundToIndex(mousePoint);
                        DateTime commentDate = this.dateSerie[i];
                        if (this.Comments.ContainsKey(commentDate))
                        {
                            string comment = this.Comments[commentDate];
                            if (!string.IsNullOrWhiteSpace(comment))
                            {
                                Size size = TextRenderer.MeasureText(comment, axisFont);

                                this.DrawString(this.foregroundGraphic, comment, axisFont, Brushes.Black, backgroundBrush, Math.Max(mousePoint.X - size.Width, this.GraphRectangle.Left + 5), mousePoint.Y - size.Height, true);
                            }
                        }
                    }
                    #endregion
                    #region Display Agenda Text
                    if (mouseOverThis && this.ShowAgenda != AgendaEntryType.No && this.Agenda != null &&
                         (mousePoint.Y <= this.GraphRectangle.Bottom) &&
                         (mousePoint.Y >= this.GraphRectangle.Bottom - EVENT_MARQUEE_SIZE * 2))
                    {
                        int i = this.RoundToIndex(mousePoint);
                        DateTime agendaDate = this.dateSerie[i];
                        if (this.Agenda.ContainsKey(agendaDate))
                        {
                            var agendaEntry = this.Agenda[agendaDate];
                            if (agendaEntry.IsOfType(this.ShowAgenda))
                            {
                                string eventText = agendaEntry.Event.Replace("\n", " ");

                                if (agendaEntry.IsOfType(AgendaEntryType.Dividend))
                                {
                                    var coupon = eventText.Substring(eventText.IndexOf(':') + 2);
                                    coupon = coupon.Substring(0, coupon.IndexOf('€'));
                                    float yield = float.Parse(coupon) / closeCurveType.DataSerie[i];
                                    eventText += Environment.NewLine + "Rendement: " + yield.ToString("P2");
                                }

                                Size size = TextRenderer.MeasureText(eventText, axisFont);
                                this.DrawString(this.foregroundGraphic, eventText, axisFont, Brushes.Black, backgroundBrush, Math.Max(mousePoint.X - size.Width, this.GraphRectangle.Left + 5), mousePoint.Y - size.Height, true);
                            }
                        }
                    }
                    #endregion
                }
                this.PaintForeground();
            }
        }
        override public void GraphControl_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!this.Focused) this.Focus();

            if (!this.IsInitialized || this.curveList == null)
            {
                return;
            }

            // Notify listeners
            if (this.lastMouseIndex != -1 && this.PointPick != null)
            {
                PointPick(lastMouseIndex, this.dateSerie[lastMouseIndex]);
            }

            PointF mousePoint = new PointF(e.X, e.Y);

            // Allow clicking in the margin for usability purpose
            if (((e.X - this.GraphRectangle.Right) > 0.0f) && ((e.X - this.GraphRectangle.Right) < 20.0f))
            {
                mousePoint.X = this.GraphRectangle.Right - 1;
            }

            Rectangle2D rect2D = new Rectangle2D(this.GraphRectangle);
            if (rect2D.Contains(mousePoint))
            {
                PointF mouseValuePoint;
                if (this.Magnetism)
                {
                    mouseValuePoint = FindClosestExtremum(GetValuePointFromScreenPoint(mousePoint));
                }
                else
                {
                    mouseValuePoint = GetValuePointFromScreenPoint(mousePoint);
                }
                if (this.DrawingMode == GraphDrawMode.Normal)
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        // Create horizontal line is CTRL key is pressed
                        if ((Control.ModifierKeys & Keys.Control) != 0)
                        {
                            if ((Control.ModifierKeys & Keys.Shift) != 0)
                            {
                                // Draw vertical line
                                mouseValuePoint = GetValuePointFromScreenPoint(mousePoint);
                                Line2D newLine = (Line2D)new Line2D(mouseValuePoint, 0.0f, 1.0f, this.DrawingPen);
                                drawingItems.Add(newLine);
                                AddToUndoBuffer(GraphActionType.AddItem, newLine);
                                this.DrawingStep = GraphDrawingStep.SelectItem;
                                this.BackgroundDirty = true; // The new line becomes a part of the background
                                selectedLineIndex = -1;
                            }
                            else
                            {
                                // Draw horizontal line
                                Line2D newLine = (Line2D)new Line2D(mouseValuePoint, 1.0f, 0.0f, this.DrawingPen);
                                drawingItems.Add(newLine);
                                AddToUndoBuffer(GraphActionType.AddItem, newLine);
                                this.DrawingStep = GraphDrawingStep.SelectItem;
                                this.BackgroundDirty = true; // The new line becomes a part of the background
                                selectedLineIndex = -1;
                            }
                        }
                        else // Show daily values box
                        {
                            if (!forceNoValueBoxDisplay)
                            {
                                this.PaintDailyBox(mousePoint);
                            }
                        }
                    }
                    else
                    {
                        this.contextMenu.Show(this, e.Location);
                    }
                }
                else
                {
                    MouseClickDrawing(e, ref mousePoint, ref mouseValuePoint);
                }
                this.PaintForeground();
            }
        }
        protected void ManageMouseMoveDrawing(System.Windows.Forms.MouseEventArgs e, PointF mouseValuePoint)
        {
            switch (this.DrawingMode)
            {
                case GraphDrawMode.AddSAR:
                    DrawTmpSR(this.foregroundGraphic, mouseValuePoint);
                    //if (this.DrawingStep == GraphDrawingStep.SelectItem)
                    //{
                    //    // first point is already selected, draw new line
                    //    if (!selectedValuePoint.Equals(mouseValuePoint))
                    //    {
                    //        DrawTmpSAR(this.foregroundGraphic, mouseValuePoint);
                    //    }
                    //}
                    break;
                case GraphDrawMode.AddLine:
                    if (this.DrawingStep == GraphDrawingStep.ItemSelected)
                    {
                        // first point is already selected, draw new line
                        if (!selectedValuePoint.Equals(mouseValuePoint))
                        {
                            DrawTmpItem(this.foregroundGraphic, this.DrawingPen, new Line2D(selectedValuePoint, mouseValuePoint), true);
                        }
                    }
                    break;
                case GraphDrawMode.AddSegment:
                    if (this.DrawingStep == GraphDrawingStep.ItemSelected)
                    {
                        // first point is already selected, draw new line
                        if (!selectedValuePoint.Equals(mouseValuePoint))
                        {
                            DrawTmpSegment(this.foregroundGraphic, this.DrawingPen, selectedValuePoint, mouseValuePoint, true);
                        }
                    }
                    break;
                case GraphDrawMode.AddHalfLine:
                    if (this.DrawingStep == GraphDrawingStep.ItemSelected)
                    {
                        // first point is already selected, draw new line
                        if (!selectedValuePoint.Equals(mouseValuePoint))
                        {
                            DrawTmpHalfLine(this.foregroundGraphic, this.DrawingPen, selectedValuePoint, mouseValuePoint, true);
                        }
                    }
                    break;
                case GraphDrawMode.FanLine:
                    if (this.DrawingStep == GraphDrawingStep.ItemSelected)
                    {
                        // first point is already selected, draw new line
                        if (!selectedValuePoint.Equals(mouseValuePoint))
                        {
                            DrawTmpItem(this.foregroundGraphic, this.DrawingPen, new Line2D(selectedValuePoint, mouseValuePoint), true);
                        }
                    }
                    break;
                case GraphDrawMode.AndrewPitchFork:
                    switch (this.DrawingStep)
                    {
                        case GraphDrawingStep.SelectItem: // Selecting the second point
                            if (andrewPitchFork != null && !andrewPitchFork.Point1.Equals(mouseValuePoint))
                            {
                                DrawTmpSegment(this.foregroundGraphic, this.DrawingPen, andrewPitchFork.Point1, mouseValuePoint, true);
                            }
                            break;
                        case GraphDrawingStep.ItemSelected: // Selecting third point

                            if (mouseValuePoint.Equals(andrewPitchFork.Point1) || mouseValuePoint.Equals(andrewPitchFork.Point2))
                            {
                                break;
                            }
                            andrewPitchFork.Point3 = mouseValuePoint;
                            try
                            {
                                foreach (Line2DBase newLine in andrewPitchFork.GetLines(this.DrawingPen))
                                {
                                    DrawTmpItem(this.foregroundGraphic, this.DrawingPen, newLine, true);
                                }
                            }
                            catch (System.ArithmeticException)
                            {
                            }
                            break;
                        default:   // Shouldn't come there
                            break;
                    }
                    break;
                case GraphDrawMode.XABCD:
                    if (XABCD != null)
                    {
                        if (XABCD.NbPoint == 1)
                        {
                            if (!XABCD.X.Equals(mouseValuePoint))
                            {
                                DrawTmpSegment(this.foregroundGraphic, this.DrawingPen, XABCD.X, mouseValuePoint, true);
                            }
                        }
                        else
                        {
                            foreach (var newLine in XABCD.GetLines(this.DrawingPen))
                            {
                                DrawTmpItem(this.foregroundGraphic, this.DrawingPen, newLine, true);
                            }
                            var lastPoint = XABCD.GetLastPoint();
                            if (!lastPoint.Equals(mouseValuePoint))
                            {
                                DrawTmpSegment(this.foregroundGraphic, this.DrawingPen, lastPoint, mouseValuePoint, true);
                            }
                        }
                    }
                    break;
                case GraphDrawMode.CopyLine:
                    switch (this.DrawingStep)
                    {
                        case GraphDrawingStep.SelectItem: // Select the line to copy
                            HighlightClosestLine(e);
                            break;
                        case GraphDrawingStep.ItemSelected: // Create new // line
                            if (selectedLineIndex != -1)
                            {
                                Line2D paraLine = ((Line2DBase)this.drawingItems[selectedLineIndex]).GetParallelLine(mouseValuePoint);
                                this.DrawTmpItem(this.foregroundGraphic, this.DrawingPen, paraLine, true);
                            }
                            break;
                        default:   // Shouldn't come there
                            break;
                    }
                    break;
                case GraphDrawMode.CutLine:
                    if (this.DrawingStep == GraphDrawingStep.SelectItem)
                    {
                        HighlightCutLine(e, mouseValuePoint, (Control.ModifierKeys & Keys.Control) == 0);
                    }
                    break;
                case GraphDrawMode.DeleteItem:
                    if (this.DrawingStep == GraphDrawingStep.SelectItem)
                    {
                        HighlightClosestLine(e);
                    }
                    break;
                default:
                    break;
            }
        }
        private void DrawTmpConvexHull(Graphics graphics, PointF mouseValuePoint)
        {
            if (this.IsInitialized && StockAnalyzerForm.MainFrame.CurrentStockSerie != null)
            {
                int index = Math.Max(Math.Min((int)Math.Round(mouseValuePoint.X), this.EndIndex), this.StartIndex);

                if (index < 200) return;
                HalfLine2D support, resistance;

                StockPaintBar_CONVEXCULL.GetConvexCull(false, StockAnalyzerForm.MainFrame.CurrentStockSerie, index - 200, index, out resistance, out support, Pens.DarkRed, Pens.DarkGreen);

                foreach (var item in StockAnalyzerForm.MainFrame.CurrentStockSerie.StockAnalysis.DrawingItems[StockAnalyzerForm.MainFrame.CurrentStockSerie.BarDuration])
                {
                    DrawTmpItem(graphics, item.Pen, item, true);
                }
            }
        }
        private void DrawTmpSR(Graphics graphics, PointF mouseValuePoint)
        {
            if (this.IsInitialized && StockAnalyzerForm.MainFrame.CurrentStockSerie != null)
            {
                int index = Math.Max(Math.Min((int)Math.Round(mouseValuePoint.X), this.EndIndex), this.StartIndex);

                if (index < 200) return;

                StockPaintBar_CONVEXCULL.GetSR(StockAnalyzerForm.MainFrame.CurrentStockSerie, index - 200, index);

                foreach (var item in StockAnalyzerForm.MainFrame.CurrentStockSerie.StockAnalysis.DrawingItems[StockAnalyzerForm.MainFrame.CurrentStockSerie.BarDuration])
                {
                    DrawTmpItem(graphics, item.Pen, item, true);
                }
            }
        }

        private void DrawTmpSAR(Graphics graphics, PointF mouseValuePoint)
        {
            if (this.IsInitialized && StockAnalyzerForm.MainFrame.CurrentStockSerie != null)
            {
                int index = Math.Max(Math.Min((int)Math.Round(mouseValuePoint.X), this.EndIndex), this.StartIndex);
                FloatSerie sarSerie = null;
                int sarStartIndex;
                bool isSupport = this.closeCurveType.DataSerie[index] > mouseValuePoint.Y;
                StockAnalyzerForm.MainFrame.CurrentStockSerie.CalculateSARStop(index, out sarStartIndex, out sarSerie, isSupport, 0.01f, 0.01f, 0.1f, 3);

                Color color = Color.Red;
                if (!isSupport)
                {
                    color = Color.Green;
                }
                using (Brush srBrush = new SolidBrush(color))
                {
                    PointF srPoint;
                    for (int i = 0; i < sarSerie.Count; i++)
                    {
                        srPoint = GetScreenPointFromValuePoint(sarStartIndex + i, sarSerie[i]);
                        graphics.FillEllipse(srBrush, srPoint.X - 2, srPoint.Y - 2, 2 * 2, 2 * 2);
                    }
                }
            }
        }
        private void MouseClickDrawing(System.Windows.Forms.MouseEventArgs e, ref PointF mousePoint, ref PointF mouseValuePoint)
        {
            switch (this.DrawingMode)
            {
                case GraphDrawMode.AddLine:
                    switch (this.DrawingStep)
                    {
                        case GraphDrawingStep.SelectItem: // Selecting the first point
                            selectedValuePoint = mouseValuePoint;
                            this.DrawingStep = GraphDrawingStep.ItemSelected;
                            break;
                        case GraphDrawingStep.ItemSelected: // Selecting second point
                            PointF point1 = selectedValuePoint;
                            PointF point2 = mouseValuePoint;
                            try
                            {
                                Line2D newLine = new Line2D(point1, point2, this.DrawingPen);
                                drawingItems.Add(newLine);
                                AddToUndoBuffer(GraphActionType.AddItem, newLine);
                                this.DrawingStep = GraphDrawingStep.SelectItem;
                                this.BackgroundDirty = true; // The new line becomes a part of the background
                                selectedLineIndex = -1;
                            }
                            catch (System.ArithmeticException)
                            {
                            }
                            break;
                        default:   // Shouldn't come there
                            break;
                    }
                    break;
                case GraphDrawMode.AddSegment:
                    switch (this.DrawingStep)
                    {
                        case GraphDrawingStep.SelectItem: // Selecting the first point
                            selectedValuePoint = mouseValuePoint;
                            this.DrawingStep = GraphDrawingStep.ItemSelected;
                            break;
                        case GraphDrawingStep.ItemSelected: // Selecting second point
                            PointF point1 = selectedValuePoint;
                            PointF point2 = mouseValuePoint;
                            try
                            {
                                Segment2D newSegment = new Segment2D(point1, point2, this.DrawingPen);
                                drawingItems.Add(newSegment);
                                AddToUndoBuffer(GraphActionType.AddItem, newSegment);
                                this.DrawingStep = GraphDrawingStep.SelectItem;
                                this.BackgroundDirty = true; // The new line becomes a part of the background
                                selectedLineIndex = -1;
                            }
                            catch (System.ArithmeticException)
                            {
                            }
                            break;
                        default:   // Shouldn't come there
                            break;
                    }
                    break;
                case GraphDrawMode.AddHalfLine:
                    switch (this.DrawingStep)
                    {
                        case GraphDrawingStep.SelectItem: // Selecting the first point
                            selectedValuePoint = mouseValuePoint;
                            this.DrawingStep = GraphDrawingStep.ItemSelected;
                            break;
                        case GraphDrawingStep.ItemSelected: // Selecting second point
                            PointF point1 = selectedValuePoint;
                            PointF point2 = mouseValuePoint;
                            try
                            {
                                HalfLine2D newhalfLine = new HalfLine2D(point1, point2, this.DrawingPen);
                                drawingItems.Add(newhalfLine);
                                AddToUndoBuffer(GraphActionType.AddItem, newhalfLine);
                                this.DrawingStep = GraphDrawingStep.SelectItem;
                                this.BackgroundDirty = true; // The new line becomes a part of the background
                                selectedLineIndex = -1;
                            }
                            catch (System.ArithmeticException)
                            {
                            }
                            break;
                        default:   // Shouldn't come there
                            break;
                    }
                    break;
                case GraphDrawMode.FanLine:
                    switch (this.DrawingStep)
                    {
                        case GraphDrawingStep.SelectItem: // Selecting the first point
                            selectedValuePoint = mouseValuePoint;
                            this.DrawingStep = GraphDrawingStep.ItemSelected;
                            break;
                        case GraphDrawingStep.ItemSelected: // Selecting second point
                            PointF point1 = selectedValuePoint;
                            PointF point2 = mouseValuePoint;
                            try
                            {
                                Line2D newLine = (Line2D)new Line2D(point1, point2, this.DrawingPen);
                                drawingItems.Add(newLine);
                                AddToUndoBuffer(GraphActionType.AddItem, newLine);
                                this.BackgroundDirty = true; // The new line becomes a part of the background
                                selectedLineIndex = -1;
                            }
                            catch (System.ArithmeticException)
                            {
                            }
                            break;
                        default:   // Shouldn't come there
                            break;
                    }
                    break;
                case GraphDrawMode.AndrewPitchFork:
                    switch (this.DrawingStep)
                    {
                        case GraphDrawingStep.SelectItem: // Selecting the first point
                            if (andrewPitchFork == null)
                            {
                                andrewPitchFork = new AndrewPitchFork(mouseValuePoint, PointF.Empty, PointF.Empty);
                            }
                            else
                            {
                                // Selecting second point
                                andrewPitchFork.Point2 = mouseValuePoint;
                                this.DrawingStep = GraphDrawingStep.ItemSelected;
                            }
                            break;
                        case GraphDrawingStep.ItemSelected: // Selecting third point
                            andrewPitchFork.Point3 = mouseValuePoint;
                            try
                            {
                                foreach (Line2DBase newLine in andrewPitchFork.GetLines(this.DrawingPen))
                                {
                                    drawingItems.Add(newLine);
                                    AddToUndoBuffer(GraphActionType.AddItem, newLine);
                                }
                                this.BackgroundDirty = true; // The new line becomes a part of the background
                                selectedLineIndex = -1;
                                this.DrawingStep = GraphDrawingStep.SelectItem;
                                andrewPitchFork = null;
                            }
                            catch (System.ArithmeticException)
                            {
                            }
                            break;
                        default:   // Shouldn't come there
                            break;
                    }
                    break;
                case GraphDrawMode.XABCD:
                    if (XABCD == null)
                    {
                        XABCD = new XABCD();
                        XABCD.AddPoint(mouseValuePoint);
                    }
                    else
                    {
                        if (XABCD.NbPoint < 4)
                        {
                            XABCD.AddPoint(mouseValuePoint);
                        }
                        else
                        {
                            try
                            {
                                XABCD.AddPoint(mouseValuePoint);
                                foreach (var newLine in XABCD.GetLines(this.DrawingPen))
                                {
                                    drawingItems.Add(newLine);
                                    AddToUndoBuffer(GraphActionType.AddItem, newLine);
                                }
                                this.BackgroundDirty = true; // The new line becomes a part of the background
                                selectedLineIndex = -1;
                                XABCD = null;
                            }
                            catch (System.ArithmeticException)
                            {
                            }
                        }
                    }
                    break;
                case GraphDrawMode.CopyLine:
                    switch (this.DrawingStep)
                    {
                        case GraphDrawingStep.SelectItem: // Select the line to copy
                            selectedLineIndex = FindClosestLine(mousePoint);
                            if (selectedLineIndex != -1)
                            {
                                this.DrawingStep = GraphDrawingStep.ItemSelected;
                            }
                            break;
                        case GraphDrawingStep.ItemSelected: // Create new // line
                            if (selectedLineIndex != -1)
                            {
                                Line2D newLine = ((Line2DBase)this.drawingItems[selectedLineIndex]).GetParallelLine(mouseValuePoint);
                                newLine.Pen = this.DrawingPen;
                                drawingItems.Add(newLine);
                                AddToUndoBuffer(GraphActionType.AddItem, newLine);
                                this.DrawingStep = GraphDrawingStep.SelectItem;
                                selectedLineIndex = -1;
                                this.BackgroundDirty = true; // The new line becomes a part of the background
                                HighlightClosestLine(e);
                            }
                            break;
                        default:   // Shouldn't come there
                            break;
                    }
                    break;
                case GraphDrawMode.DeleteItem:
                    switch (this.DrawingStep)
                    {
                        case GraphDrawingStep.SelectItem: // Delete element
                            selectedLineIndex = FindClosestLine(mousePoint);
                            if (selectedLineIndex != -1)
                            {
                                DrawingItem removeLine = this.drawingItems.ElementAt(selectedLineIndex);
                                this.drawingItems.RemoveAt(selectedLineIndex);
                                AddToUndoBuffer(GraphActionType.DeleteItem, removeLine);
                                this.BackgroundDirty = true; // New to redraw the background
                                HighlightClosestLine(e);
                            }
                            break;
                        default:   // Shouldn't come there
                            break;
                    }
                    break;
                case GraphDrawMode.CutLine:
                    if (this.DrawingStep == GraphDrawingStep.SelectItem)
                    {
                        selectedLineIndex = FindClosestLine(mousePoint);
                        if (selectedLineIndex != -1) // #### Check if visible to avoid cutting out of scope items
                        {
                            Line2DBase itemToCut = (Line2DBase)this.drawingItems.ElementAt(selectedLineIndex);
                            Line2DBase cutItem = itemToCut.Cut(mouseValuePoint.X, (Control.ModifierKeys & Keys.Control) == 0);
                            if (cutItem != null)
                            {
                                this.drawingItems.RemoveAt(selectedLineIndex);
                                this.drawingItems.Add(cutItem);
                                AddToUndoBuffer(GraphActionType.CutItem, itemToCut, cutItem);
                                this.BackgroundDirty = true; // New to redraw the background
                                HighlightCutLine(e, mouseValuePoint, (Control.ModifierKeys & Keys.Control) == 0);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        private void RefreshMouseMarquee(int mouseIndex, Point mousePos, bool bulletOnly)
        {
            // Display under mouse info
            DrawMousePos(mouseIndex, mousePos.Y);

            // Save last index
            lastMouseIndex = mouseIndex;
            if (bulletOnly) return;

            // Draw text value for selected value
            float previousClose = closeCurveType.DataSerie[Math.Max(0, mouseIndex - 1)];
            float currentClose = closeCurveType.DataSerie[mouseIndex];
            if (this.ShowVariation)
            {
                string variation = (previousClose == 0 ? 0 : (currentClose - previousClose) / previousClose).ToString("P2").Trim();
                PointF point2 = GetScreenPointFromValuePoint(new PointF(mouseIndex, currentClose));
                if (currentClose >= previousClose)
                {
                    this.DrawString(this.foregroundGraphic, "+" + variation, axisFont, Brushes.Green, backgroundBrush, new PointF(point2.X + 10, point2.Y - 20), true);
                }
                else
                {
                    this.DrawString(this.foregroundGraphic, variation, axisFont, Brushes.Red, backgroundBrush, new PointF(point2.X + 10, point2.Y - 20), true);
                }
            }
        }
        protected override void DrawMousePos(int indexInValues, int y)
        {
            if (indexInValues != -1)
            {
                float value;
                PointF screenPoint;
                if (this.Magnetism)
                {
                    float low = lowCurveType.DataSerie[indexInValues];
                    float high = highCurveType.DataSerie[indexInValues];
                    PointF midPoint = GetScreenPointFromValuePoint(indexInValues, (low + high) / 2.0f);
                    if (y < midPoint.Y)
                    {
                        value = high;
                        screenPoint = GetScreenPointFromValuePoint(indexInValues, high);
                        this.foregroundGraphic.DrawEllipse(greenPen, screenPoint.X - MOUSE_MARQUEE_SIZE, screenPoint.Y - MOUSE_MARQUEE_SIZE, MOUSE_MARQUEE_SIZE * 2, MOUSE_MARQUEE_SIZE * 2);
                    }
                    else
                    {
                        value = low;
                        screenPoint = GetScreenPointFromValuePoint(indexInValues, low);
                        this.foregroundGraphic.DrawEllipse(redPen, screenPoint.X - MOUSE_MARQUEE_SIZE, screenPoint.Y - MOUSE_MARQUEE_SIZE, MOUSE_MARQUEE_SIZE * 2, MOUSE_MARQUEE_SIZE * 2);
                    }
                }
                else
                {
                    value = closeCurveType.DataSerie[indexInValues];
                    screenPoint = GetScreenPointFromValuePoint(new PointF(indexInValues, value));
                    this.foregroundGraphic.DrawEllipse(mousePen, screenPoint.X - MOUSE_MARQUEE_SIZE, screenPoint.Y - MOUSE_MARQUEE_SIZE, MOUSE_MARQUEE_SIZE * 2, MOUSE_MARQUEE_SIZE * 2);
                }
                // Draw date for selected value
                string dateString;
                DateTime currentDate = this.dateSerie[indexInValues];
                if (currentDate.Hour == 0 && currentDate.Minute == 0)
                {
                    dateString = currentDate.ToShortDateString();
                }
                else
                {
                    dateString = currentDate.ToShortTimeString();
                }

                PointF dateLocation = new PointF((int)screenPoint.X - 50, (int)GraphRectangle.Bottom + 2);
                this.DrawString(this.foregroundGraphic, dateString, axisFont, Brushes.Black, this.backgroundBrush, dateLocation, true);
                this.DrawString(this.foregroundGraphic, value.ToString("0.####"), axisFont, textBrush, backgroundBrush, new PointF(GraphRectangle.Right + 2, screenPoint.Y - 8), true);
            }
        }
        #endregion
        #region Geometric Functions
        private PointF FindClosestExtremum(PointF mouseValuePoint)
        {
            int selectedIndex = (int)Math.Round(mouseValuePoint.X);
            PointF returnPoint;
            Segment2D segment1, segment2;
            FloatSerie highSerie = highCurveType.DataSerie;
            FloatSerie lowSerie = lowCurveType.DataSerie;
            segment1 = new Segment2D(mouseValuePoint.X, mouseValuePoint.Y, selectedIndex, highSerie[selectedIndex]);
            segment2 = new Segment2D(mouseValuePoint.X, mouseValuePoint.Y, selectedIndex, lowSerie[selectedIndex]);
            if (segment1.Length() < segment2.Length())
            {
                returnPoint = new PointF(selectedIndex, highSerie[selectedIndex]);
            }
            else
            {
                returnPoint = new PointF(selectedIndex, lowSerie[selectedIndex]);
            }
            return returnPoint;
        }
        private void HighlightClosestLine(System.Windows.Forms.MouseEventArgs e)
        {
            // Find the closest line in the list
            int index = FindClosestLine(new PointF(e.X, e.Y));

            if (index != -1)
            {
                selectedLineIndex = index;
                Line2DBase closestLine = ((Line2DBase)this.drawingItems[index]);
                closestLine.Draw(this.foregroundGraphic, selectedLinePen, this.matrixValueToScreen, new Rectangle2D(this.GraphRectangle), this.IsLogScale);
            }
        }
        private void HighlightCutLine(MouseEventArgs e, PointF valuePoint, bool cutRight)
        {
            // Find the closest line in the list
            int index = FindClosestLine(new PointF(e.X, e.Y));

            if (index != -1)
            {
                selectedLineIndex = index;
                Line2DBase closestLine = ((Line2DBase)this.drawingItems[index]);
                Line2DBase cutLine = closestLine.Cut(valuePoint.X, !cutRight);
                if (cutLine != null)
                {
                    cutLine.Draw(this.foregroundGraphic, selectedLinePen, this.matrixValueToScreen, new Rectangle2D(this.GraphRectangle), this.IsLogScale);
                    PointF cutPoint;
                    if (cutRight ^ cutLine.Point1.X > cutLine.Point2.X)
                    {
                        cutPoint = GetScreenPointFromValuePoint(cutLine.Point1);
                    }
                    else
                    {
                        cutPoint = GetScreenPointFromValuePoint(cutLine.Point2);
                    }
                    this.foregroundGraphic.FillEllipse(Brushes.Red, cutPoint.X - MOUSE_MARQUEE_SIZE, cutPoint.Y - MOUSE_MARQUEE_SIZE, MOUSE_MARQUEE_SIZE * 2, MOUSE_MARQUEE_SIZE * 2);
                }
            }
        }
        // Working on screen coordinates
        private int FindClosestLine(PointF point2D)
        {
            int index = -1, counter = 0;
            float minDistance = float.MaxValue;
            float currentDistance = float.MaxValue;
            Line2DBase line;
            foreach (Line2DBase line2D in this.drawingItems.Where(d => (d is Line2DBase) && d.IsPersistent)) // There is an issue here as it supports only persistent items. Does't work with generated line.
            {
                line = line2D.Transform(this.matrixValueToScreen, this.IsLogScale);
                currentDistance = line.DistanceTo(point2D);
                if (currentDistance < minDistance)
                {
                    index = counter;
                    minDistance = currentDistance;
                }
                counter++;
            }
            return index;
        }
        #endregion

        private PointF GetScreenPointFromOrder(StockOrder stockOrder)
        {
            PointF valuePoint2D = PointF.Empty;

            DateTime orderDate = serieName.StartsWith("INT_") ? stockOrder.ExecutionDate : stockOrder.ExecutionDate.Date;
            valuePoint2D.X = this.IndexOf(orderDate);
            valuePoint2D.Y = stockOrder.UnitCost;
            return this.GetScreenPointFromValuePoint(valuePoint2D);
        }
        private int IndexOfRec(DateTime date, int startIndex, int endIndex)
        {
            if (startIndex < endIndex)
            {
                if (dateSerie[startIndex] == date)
                {
                    return startIndex;
                }
                if (dateSerie[endIndex] == date)
                {
                    return endIndex;
                }
                int midIndex = (startIndex + endIndex) / 2;
                int comp = date.CompareTo(dateSerie[midIndex]);
                if (comp == 0)
                {
                    return midIndex;
                }
                else if (comp < 0)
                {// 
                    return IndexOfRec(date, startIndex + 1, midIndex - 1);
                }
                else
                {
                    return IndexOfRec(date, midIndex + 1, endIndex - 1);
                }
            }
            else
            {
                if (startIndex == endIndex && dateSerie[startIndex] == date)
                {
                    return startIndex;
                }
                return -1;
            }
        }
        #region Order Management


        void buyMenu_Click(object sender, System.EventArgs e)
        {
            if (lastMouseIndex != -1 && this.openCurveType != null && this.dateSerie != null)
            {
                StockOrder newOrder = new StockOrder();
                newOrder.State = StockOrder.OrderStatus.Executed;
                newOrder.StockName = this.serieName;
                newOrder.Type = StockOrder.OrderType.BuyAtLimit;
                newOrder.Value = this.closeCurveType.DataSerie[lastMouseIndex];
                newOrder.Number = 0;
                newOrder.CreationDate = this.dateSerie[lastMouseIndex];
                newOrder.ExecutionDate = this.dateSerie[lastMouseIndex];
                newOrder.ExpiryDate = this.dateSerie[lastMouseIndex].AddMonths(1);

                OrderEditionDlg dlg = new OrderEditionDlg(newOrder);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    StockAnalyzerForm.MainFrame.CurrentPortofolio.OrderList.Add(newOrder);

                    this.ForegroundDirty = true;
                }
            }
        }

        void sellMenu_Click(object sender, System.EventArgs e)
        {
            if (lastMouseIndex != -1 && this.openCurveType != null && this.dateSerie != null)
            {
                StockOrder newOrder = new StockOrder();
                newOrder.State = StockOrder.OrderStatus.Executed;
                newOrder.StockName = this.serieName;
                newOrder.Type = StockOrder.OrderType.SellAtLimit;
                newOrder.Value = this.openCurveType.DataSerie[lastMouseIndex];
                newOrder.Number = 10;
                newOrder.CreationDate = this.dateSerie[lastMouseIndex];
                newOrder.ExecutionDate = this.dateSerie[lastMouseIndex];
                newOrder.ExpiryDate = this.dateSerie[lastMouseIndex].AddMonths(1);

                OrderEditionDlg dlg = new OrderEditionDlg(newOrder);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    StockAnalyzerForm.MainFrame.CurrentPortofolio.OrderList.Add(newOrder);

                    this.ForegroundDirty = true;
                }
            }
        }

        void shortMenu_Click(object sender, System.EventArgs e)
        {
            if (lastMouseIndex != -1 && this.openCurveType != null && this.dateSerie != null)
            {
                StockOrder newOrder = new StockOrder();
                newOrder.State = StockOrder.OrderStatus.Executed;
                newOrder.StockName = this.serieName;
                newOrder.Type = StockOrder.OrderType.SellAtLimit;
                newOrder.IsShortOrder = true;
                newOrder.Value = this.openCurveType.DataSerie[lastMouseIndex];
                newOrder.Number = 10;
                newOrder.CreationDate = this.dateSerie[lastMouseIndex];
                newOrder.ExecutionDate = this.dateSerie[lastMouseIndex];
                newOrder.ExpiryDate = this.dateSerie[lastMouseIndex].AddMonths(1);

                OrderEditionDlg dlg = new OrderEditionDlg(newOrder);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    StockAnalyzerForm.MainFrame.CurrentPortofolio.OrderList.Add(newOrder);

                    this.ForegroundDirty = true;
                }
            }
        }

        void coverMenu_Click(object sender, System.EventArgs e)
        {
            if (lastMouseIndex != -1 && this.openCurveType != null && this.dateSerie != null)
            {
                StockOrder newOrder = new StockOrder();
                newOrder.State = StockOrder.OrderStatus.Executed;
                newOrder.StockName = this.serieName;
                newOrder.Type = StockOrder.OrderType.BuyAtLimit;
                newOrder.IsShortOrder = true;
                newOrder.Value = this.openCurveType.DataSerie[lastMouseIndex];
                newOrder.Number = 10;
                newOrder.CreationDate = this.dateSerie[lastMouseIndex];
                newOrder.ExecutionDate = this.dateSerie[lastMouseIndex];
                newOrder.ExpiryDate = this.dateSerie[lastMouseIndex].AddMonths(1);

                OrderEditionDlg dlg = new OrderEditionDlg(newOrder);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    StockAnalyzerForm.MainFrame.CurrentPortofolio.OrderList.Add(newOrder);

                    this.ForegroundDirty = true;
                }
            }
        }
        void financialMenu_Click(object sender, System.EventArgs e)
        {
            StockAnalyzerForm.MainFrame.ShowFinancials();
        }
        void agendaMenu_Click(object sender, System.EventArgs e)
        {
            StockAnalyzerForm.MainFrame.ShowAgenda();
        }
        void openInABCMenu_Click(object sender, System.EventArgs e)
        {
            StockAnalyzerForm.MainFrame.OpenInABCMenu();
        }
        void openInZBMenu_Click(object sender, System.EventArgs e)
        {
            StockAnalyzerForm.MainFrame.OpenInZBMenu();
        }
        void statMenu_Click(object sender, System.EventArgs e)
        {
            StockAnalyzerForm.MainFrame.statMenu_Click();
        }
        #endregion
    }
}