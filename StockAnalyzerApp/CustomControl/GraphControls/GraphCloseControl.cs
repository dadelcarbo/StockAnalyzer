using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockDecorators;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockPortfolio;
using StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs;
using StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs.TradeManager;
using StockAnalyzerSettings.Properties;
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
            }
        }

        public bool Magnetism { get; set; }
        public bool HideIndicators { get; set; }
        public StockPortfolio Portfolio => StockAnalyzerForm.MainFrame.Portfolio;

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

        protected bool ShowDrawings => Settings.Default.ShowDrawings;
        protected bool ShowVariation => Settings.Default.ShowVariation;
        protected bool ShowOrders => Settings.Default.ShowOrders;
        protected bool ShowPositions => Settings.Default.ShowPositions;
        protected bool ShowEventMarquee => Settings.Default.ShowEventMarquee;
        protected bool ShowCommentMarquee => Settings.Default.ShowCommentMarquee;
        protected bool ShowDividend => Settings.Default.ShowDividend;

        protected AgendaEntryType ShowAgenda => (AgendaEntryType)Enum.Parse(typeof(AgendaEntryType), Settings.Default.ShowAgenda);
        protected bool ShowIndicatorDiv => Settings.Default.ShowIndicatorDiv;
        protected bool ShowIndicatorText => Settings.Default.ShowIndicatorText;


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
        public StockAgenda Agenda { get; set; }
        public StockDividend Dividends { get; set; }

        // Secondary serie management
        protected Matrix matrixSecondaryScreenToValue;
        protected Matrix matrixSecondaryValueToScreen;
        public Pen SecondaryPen { get; set; }
        public bool IsBuying { get; private set; } = false;
        public bool IsSelling { get; private set; } = false;

        #endregion

        public delegate void PointPickEventHandler(int index, DateTime date);
        public event PointPickEventHandler PointPick;

        public delegate void StopChangedEventHandler(float stopValue);
        public event StopChangedEventHandler StopChanged;

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

                    float coefX = (this.GraphRectangle.Width - this.XMargin) / (EndIndex - StartIndex + 1);
                    float coefY = this.GraphRectangle.Height / (maxValue - minValue);

                    matrixSecondaryValueToScreen = new Matrix();
                    matrixSecondaryValueToScreen.Translate(this.GraphRectangle.X - (StartIndex - 0.5f) * coefX, maxValue * coefY + this.GraphRectangle.Y);
                    matrixSecondaryValueToScreen.Scale(coefX, -coefY);

                    matrixSecondaryScreenToValue = (Matrix)matrixValueToScreen.Clone();
                    matrixSecondaryScreenToValue.Invert();
                }
            }
        }
        override protected void SetFrameMargin()
        {
            this.XMargin = WIDTH_MARGIN_SIZE;
            this.YMargin = HEIGHT_MARGIN_SIZE;
        }
        #region PAINT METHODS
        override protected void PaintCopyright(Graphics aGraphic)
        {
            return;
            /*
            using MethodLogger ml = new MethodLogger(this);
            string graphCopyright = "Copyright © " + DateTime.Today.Year + " Dad El Carbo";

            Size size = TextRenderer.MeasureText(graphCopyright, this.axisFont);
            PointF point = new PointF(aGraphic.VisibleClipBounds.Right - size.Width + 10, 5);

            this.DrawString(aGraphic, graphCopyright, this.axisFont, Brushes.Black, this.backgroundBrush, point, false);
            */
        }

        protected override void PaintTmpGraph(Graphics aGraphic)
        {
            using MethodLogger ml = new MethodLogger(this);

            #region Display Portfolio

            // Draw order management area
            var orderArea = new RectangleF(GraphRectangle.Right - ORDER_AREA_WITDH, GraphRectangle.Y, ORDER_AREA_WITDH, GraphRectangle.Height);
            aGraphic.FillRectangle(orderAreaBrush, orderArea);
            #endregion

            #region Draw Grid

            // Draw grid
            var gridWidth = this.ShowGrid ? GraphRectangle.X + GraphRectangle.Width : GraphRectangle.X + 3;
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
                    aGraphic.DrawLine(gridPen, GraphRectangle.X - 3, p1.Y, gridWidth, p1.Y);
                    aGraphic.DrawString(val.ToString("0.##"), axisFont, legendBrush, 0, p1.Y - 8);
                }
                val += step;
            }

            #endregion

            #region Draw vertical lines

            DrawVerticalGridLines(aGraphic, true, this.StartIndex, this.EndIndex);

            #endregion

            aGraphic.DrawString(this.dateSerie[this.EndIndex].ToString("dd/MM"), axisFont, legendBrush,
               GraphRectangle.Right - 3, GraphRectangle.Y + GraphRectangle.Height);
            aGraphic.DrawString(this.dateSerie[this.EndIndex].ToString("yyyy"), axisFont, legendBrush,
               GraphRectangle.Right - 1, GraphRectangle.Y + GraphRectangle.Height + 8);

            #endregion

            #region Draw HLine
            // Paint horizontal lines first
            this.PaintHorizontalLines(aGraphic);
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
                    this.DrawStockText(aGraphic, this.CurveList.Cloud.StockTexts);

                    var bullColor = Color.FromArgb(this.CurveList.Cloud.SeriePens[0].Color.A, this.CurveList.Cloud.SeriePens[0].Color.R, this.CurveList.Cloud.SeriePens[0].Color.G, this.CurveList.Cloud.SeriePens[0].Color.B);
                    var bullBrush = new SolidBrush(bullColor);
                    var bullPen = this.CurveList.Cloud.SeriePens[0];

                    var bearColor = Color.FromArgb(this.CurveList.Cloud.SeriePens[1].Color.A, this.CurveList.Cloud.SeriePens[1].Color.R, this.CurveList.Cloud.SeriePens[1].Color.G, this.CurveList.Cloud.SeriePens[1].Color.B);
                    var bearBrush = new SolidBrush(bearColor);
                    var bearPen = this.CurveList.Cloud.SeriePens[1];

                    var bullPoints = GetScreenPoints(StartIndex, EndIndex, this.CurveList.Cloud.Series[0]);
                    var bearPoints = GetScreenPoints(StartIndex, EndIndex, this.CurveList.Cloud.Series[1]);

                    bool isBull = bullPoints[0].Y < bearPoints[0].Y;
                    var nbPoints = bullPoints.Length;
                    var upPoints = new List<PointF>() { bullPoints[0] };
                    var downPoints = new List<PointF>() { bearPoints[0] };
                    for (int i = 1; i < nbPoints; i++)
                    {
                        if (isBull && bullPoints[i].Y < bearPoints[i].Y) // Bull cloud continuing
                        {
                            upPoints.Add(bullPoints[i]);
                            downPoints.Insert(0, bearPoints[i]);
                        }
                        else if (!isBull && bullPoints[i].Y > bearPoints[i].Y) // Bear cloud continuing
                        {
                            upPoints.Add(bullPoints[i]);
                            downPoints.Insert(0, bearPoints[i]);
                        }
                        else // Cloud reversing, need a draw
                        {
                            if (upPoints.Count > 0)
                            {
                                upPoints.Add(bearPoints[i]);
                                downPoints.Insert(0, bullPoints[i]);
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
                    if (upPoints.Count > 1)
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
                    for (int i = 2; i < this.CurveList.Cloud.SeriesCount; i++)
                    {
                        if (this.CurveList.Cloud.SerieVisibility[i] && this.CurveList.Cloud.Series[i]?.Count > 0)
                        {
                            DrawSeriePoints(aGraphic, this.CurveList.Cloud.Series[i], this.CurveList.Cloud.SeriePens[i]);
                        }
                    }
                }
                #endregion
                #region DISPLAY TRAIL STOPS
                if (this.CurveList.TrailStop?.Series[0] != null && this.CurveList.TrailStop.Series[0].Count > 0)
                {
                    this.DrawStockText(aGraphic, this.CurveList.TrailStop.StockTexts);
                    FloatSerie longStopSerie = this.CurveList.TrailStop.Series[0];
                    FloatSerie shortStopSerie = this.CurveList.TrailStop.Series[1];

                    Pen longPen = this.CurveList.TrailStop.SeriePens[0];
                    Pen shortPen = this.CurveList.TrailStop.SeriePens[1];

                    List<PointF> points = new List<PointF>();

                    // Draw Long trail
                    using (Brush brush = new SolidBrush(Color.FromArgb(92, longPen.Color.R, longPen.Color.G, longPen.Color.B)))
                    {
                        FillArea(aGraphic, longStopSerie, closeCurveType.DataSerie, longPen, brush);
                    }
                    using (Brush brush = new SolidBrush(Color.FromArgb(92, shortPen.Color.R, shortPen.Color.G, shortPen.Color.B)))
                    {
                        FillArea(aGraphic, shortStopSerie, closeCurveType.DataSerie, shortPen, brush);
                    }
                    for (int i = 2; i < this.CurveList.TrailStop.SerieVisibility.Length; i++)
                    {
                        if (this.CurveList.TrailStop.SerieVisibility[i] && this.CurveList.TrailStop.Series[i]?.Count > 0)
                        {
                            DrawSeriePoints(aGraphic, this.CurveList.TrailStop.Series[i], this.CurveList.TrailStop.SeriePens[i]);
                        }
                    }
                }
                #endregion
                #region DISPLAY AUTO DRAWING CURVES
                if (this.CurveList.AutoDrawing != null && this.CurveList.AutoDrawing.Series.Count() > 0 && this.CurveList.AutoDrawing.Series[0].Count > 0)
                {
                    FloatSerie longStopSerie = this.CurveList.AutoDrawing.Series[0];
                    FloatSerie shortStopSerie = this.CurveList.AutoDrawing.Series[1];

                    Pen longPen = this.CurveList.AutoDrawing.SeriePens[0];
                    Pen shortPen = this.CurveList.AutoDrawing.SeriePens[1];

                    using Brush longBrush = new SolidBrush(longPen.Color);
                    using Brush shortBrush = new SolidBrush(shortPen.Color);
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
                            if (!float.IsNaN(srPoint1.Y))
                            {
                                aGraphic.DrawLine(longPen, srPoint1, srPoint2);
                            }
                        }
                        else
                        {
                            srPoint2 = GetScreenPointFromValuePoint(i, shortStopSerie[i]);
                            if (!float.IsNaN(srPoint1.Y))
                            {
                                aGraphic.DrawLine(shortPen, srPoint1, srPoint2);
                            }
                        }
                        srPoint1 = srPoint2;
                    }
                }
                #endregion
                #region DISPLAY INDICATORS

                foreach (var stockIndicator in CurveList.Indicators)
                {
                    this.DrawStockText(aGraphic, stockIndicator.StockTexts);
                    for (int i = 0; i < stockIndicator.SeriesCount; i++)
                    {
                        if (stockIndicator.SerieVisibility[i] && stockIndicator.Series[i].Count > 0)
                        {
                            int indexOfPB = Array.IndexOf(stockIndicator.EventNames, "Pullback");
                            int indexOfEoT = Array.IndexOf(stockIndicator.EventNames, "EndOfTrend");

                            bool isSupport = stockIndicator.SerieNames[i].EndsWith(".S");
                            bool isResistance = stockIndicator.SerieNames[i].EndsWith(".R");
                            if (isSupport || isResistance)
                            {
                                PointF srPoint = PointF.Empty;
                                FloatSerie srSerie = stockIndicator.Series[i];
                                float pointSize = stockIndicator.SeriePens[i].Width;
                                using Brush srBrush = new SolidBrush(stockIndicator.SeriePens[i].Color);
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

                                    if (this.ShowIndicatorText && indexOfPB != -1 && indexOfEoT != -1)
                                    {
                                        const int textOffset = 4;

                                        float yPos = isSupport
                                           ? srPoint.Y + pointSize
                                           : srPoint.Y - 2 * pointSize - 12;

                                        // Draw PB and EndOfTrend text
                                        if (stockIndicator.Events[indexOfPB][index])
                                        {
                                            // Pullback in trend detected
                                            this.DrawString(aGraphic, "PB", axisFont, srBrush, srPoint.X - textOffset, yPos, false);
                                        }
                                        else if (stockIndicator.Events[indexOfEoT][index])
                                        {
                                            // End of trend detected
                                            this.DrawString(aGraphic, "End", axisFont, srBrush, srPoint.X - textOffset,
                                               yPos, false);
                                        }
                                        else
                                        {
                                            if (isSupport && stockIndicator.Events[4][index])
                                            {
                                                this.DrawString(aGraphic, "HL", axisFont, srBrush, srPoint.X - textOffset, yPos, false);

                                            }
                                            if (isResistance && stockIndicator.Events[5][index])
                                            {
                                                this.DrawString(aGraphic, "LH", axisFont, srBrush, srPoint.X - textOffset, yPos, false);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                DrawSeriePoints(aGraphic, stockIndicator.Series[i], stockIndicator.SeriePens[i]);
                            }
                        }
                    }

                    // Draw indicator areas
                    if (stockIndicator.Areas != null)
                    {
                        foreach (var area in stockIndicator.Areas.Where(a => a.Visibility))
                        {
                            FillAreaEx(aGraphic, area.UpLine, area.DownLine, null, area.Brush);
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
                                using Brush brush = new SolidBrush(pen.Color);
                                BoolSerie decoSerie = decorator.Events[i];
                                for (int index = this.StartIndex; index <= this.EndIndex; index++)
                                {
                                    if (decoSerie[index])
                                    {
                                        PointF point = new PointF(index, dataSerie[index]);
                                        PointF point2 = GetScreenPointFromValuePoint(point);
                                        aGraphic.FillEllipse(brush, point2.X - pen.Width * 1.5f, point2.Y - pen.Width * 1.5f, pen.Width * 3f, pen.Width * 3f);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                if (StockPortfolio.Portfolios.Any(p => p.Positions.Any(pos => !pos.IsClosed && pos.StockName == serie.StockName)))
                {
                    var portfolioArea = new RectangleF(GraphRectangle.Right - ORDER_AREA_WITDH, GraphRectangle.Y, ORDER_AREA_WITDH, 10);
                    aGraphic.FillRectangle(PortfolioAreaBrush, portfolioArea);
                }
            }

            #endregion

            #region Display the stock value
            if (this.CurveList.PaintBar != null)
            {
                this.DrawStockText(aGraphic, this.CurveList.PaintBar.StockTexts);
            }

            // Then draw the value
            if (closeCurveType != null && closeCurveType.IsVisible)
            {
                tmpPoints = GetScreenPoints(StartIndex, EndIndex, closeCurveType.DataSerie);

                // Store in member for future use (Display mouse marquee)
                switch (this.ChartMode)
                {
                    case GraphChartMode.Line:
                        aGraphic.DrawLines(closeCurveType.CurvePen, tmpPoints);
                        break;
                    case GraphChartMode.LineCross:
                        aGraphic.DrawLines(closeCurveType.CurvePen, tmpPoints);

                        if (EndIndex - StartIndex < GraphRectangle.Width / 3)
                        {
                            for (int i = 0; i < tmpPoints.Length; i++)
                            {
                                var p = tmpPoints[i];
                                aGraphic.DrawLine(closeCurveType.CurvePen, p.X - 3, p.Y, p.X + 3, p.Y);
                                aGraphic.DrawLine(closeCurveType.CurvePen, p.X, p.Y - 3, p.X, p.Y + 3);
                            }
                        }
                        break;
                    case GraphChartMode.BarChart:
                        {
                            FloatSerie openSerie = openCurveType.DataSerie;
                            FloatSerie highSerie = highCurveType.DataSerie;
                            FloatSerie lowSerie = lowCurveType.DataSerie;

                            tmpOpenPoints = GetScreenPoints(StartIndex, EndIndex, openSerie);
                            tmpHighPoints = GetScreenPoints(StartIndex, EndIndex, highSerie);
                            tmpLowPoints = GetScreenPoints(StartIndex, EndIndex, lowSerie);

                            OHLCBar bar = new OHLCBar
                            {
                                Width = 0.40f * aGraphic.VisibleClipBounds.Width / tmpPoints.Count()
                            };
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

                            CandleStick candleStick = new CandleStick
                            {
                                Width = (int)(0.40f * aGraphic.VisibleClipBounds.Width / tmpPoints.Count()) - 1
                            };
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
                                        using Brush brush = new SolidBrush(color.Value);
                                        candleStick.Draw(aGraphic, new Pen(color.Value), brush);
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
            aGraphic.DrawRectangle(gridPen, GraphRectangle.X, GraphRectangle.Y, GraphRectangle.Width, GraphRectangle.Height);
            aGraphic.DrawLine(gridPen, orderArea.X, orderArea.Y, orderArea.X, orderArea.Bottom);

            // Display values and dates
            var lastValue = closeCurveType.DataSerie[EndIndex];
            var lastValuepoint = GetScreenPointFromValuePoint(EndIndex, lastValue);
            aGraphic.DrawString(lastValue.ToString(), axisFont, legendBrush, GraphRectangle.Right + 1, lastValuepoint.Y - 8);

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
                        foreach (var eventSerie in this.CurveList.PaintBar.Events.Where(ev => ev != null && ev.Count > 0))
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
                    if (!eventFound && this.CurveList.Cloud != null && this.CurveList.Cloud.EventCount > 0)
                    {
                        int j = 0;
                        foreach (var eventSerie in this.CurveList.Cloud.Events.Where(ev => ev != null && ev.Count > 0))
                        {
                            if (this.CurveList.Cloud.IsEvent != null && this.CurveList.Cloud.IsEvent[j] && eventSerie[i])
                            {
                                eventFound = true;
                                break;
                            }
                            j++;
                        }
                    }
                    // AutoDrawing
                    if (!eventFound && this.CurveList.AutoDrawing != null && this.CurveList.AutoDrawing.EventCount > 0)
                    {
                        int j = 0;
                        foreach (var eventSerie in this.CurveList.AutoDrawing.Events.Where(ev => ev != null && ev.Count > 0))
                        {
                            if (this.CurveList.AutoDrawing.IsEvent != null && this.CurveList.AutoDrawing.IsEvent[j] && eventSerie[i])
                            {
                                eventFound = true;
                                break;
                            }
                            j++;
                        }
                    }
                    if (eventFound)
                    {
                        PointF[] marqueePoints = GetEventMarqueePointsAtIndex(i);
                        aGraphic.FillPolygon(Brushes.DarkBlue, marqueePoints);
                    }
                }
            }

            #endregion

            #region Display comment marquee
            if (this.Agenda != null && this.ShowAgenda != AgendaEntryType.No)
            {
                var startDate = this.dateSerie[StartIndex];
                var endDate = this.dateSerie[EndIndex];
                foreach (var agendaEntry in this.Agenda.Entries.Where(a => a.Date >= startDate && a.Date <= endDate))
                {
                    if (agendaEntry.IsOfType(this.ShowAgenda))
                    {
                        int index = this.IndexOf(agendaEntry.Date, this.StartIndex, this.EndIndex);

                        PointF[] marqueePoints = GetCommentMarqueePointsAtIndex(index);
                        aGraphic.FillPolygon(Brushes.DarkCyan, marqueePoints);
                    }
                }
            }
            if (this.ShowDividend && this.Dividends != null && this.Dividends.Entries.Count > 0)
            {
                var startDate = this.dateSerie[StartIndex];
                var endDate = this.dateSerie[EndIndex];
                foreach (var dividendEntry in this.Dividends.Entries.Where(a => a.Date >= startDate && a.Date <= endDate))
                {
                    int index = this.IndexOf(dividendEntry.Date, this.StartIndex, this.EndIndex);

                    PointF[] marqueePoints = GetCommentMarqueePointsAtIndex(index);
                    aGraphic.FillPolygon(Brushes.DarkGreen, marqueePoints);
                }
            }
            #endregion

            #region Draw orders

            if (ShowPositions && this.Portfolio != null)
            {
                PaintPositions(aGraphic);
            }
            if (ShowOrders && this.Portfolio != null)
            {
                PaintExecutedOrders(aGraphic);
            }

            #endregion

            #region Display drawing items

            if (this.ShowDrawings && this.drawingItems != null)
            {
                PaintDrawings(aGraphic, this.drawingItems);
            }
            if (this.CurveList?.AutoDrawing?.DrawingItems != null)
            {
                PaintDrawings(aGraphic, this.CurveList.AutoDrawing.DrawingItems);
            }

            #endregion

        }

        private void DrawSeriePoints(Graphics aGraphic, FloatSerie pointSerie, Pen pen)
        {
            List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();
            int start = -1;
            int end = -1;
            for (int k = StartIndex; k <= EndIndex; k++)
            {
                if (float.IsNaN(pointSerie[k]))
                {
                    if (start != -1)
                    {
                        if (start != end) // Draw only if there are at least two points
                        {
                            tuples.Add(new Tuple<int, int>(start, end));
                        }
                        start = -1;
                        end = -1;
                    }
                }
                else
                {
                    if (start == -1)
                    {
                        start = k;
                    }
                    end = k;
                }
            }
            if (start != end) // Draw only if there are at least two points
            {
                tuples.Add(new Tuple<int, int>(start, end));
            }
            foreach (var tuple in tuples)
            {
                var tmpPoints = GetScreenPoints(tuple.Item1, tuple.Item2, pointSerie);
                if (tmpPoints != null)
                {
                    aGraphic.DrawLines(pen, tmpPoints);
                }
            }
        }

        private void FillArea(Graphics aGraphic, FloatSerie dataSerie1, FloatSerie dataSerie2, Pen pen, Brush brush)
        {
            List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();
            int start = -1;
            int end = -1;
            for (int k = StartIndex; k <= EndIndex; k++)
            {
                if (float.IsNaN(dataSerie1[k]))
                {
                    if (start != -1)
                    {
                        if (start != end) // Draw only if there are at least two points
                        {
                            tuples.Add(new Tuple<int, int>(start, end));
                        }
                        start = -1;
                        end = -1;
                    }
                }
                else
                {
                    if (start == -1)
                    {
                        start = k;
                    }
                    end = k;
                }
            }
            if (start != end) // Draw only if there are at least two points
            {
                tuples.Add(new Tuple<int, int>(start, end));
            }
            foreach (var tuple in tuples)
            {
                var tmpPoints = GetScreenPoints(tuple.Item1, tuple.Item2, dataSerie1);
                if (tmpPoints != null)
                {
                    var closePoints = GetScreenPoints(tuple.Item1, tuple.Item2, dataSerie2);
                    var fillPoints = tmpPoints.Concat(closePoints.Reverse()).ToArray();
                    aGraphic.FillPolygon(brush, fillPoints);
                    if (pen != null)
                    {
                        aGraphic.DrawLines(pen, tmpPoints);
                    }
                }
            }
        }
        private void FillAreaEx(Graphics aGraphic, FloatSerie dataSerie1, FloatSerie dataSerie2, Pen pen, Brush brush)
        {
            List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();
            int start = -1;
            int end = -1;
            for (int k = StartIndex; k <= EndIndex; k++)
            {
                if (float.IsNaN(dataSerie1[k]))
                {
                    if (start != -1)
                    {
                        if (start != end) // Draw only if there are at least two points
                        {
                            tuples.Add(new Tuple<int, int>(start, end));
                        }
                        start = -1;
                        end = -1;
                    }
                }
                else
                {
                    if (start == -1)
                    {
                        start = k;
                    }
                    end = k;
                }
            }
            if (start != end) // Draw only if there are at least two points
            {
                tuples.Add(new Tuple<int, int>(start, end));
            }
            foreach (var tuple in tuples)
            {
                var tmpPoints = GetScreenPointsEx(tuple.Item1, tuple.Item2, dataSerie1);
                if (tmpPoints != null)
                {
                    var closePoints = GetScreenPointsEx(tuple.Item1, tuple.Item2, dataSerie2);
                    var fillPoints = tmpPoints.Concat(closePoints.Reverse()).ToArray();
                    aGraphic.FillPolygon(brush, fillPoints);
                    if (pen != null)
                    {
                        aGraphic.DrawLines(pen, tmpPoints);
                    }
                }
            }
        }

        private void PaintDrawings(Graphics aGraphic, StockDrawingItems di)
        {
            if (di == null) return;
            foreach (DrawingItem item in di)
            {
                if (item.GetType() == typeof(CupHandle2D))
                {
                    var cupHandle = (CupHandle2D)item;
                    DrawTmpCupHandle(aGraphic, DrawingPen, cupHandle, true);
                }
                else
                {
                    item.Draw(aGraphic, this.matrixValueToScreen, new Rectangle2D(this.GraphRectangle), this.IsLogScale);
                    // Display support résistance value
                    if (item.GetType() == typeof(Line2D))
                    {
                        Line2D line = (Line2D)item;
                        if (line.IsHorizontal)
                        {
                            PointF textLocation = GetScreenPointFromValuePoint(new PointF(StartIndex, line.Point1.Y));
                            this.DrawString(aGraphic, line.Point1.Y.ToString("0.##"), axisFont, legendBrush, backgroundBrush,
                               new PointF(1, textLocation.Y - 8), true);
                        }
                    }
                }
            }
        }

        private void DrawStockText(Graphics g, List<StockText> stockTexts)
        {
            if (!this.ShowIndicatorText || stockTexts == null || stockTexts.Count == 0)
                return;
            foreach (var text in stockTexts.Where(t => t.AbovePrice && t.Index > this.StartIndex && t.Index <= this.EndIndex))
            {
                var point = float.IsNaN(text.Price) ? GetScreenPointFromValuePoint(text.Index, this.highCurveType.DataSerie[text.Index]) : GetScreenPointFromValuePoint(text.Index, text.Price);
                this.DrawString(g, text.Text, axisFont, legendBrush, point.X, point.Y - 15, false);
            }
            foreach (var text in stockTexts.Where(t => !t.AbovePrice && t.Index > this.StartIndex && t.Index <= this.EndIndex))
            {
                var point = float.IsNaN(text.Price) ? GetScreenPointFromValuePoint(text.Index, this.lowCurveType.DataSerie[text.Index]) : GetScreenPointFromValuePoint(text.Index, text.Price);
                this.DrawString(g, text.Text, axisFont, legendBrush, point.X, point.Y + 5, false);
            }
        }

        // Input point are in Value Units
        protected override void PaintDailyBox(PointF mousePoint)
        {
            if (this.serie.Count == 0) return;
            if (lastMouseIndex == -1 || lastMouseIndex > this.serie.Count) return;
            string value = string.Empty;
            var mouseDate = this.dateSerie[lastMouseIndex];
            value += BuildTabbedString("DATE", mouseDate.ToString("dd/MM/yy"), 12) + "\r\n";
            if (mouseDate.Hour != 0)
            {
                value += BuildTabbedString("TIME", mouseDate.ToShortTimeString(), 12) + "\r\n";
            }
            float closeValue = float.NaN;
            float var = float.NaN;
            var mouseBar = this.serie[mouseDate];
            foreach (GraphCurveType curveType in this.CurveList)
            {
                if (!float.IsNaN(curveType.DataSerie[this.lastMouseIndex]))
                {
                    if (curveType.DataSerie.Name == "CLOSE")
                    {
                        closeValue = mouseBar.CLOSE;
                        var previousClose = curveType.DataSerie[Math.Max(0, this.lastMouseIndex - 1)];
                        var = (closeValue - previousClose) / previousClose;
                    }
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
            if (!float.IsNaN(var))
            {
                value += BuildTabbedString("VAR", var.ToString("P2"), 12) + "\r\n";
                //value += BuildTabbedString("ADR", mouseBar.ADR.ToString("0.####"), 12) + "\r\n";
                value += BuildTabbedString("NADR", mouseBar.NADR.ToString("P2"), 12) + "\r\n";
                //value += BuildTabbedString("ATR", mouseBar.ATR.ToString("0.####"), 12) + "\r\n";
                value += BuildTabbedString("NATR", mouseBar.NATR.ToString("P2"), 12) + "\r\n" + "\r\n";
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
            // Add Trail Stops
            var reentry = float.NaN;
            var trailName = string.Empty;
            if (CurveList.TrailStop != null)
            {
                for (int i = 0; i < CurveList.TrailStop.SeriesCount; i++)
                {
                    if (CurveList.TrailStop.Series[i] != null && CurveList.TrailStop.Series[i].Count > 0 && !float.IsNaN(CurveList.TrailStop.Series[i][this.lastMouseIndex]))
                    {
                        trailName = CurveList.TrailStop.Series[i].Name;
                        reentry = CurveList.TrailStop.Series[i][this.lastMouseIndex];
                        value += BuildTabbedString(trailName, reentry, 12) + "\r\n";
                        if (!float.IsNaN(reentry))
                        {
                            value += BuildTabbedString(trailName, (Math.Abs(reentry - closeValue) / closeValue).ToString("P2"), 12) + "\r\n";
                        }
                    }
                }
                value += "\r\n";
                foreach (var extra in CurveList.TrailStop.Extras)
                {
                    if (extra != null && extra.Count > 0 && !float.IsNaN(extra[this.lastMouseIndex]))
                    {
                        trailName = extra.Name;
                        reentry = extra[this.lastMouseIndex];
                        if (extra.Name.StartsWith("Bars"))
                        {
                            value += BuildTabbedString(trailName, reentry, 12) + "\r\n\r\n";
                        }
                        else
                        {
                            value += BuildTabbedString(trailName, reentry.ToString("P2"), 12) + "\r\n";
                        }
                    }
                }
            }
            if (CurveList.AutoDrawing != null)
            {
                for (int i = 0; i < CurveList.AutoDrawing.SeriesCount; i++)
                {
                    if (CurveList.AutoDrawing.Series[i] != null && CurveList.AutoDrawing.Series[i].Count > 0 && !float.IsNaN(CurveList.AutoDrawing.Series[i][this.lastMouseIndex]))
                    {
                        trailName = CurveList.AutoDrawing.Series[i].Name;
                        reentry = CurveList.AutoDrawing.Series[i][this.lastMouseIndex];
                        value += BuildTabbedString(trailName, reentry, 12) + "\r\n";
                    }
                }
                if (!float.IsNaN(reentry))
                {
                    value += BuildTabbedString(trailName, (Math.Abs(reentry - closeValue) / closeValue).ToString("P2"), 12) + "\r\n";
                }
            }
            // Add Cloud
            var cloudValue = float.NaN;
            var cloudName = string.Empty;
            if (CurveList.Cloud != null)
            {
                for (int i = 0; i < CurveList.Cloud.SeriesCount; i++)
                {
                    if (CurveList.Cloud.Series[i] != null && CurveList.Cloud.Series[i].Count > 0 && !float.IsNaN(CurveList.Cloud.Series[i][this.lastMouseIndex]))
                    {
                        cloudName = CurveList.Cloud.SerieNames[i];
                        cloudValue = CurveList.Cloud.Series[i][this.lastMouseIndex];
                        value += BuildTabbedString(cloudName, cloudValue, 12) + "\r\n";
                    }
                }
            }
            // Add secondary serie
            if (this.secondaryFloatSerie != null)
            {
                value += BuildTabbedString(this.secondaryFloatSerie.Name, this.secondaryFloatSerie[this.lastMouseIndex], 12) + "\r\n";
            }
            // Calculate Highest in bars.
            if (this.lastMouseIndex > 0)
            {
                var closeSerie = this.serie.GetSerie(StockDataType.CLOSE);
                int highest = closeSerie.GetHighestIn(lastMouseIndex);
                if (highest > 0)
                {
                    value += BuildTabbedString("HighestIn", highest.ToString(), 12) + "\r\n";
                }
                int lowest = closeSerie.GetLowestIn(lastMouseIndex);
                if (lowest > 0)
                {
                    value += BuildTabbedString("LowestIn", lowest.ToString(), 12) + "\r\n";
                }
            }
#if DEBUG
            if (StockAnalyzerForm.MainFrame.CurrentStockSerie != null && StockAnalyzerForm.MainFrame.CurrentStockSerie.IsInitialised && StockAnalyzerForm.MainFrame.CurrentStockSerie.LastIndex == this.lastMouseIndex)
            {
                value += BuildTabbedString("COMPLETE", StockAnalyzerForm.MainFrame.CurrentStockSerie.ValueArray[this.lastMouseIndex].IsComplete.ToString(), 12) + "\r\n";
            }
            value += "\r\n" + BuildTabbedString("Index", this.lastMouseIndex.ToString(), 12);
#endif 
            value.Trim();
            // Draw it now
            Size size = TextRenderer.MeasureText(value, toolTipFont);
            PointF point = new PointF(Math.Min(mousePoint.X + 10, GraphRectangle.Right - size.Width), Math.Min(mousePoint.Y + 10, GraphRectangle.Bottom - size.Height));

            this.DrawString(this.foregroundGraphic, value, toolTipFont, Brushes.Black, this.textBackgroundBrush, point, true);
        }

        protected override void PaintGraphTitle(Graphics gr)
        {
            string graphTitle = this.serie.StockGroup + " - " + this.serie?.StockName;

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
            if (this.CurveList.AutoDrawing != null)
            {
                graphTitle += " " + (this.CurveList.AutoDrawing.Name);
            }
            float right = this.DrawString(gr, graphTitle, this.axisFont, Brushes.Black, this.textBackgroundBrush, new PointF(1, 1), true);
            if (this.SecondaryFloatSerie != null)
            {
                graphTitle = this.SecondaryFloatSerie.Name;
                using Brush textBrush = new SolidBrush(SecondaryPen.Color);
                this.DrawString(gr, graphTitle, this.axisFont, textBrush, this.backgroundBrush, new PointF(right + 16, 1), true);
            }
        }
        private void PaintExecutedOrders(Graphics graphic)
        {
            if (this.Portfolio == null)
            {
                return;
            }
            PointF valuePoint2D = PointF.Empty;
            PointF screenPoint2D = PointF.Empty;
            var operations = this.Portfolio.GetExecutedOrders(this.serie.StockName);
            var startDate = this.dateSerie[this.StartIndex];
            var endDate = this.EndIndex == this.dateSerie.Length - 1 ? DateTime.MaxValue : this.dateSerie[this.EndIndex + 1];
            foreach (var operation in operations.Where(p => p.ActivityTime >= startDate && p.ActivityTime < endDate))
            {
                DateTime orderDate = (StockBarDuration.IsIntraday(this.serie.BarDuration) || this.serie.StockGroup == StockSerie.Groups.TURBO) ? operation.ActivityTime : operation.ActivityTime.Date;
                int index = this.IndexOf(orderDate, this.StartIndex, this.EndIndex);
                valuePoint2D.X = index;
                if (valuePoint2D.X < 0)
                {
                    StockLog.Write("Order date not found: " + operation.ActivityTime);
                    continue;
                }
                valuePoint2D.Y = operation.BuySell == "Sell" ? this.highCurveType.DataSerie[index] : this.lowCurveType.DataSerie[index];
                screenPoint2D = this.GetScreenPointFromValuePoint(valuePoint2D);
                this.DrawArrow(graphic, screenPoint2D, operation.BuySell == "Buy", false);
            }
        }
        private void PaintSaxoOpenedOrders(Graphics graphic)
        {
            if (this.Portfolio == null)
            {
                return;
            }
            PointF valuePoint2D = PointF.Empty;
            var openedOrders = this.Portfolio.SaxoOpenOrders.Where(o => o.StockName == this.serie.StockName);
            foreach (var operation in openedOrders)
            {
                valuePoint2D.Y = operation.Price.Value;
                var screenPoint2D = this.GetScreenPointFromValuePoint(valuePoint2D);

                graphic.DrawLine(stopPen, GraphRectangle.Right - ORDER_AREA_WITDH, screenPoint2D.Y, GraphRectangle.Right, screenPoint2D.Y);


                //this.DrawStop(graphic, stopPen, this.EndIndex, operation.Price.Value, true);
            }
        }

        private void PaintOpenedPosition(Graphics graphic, StockPositionBase position)
        {
            DateTime orderDate = (StockBarDuration.IsIntraday(this.serie.BarDuration) || this.serie.StockGroup == StockSerie.Groups.TURBO) ? position.EntryDate : position.EntryDate.Date;
            int entryIndex = this.IndexOf(orderDate, this.StartIndex, this.EndIndex);
            entryIndex = Math.Max(this.StartIndex, entryIndex);

            this.DrawEntry(graphic, entryPen, entryIndex, position.EntryValue, true);

            if (position.Stop != 0)
            {
                this.DrawStop(graphic, stopPen, entryIndex, position.Stop, true);
            }

            var openedOrders = this.Portfolio.SaxoOpenOrders.Where(o => o.StockName == this.serie.StockName);
            foreach (var operation in openedOrders)
            {
                if (operation.Price.Value != position.Stop)
                {
                    this.DrawStop(graphic, trailStopPen, entryIndex, operation.Price.Value, true);
                }
            }

            ////          this.DrawText(g, $"R={ratio.ToString("0.##")}", font, Brushes.Black, Brushes.White, new PointF(left - 35, points[0].Y + 2), true, Pens.Black);

            var winRatio = new WinRatio(entryIndex, this.EndIndex, position.EntryValue, position.Stop, closeCurveType.DataSerie.Last);
            DrawTmpItem(graphic, winRatio, true);
        }

        static readonly SolidBrush RedBrush = new SolidBrush(Color.FromArgb(50, Color.Red));
        static readonly SolidBrush GreenBrush = new SolidBrush(Color.FromArgb(50, Color.Green));

        private void PaintPositions(Graphics graphic)
        {
            if (this.Portfolio == null)
                return;

            var name = this.serie.StockName.ToUpper();
            var positions = this.Portfolio.Positions.Where(p => p.StockName.ToUpper() == name).Cast<StockPositionBase>().Union(this.Portfolio.ClosedPositions.Where(p => p.StockName.ToUpper() == name)).ToList();


            foreach (var openedOrder in Portfolio.GetActiveOrders(this.serie.StockName).Where(o => o.BuySell == "Buy"))
            {
                if (openedOrder != null)
                {
                    this.DrawOpenedOrder(graphic, entryOrderPen, this.EndIndex, openedOrder.Value, true);
                    if (openedOrder.StopValue != 0)
                        this.DrawOpenedOrder(graphic, stopPen, this.EndIndex - 10, openedOrder.StopValue, true);
                }
            }

            if (positions.Count == 0)
                return;

            PointF valuePoint2D = PointF.Empty;
            PointF screenPoint2D = PointF.Empty;
            var startDate = this.dateSerie[this.StartIndex];
            var endDate = this.EndIndex == this.dateSerie.Length - 1 ? DateTime.MaxValue : this.dateSerie[this.EndIndex + 1];

            foreach (var position in positions.Where(p => p.IsClosed && startDate < p.ExitDate.Value && endDate > p.EntryDate))
            {
                DateTime entryDate = (StockBarDuration.IsIntraday(this.serie.BarDuration) || this.serie.StockGroup == StockSerie.Groups.TURBO) ? position.EntryDate : position.EntryDate.Date;
                int entryIndex = this.IndexOf(entryDate, this.StartIndex, this.EndIndex);
                entryIndex = Math.Max(this.StartIndex, entryIndex);

                if (entryIndex == -1) continue;
                int exitIndex = this.IndexOf(position.ExitDate.Value, this.StartIndex, this.EndIndex);
                if (exitIndex == -1) continue;

                if (position.Stop != 0)
                {
                    var winRatio = new WinRatio(entryIndex, exitIndex, position.EntryValue, position.Stop, position.ExitValue.Value);
                    DrawTmpItem(graphic, winRatio, true);
                }
                else
                {
                    PointF entryPoint = this.GetScreenPointFromValuePoint(entryIndex, position.EntryValue);
                    PointF exitPoint = this.GetScreenPointFromValuePoint(exitIndex, position.ExitValue.Value);
                    if (position.EntryValue < position.ExitValue.Value)
                    {
                        graphic.FillRectangle(GreenBrush, entryPoint.X, exitPoint.Y, exitPoint.X - entryPoint.X, entryPoint.Y - exitPoint.Y);
                        graphic.DrawRectangle(greenPen, entryPoint.X, exitPoint.Y, exitPoint.X - entryPoint.X, entryPoint.Y - exitPoint.Y);
                    }
                    else
                    {
                        graphic.FillRectangle(RedBrush, entryPoint.X, entryPoint.Y, exitPoint.X - entryPoint.X, exitPoint.Y - entryPoint.Y);
                        graphic.DrawRectangle(redPen, entryPoint.X, entryPoint.Y, exitPoint.X - entryPoint.X, exitPoint.Y - entryPoint.Y);
                    }
                }
            }

            if (this.EndIndex == this.dateSerie.Length - 1)
            {
                var position = positions.FirstOrDefault(p => !p.IsClosed);
                if (position != null)
                {
                    PaintOpenedPosition(graphic, position);
                }
            }
        }

        static readonly Pen buyLongPen = new Pen(Color.Green, 5) { StartCap = LineCap.Square, EndCap = LineCap.ArrowAnchor };
        static readonly Pen buyShortPen = new Pen(Color.Red, 5) { StartCap = LineCap.RoundAnchor, EndCap = LineCap.ArrowAnchor };
        static readonly Pen sellLongPen = new Pen(Color.Red, 5) { StartCap = LineCap.Square, EndCap = LineCap.ArrowAnchor };
        static readonly Pen sellShortPen = new Pen(Color.Green, 5) { StartCap = LineCap.RoundAnchor, EndCap = LineCap.ArrowAnchor };

        private void DrawArrow(Graphics g, PointF point, bool isBuy, bool isShort)
        {
            int arrowLengh = 15;
            float offset = 10;
            isBuy = isShort ? !isBuy : isBuy;
            Pen p;
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
        private PointF[] GetStopMarqueePoints(float value)
        {
            PointF[] marqueePoints = new PointF[3];

            PointF basePoint = this.GetScreenPointFromValuePoint(new PointF(0, value));
            basePoint.X = this.GraphRectangle.Right;

            marqueePoints[0] = new PointF(basePoint.X - ORDER_AREA_WITDH + 5, basePoint.Y);
            marqueePoints[1] = new PointF(basePoint.X, basePoint.Y - EVENT_MARQUEE_SIZE);
            marqueePoints[2] = new PointF(basePoint.X, basePoint.Y + EVENT_MARQUEE_SIZE);

            return marqueePoints;
        }
        private PointF[] GetEntryMarqueePoints(float value)
        {
            PointF[] marqueePoints = new PointF[3];

            PointF basePoint = this.GetScreenPointFromValuePoint(new PointF(0, value));
            basePoint.X = this.GraphRectangle.Right - ORDER_AREA_WITDH + 15;

            marqueePoints[0] = new PointF(basePoint.X, basePoint.Y);
            marqueePoints[1] = new PointF(basePoint.X - 15, basePoint.Y - EVENT_MARQUEE_SIZE);
            marqueePoints[2] = new PointF(basePoint.X - 15, basePoint.Y + EVENT_MARQUEE_SIZE);

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
        override public void MouseMoveOverControl(MouseEventArgs e, Keys key, bool mouseOverThis)
        {
            if (!this.IsInitialized || this.CurveList == null || this.CurveList.Count == 0)
                return;
            if (mouseDown)
            {
                DrawSelectionZone(e, key);
                this.PaintForeground();
                return;
            }
            PointF mousePoint = new PointF(e.X, e.Y);
            PointF mouseValuePoint = GetValuePointFromScreenPoint(mousePoint);
            if (this.Magnetism)
                mouseValuePoint = FindClosestExtremum(mouseValuePoint);

            // Check Order area
            if (mouseOverThis && mousePoint.X + ORDER_AREA_WITDH >= this.GraphRectangle.Right)
            {
                if ((key & Keys.Control) != 0)
                {
                    if (IsBuying)
                    {
                        this.RaiseDateChangedEvent(null, this.serie.LastValue.DATE, mouseValuePoint.Y, true);
                    }
                    else
                    {
                        var position = Portfolio.Positions.FirstOrDefault(p => p.StockName == this.serie.StockName);
                        if (position != null)
                        {
                            this.DrawStop(foregroundGraphic, trailStopPen, this.StartIndex, mouseValuePoint.Y, true);
                            this.RaiseDateChangedEvent(null, this.serie.LastValue.DATE, mouseValuePoint.Y, true);
                            this.PaintForeground();
                        }
                        else
                        {
                            var openOrder = Portfolio.GetActiveOrders(this.serie.StockName).FirstOrDefault();
                            if (openOrder != null)
                            {
                                this.DrawStop(foregroundGraphic, entryOrderPen, this.StartIndex, mouseValuePoint.Y, true);
                                this.PaintForeground();
                            }
                        }
                    }
                }
                else
                {
                    if (this.IsBuying && openTradeViewModel != null)
                    {
                        if (openTradeViewModel.StopValue != 0)
                            this.DrawOpenedOrder(this.foregroundGraphic, stopPen, this.EndIndex - 10, openTradeViewModel.StopValue, true);
                        this.DrawOpenedOrder(this.foregroundGraphic, entryOrderPen, this.EndIndex, openTradeViewModel.EntryValue, true);
                    }
                    DrawMouseCross(mousePoint, true, true, this.axisDashPen);
                    this.PaintForeground();
                }
                return;
            }

            if (this.IsBuying && openTradeViewModel != null)
            {
                if (openTradeViewModel.StopValue != 0)
                    this.DrawOpenedOrder(this.foregroundGraphic, stopPen, this.EndIndex - 10, openTradeViewModel.StopValue, true);
                this.DrawOpenedOrder(this.foregroundGraphic, entryOrderPen, this.EndIndex, openTradeViewModel.EntryValue, true);
            }
            int index = Math.Max(Math.Min((int)Math.Round(mouseValuePoint.X), this.EndIndex), this.StartIndex);
            bool drawHorizontalLine = mouseOverThis && mousePoint.Y > GraphRectangle.Top && mousePoint.Y < GraphRectangle.Bottom;
            if ((key & Keys.Control) != 0)
            {
                DrawMouseValueCross(mouseValuePoint, drawHorizontalLine, false, DrawingPen, true);
            }
            else
            {
                DrawMouseValueCross(mouseValuePoint, drawHorizontalLine, true, this.axisDashPen, false);
            }
            if (this.DrawingMode == GraphDrawMode.Normal)
            {
                if ((key & Keys.Control) != 0)
                {
                    this.RaiseDateChangedEvent(null, this.dateSerie[index], mouseValuePoint.Y, !this.IsBuying);
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
                if (this.CurveList.Cloud != null && this.CurveList.Cloud.EventCount > 0)
                {
                    for (int j = 0; j < CurveList.Cloud.EventCount; j++)
                    {
                        BoolSerie eventSerie = CurveList.Cloud.Events[j];
                        if (CurveList.Cloud.IsEvent[j] && eventSerie != null && CurveList.Cloud.Events.Count() > 0)
                        {
                            if (eventSerie[i])
                            {
                                eventTypeString += CurveList.Cloud.Name + " - " + eventSerie.Name + System.Environment.NewLine;
                            }
                        }
                    }
                }
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
                // Paint Bars
                if (this.CurveList.AutoDrawing != null && this.CurveList.AutoDrawing.EventCount > 0)
                {

                    int j = 0;
                    foreach (BoolSerie eventSerie in this.CurveList.AutoDrawing.Events.Where(ev => ev != null && ev.Count > 0))
                    {
                        if (this.CurveList.AutoDrawing.IsEvent != null && this.CurveList.AutoDrawing.IsEvent[j] && eventSerie[i])
                        {
                            eventTypeString += this.CurveList.AutoDrawing.Name + " - " + eventSerie.Name + System.Environment.NewLine;
                        }
                        j++;
                    }
                }

                if ((this.GraphRectangle.Right - mousePoint.X) < 100.0f)
                {
                    mousePoint.X -= 100.0f;
                }
                this.DrawString(this.foregroundGraphic, eventTypeString, axisFont, Brushes.Black, mousePoint.X, mousePoint.Y, true);
            }
            #endregion
            #region Display Agenda Text
            if (mouseOverThis && this.ShowAgenda != AgendaEntryType.No && this.Agenda != null &&
                 (mousePoint.Y <= this.GraphRectangle.Bottom) &&
                 (mousePoint.Y >= this.GraphRectangle.Bottom - (EVENT_MARQUEE_SIZE * 3)))
            {
                int i = this.RoundToIndex(mousePoint);
                DateTime agendaDate1 = this.dateSerie[Math.Max(StartIndex, i - 1)];
                DateTime agendaDate2 = this.dateSerie[Math.Min(EndIndex, i + 1)];
                var agendaEntry = this.Agenda.Entries.FirstOrDefault(a => a.Date >= agendaDate1 && a.Date <= agendaDate2 && a.IsOfType(this.ShowAgenda));
                if (agendaEntry != null)
                {
                    string eventText = agendaEntry.Event.Replace("\n", " ") + Environment.NewLine;
                    eventText += "Date : " + agendaEntry.Date.ToShortDateString();
                    Size size = TextRenderer.MeasureText(eventText, axisFont);
                    this.DrawString(this.foregroundGraphic, eventText, axisFont, Brushes.Black, Math.Max(mousePoint.X - size.Width, this.GraphRectangle.Left + 5), mousePoint.Y - size.Height, true);
                }
            }
            #endregion
            #region Display Dividend Text
            if (mouseOverThis && this.ShowDividend &&
                this.Dividends != null && this.Dividends.Entries.Count > 0 &&
                 (mousePoint.Y <= this.GraphRectangle.Bottom) &&
                 (mousePoint.Y >= this.GraphRectangle.Bottom - (EVENT_MARQUEE_SIZE * 3)))
            {
                int i = this.RoundToIndex(mousePoint);
                DateTime agendaDate1 = this.dateSerie[Math.Max(StartIndex, i - 1)];
                DateTime agendaDate2 = this.dateSerie[Math.Min(EndIndex, i + 1)];
                var dividendEntry = this.Dividends.Entries.FirstOrDefault(a => a.Date >= agendaDate1 && a.Date <= agendaDate2);
                if (dividendEntry != null)
                {
                    var coupon = dividendEntry.Dividend;
                    float yield = coupon / closeCurveType.DataSerie[i];
                    var eventText = "Dividende";
                    eventText += Environment.NewLine + "Date: " + dividendEntry.Date.ToShortDateString();
                    eventText += Environment.NewLine + "Coupon: " + dividendEntry.Dividend.ToString();
                    eventText += Environment.NewLine + "Rendement: " + yield.ToString("P2");

                    Size size = TextRenderer.MeasureText(eventText, axisFont);
                    this.DrawString(this.foregroundGraphic, eventText, axisFont, Brushes.Black, Math.Max(mousePoint.X - size.Width, this.GraphRectangle.Left + 5), mousePoint.Y - size.Height, true);
                }
            }
            #endregion

            this.PaintForeground();
        }
        override public void GraphControl_MouseClick(object sender, MouseEventArgs e)
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
            if (mousePoint.Y - this.GraphRectangle.Y < 10)
            {
                var ptfs = StockPortfolio.Portfolios.Where(p => p.Positions.Any(pos => !pos.IsClosed && pos.StockName == serie.StockName) || p.SaxoOpenOrders.Any(o => o.StockName == serie.StockName)).ToList();
                if (ptfs.Count == 0)
                    return;
                var text = ptfs.Select(p => p.Name).Aggregate((i, j) => i + Environment.NewLine + j);

                Size size = TextRenderer.MeasureText(text, axisFont);
                this.DrawString(this.foregroundGraphic, text, axisFont, Brushes.Black, Math.Max(mousePoint.X - size.Width, this.GraphRectangle.Left + 5), mousePoint.Y, true);

                this.PaintForeground();
                return;
            }

            if (this.ShowPositions && (Control.ModifierKeys & Keys.Control) != 0 && this.Portfolio != null && mousePoint.X + ORDER_AREA_WITDH >= this.GraphRectangle.Right)
            {
                var position = Portfolio.Positions.FirstOrDefault(p => p.StockName == this.serie.StockName);
                if (position != null)
                {
                    if (DialogResult.Yes == MessageBox.Show("Do you want to sent order to Saxo", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        var mouseValuePoint = GetValuePointFromScreenPoint(mousePoint);
                        var trailStopValue = Math.Max(mouseValuePoint.Y, position.Stop);
                        trailStopValue = Math.Min(trailStopValue, serie.LastValue.LOW);

                        var orderId = this.Portfolio.SaxoUpdateStopOrder(position, trailStopValue);
                        this.ForceRefresh();
                        if (StopChanged != null)
                        {
                            this.StopChanged(trailStopValue);
                        }
                    }
                }
                else
                {
                    var openOrder = Portfolio.GetActiveOrders(this.serie.StockName).FirstOrDefault();
                    if (openOrder != null)
                    {
                        if (DialogResult.Yes == MessageBox.Show("Do you want to sent order update to Saxo", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                        {
                            var mouseValuePoint = GetValuePointFromScreenPoint(mousePoint);
                            var value = mouseValuePoint.Y;
                            if (openOrder.OrderType == SaxoOrderType.StopIfTraded.ToString())
                            {
                                value = Math.Max(serie.LastValue.CLOSE, value);
                            }
                            else
                            {
                                value = Math.Min(serie.LastValue.CLOSE, value);
                            }
                            var orderId = this.Portfolio.SaxoUpdateOpenOrder(openOrder, value);
                            this.ForceRefresh();
                        }
                    }
                }
                return;
            }

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
                    if (e.Button == MouseButtons.Left)
                    {
                        // Create horizontal line is CTRL key is pressed
                        if ((Control.ModifierKeys & Keys.Control) != 0)
                        {
                            if ((Control.ModifierKeys & Keys.Shift) != 0)
                            {
                                // Draw vertical line
                                mouseValuePoint = GetValuePointFromScreenPoint(mousePoint);
                                Line2D newLine = (Line2D)new Line2D(mouseValuePoint, 0.0f, 1.0f, DrawingPen);
                                drawingItems.Add(newLine);
                                drawingItems.RefDate = dateSerie[(int)mouseValuePoint.X];
                                drawingItems.RefDateIndex = (int)mouseValuePoint.X;
                                AddToUndoBuffer(GraphActionType.AddItem, newLine);
                                this.DrawingStep = GraphDrawingStep.SelectItem;
                                this.BackgroundDirty = true; // The new line becomes a part of the background
                                selectedLineIndex = -1;
                            }
                            else
                            {
                                // Draw horizontal line
                                Line2D newLine = (Line2D)new Line2D(mouseValuePoint, 1.0f, 0.0f, DrawingPen);
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
                        this.sellMenu.Visible = Portfolio?.Positions.FirstOrDefault(p => p.StockName == this.serie.StockName) != null;
                        this.cancelMenu.Visible = Portfolio.GetActiveOrders(this.serie.StockName).FirstOrDefault() != null;
                        this.buyMenu.Visible = !(this.sellMenu.Visible || this.cancelMenu.Visible);
                        this.openSaxoIntradyConfigDlg.Visible = this.serie?.SaxoId > 0;
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
        protected void ManageMouseMoveDrawing(MouseEventArgs e, PointF mouseValuePoint)
        {
            PointF point1 = selectedValuePoint;
            PointF point2 = mouseValuePoint;

            if ((Control.ModifierKeys & Keys.Shift) != 0)
            {
                point2 = new PointF(mouseValuePoint.X, point1.Y);
            }
            if (point2 == point1)
                return;
            switch (this.DrawingMode)
            {
                case GraphDrawMode.AddLine:
                    if (this.DrawingStep == GraphDrawingStep.ItemSelected)
                    {
                        DrawTmpItem(this.foregroundGraphic, new Line2D(point1, point2), true);
                    }
                    break;
                case GraphDrawMode.AddBox:
                    if (this.DrawingStep == GraphDrawingStep.ItemSelected)
                    {
                        DrawTmpItem(this.foregroundGraphic, new Box(new PointF((int)point1.X, point1.Y), new PointF((int)point2.X, point2.Y)), true);
                    }
                    break;
                case GraphDrawMode.AddWinRatio:
                    if (this.DrawingStep == GraphDrawingStep.ItemSelected)
                    {
                        if (newWinRatio == null)
                        {
                            if (point1.Y < point2.Y)
                            {
                                DrawTmpItem(this.foregroundGraphic, new WinRatio(point1, point2, PointF.Empty), true);
                            }
                            else
                            {
                                DrawTmpItem(this.foregroundGraphic, new WinRatio(point2, point1, PointF.Empty), true);
                            }
                        }
                        else
                        {
                            newWinRatio.Exit = point2;
                            DrawTmpItem(this.foregroundGraphic, newWinRatio, true);
                        }
                    }
                    break;
                case GraphDrawMode.AddSegment:
                    if (this.DrawingStep == GraphDrawingStep.ItemSelected)
                    {
                        DrawTmpSegment(this.foregroundGraphic, DrawingPen, point1, point2, true);
                    }
                    break;
                case GraphDrawMode.AddCupHandle:
                    // Detect Cap and Handle
                    var cupHandle = DetectCupHandle((int)Math.Round(mouseValuePoint.X));
                    if (cupHandle != null)
                    {
                        DrawTmpCupHandle(foregroundGraphic, DrawingPen, cupHandle, true);
                    }
                    break;
                case GraphDrawMode.AddHalfLine:
                    if (this.DrawingStep == GraphDrawingStep.ItemSelected)
                    {
                        DrawTmpHalfLine(this.foregroundGraphic, DrawingPen, point1, point2, true);
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
                                this.DrawTmpItem(this.foregroundGraphic, paraLine, true);
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
        private CupHandle2D DetectCupHandle(int index)
        {
            return closeCurveType.DataSerie.DetectCupHandle(index, 2, false);
        }
        protected void DrawTmpCupHandle(Graphics graph, Pen pen, CupHandle2D cupHandle, bool useTransform)
        {
            // Fill the area in green if bullish
            var start = (int)Math.Min(cupHandle.Point1.X, cupHandle.Point2.X);
            var end = (int)Math.Max(cupHandle.Point1.X, cupHandle.Point2.X);
            if (end < StartIndex || start > EndIndex)
                return;
            start = Math.Max(StartIndex, start);
            end = Math.Min(EndIndex, end);
            //if (false)
            //{
            //    PointF[] polygonPoints = new PointF[end - start + 1];
            //    if (cupHandle.Inverse)
            //    {
            //        // Calculate lower body low
            //        for (int i = start; i < end; i++)
            //        {
            //            polygonPoints[i - start] = GetScreenPointFromValuePoint(i, Math.Min(openCurveType.DataSerie[i], closeCurveType.DataSerie[i]));
            //        }
            //    }
            //    else
            //    {
            //        // Calculate upper body high
            //        for (int i = start; i < end; i++)
            //        {
            //            polygonPoints[i - start] = GetScreenPointFromValuePoint(i, closeCurveType.DataSerie[i]);
            //            //polygonPoints[i - start] = GetScreenPointFromValuePoint(i, Math.Max(openCurveType.DataSerie[i], closeCurveType.DataSerie[i]));
            //        }
            //    }
            //    polygonPoints[0] = GetScreenPointFromValuePoint(start, cupHandle.Point1.Y);
            //    polygonPoints[end - start] = GetScreenPointFromValuePoint(end, cupHandle.Point2.Y);
            //    if (cupHandle.Inverse)
            //    {
            //        graph.FillPolygon(CupHandleInvBrush, polygonPoints);
            //    }
            //    else
            //    {
            //        graph.FillPolygon(CupHandleBrush, polygonPoints);
            //    }
            //}
            // Calculate intersection with bounding rectangle
            Rectangle2D rect2D = new Rectangle2D(GraphRectangle);
            if (useTransform)
            {
                cupHandle.Draw(graph, pen, this.matrixValueToScreen, rect2D, this.IsLogScale);
            }
            else
            {
                cupHandle.Draw(graph, pen, GraphControl.matrixIdentity, rect2D, false);
            }
            if (!ShowIndicatorText)
                return;
            string text;
            // Draw Handle metrics
            var textPos = GetScreenPointFromValuePoint(cupHandle.Pivot.X, cupHandle.Pivot.Y);
            if (cupHandle.Inverse)
            {
                textPos.X -= 15;
                textPos.Y += 9;
                text = ((int)cupHandle.Pivot.X - cupHandle.Point1.X).ToString() + " - " + ((int)cupHandle.Point2.X - cupHandle.Pivot.X).ToString();
            }
            else
            {
                textPos.X -= 15;
                textPos.Y -= 16;
                if (cupHandle.Point1.X == 1)
                {
                    text = "ATH - " + ((int)cupHandle.Point2.X - cupHandle.Pivot.X).ToString();
                }
                else
                {
                    text = ((int)cupHandle.Pivot.X - cupHandle.Point1.X).ToString() + " - " + ((int)cupHandle.Point2.X - cupHandle.Pivot.X).ToString();
                }
            }
            this.DrawString(graph, text, axisFont, legendBrush, this.backgroundBrush, textPos, false);

            // Draw HL and LL
            textPos = GetScreenPointFromValuePoint(cupHandle.LeftLow.X, cupHandle.LeftLow.Y);
            if (cupHandle.Inverse)
            {
                textPos.X -= 5;
                textPos.Y -= 16;
                text = cupHandle.LeftLow.Y < cupHandle.RightLow.Y ? "LH" : "HH";
            }
            else
            {
                textPos.X -= 5;
                textPos.Y += 5;
                text = cupHandle.LeftLow.Y < cupHandle.RightLow.Y ? "LL" : "HL";
            }
            this.DrawString(graph, text, axisFont, legendBrush, this.backgroundBrush, textPos, false);

            textPos = GetScreenPointFromValuePoint(cupHandle.RightLow.X, cupHandle.RightLow.Y);
            if (cupHandle.Inverse)
            {
                textPos.X -= 5;
                textPos.Y -= 16;
                text = cupHandle.LeftLow.Y > cupHandle.RightLow.Y ? "LH" : "HH";
            }
            else
            {
                textPos.X -= 5;
                textPos.Y += 5;
                text = cupHandle.LeftLow.Y > cupHandle.RightLow.Y ? "LL" : "HL";
            }
            this.DrawString(graph, text, axisFont, legendBrush, this.backgroundBrush, textPos, false);
        }

        WinRatio newWinRatio = null;
        private void MouseClickDrawing(MouseEventArgs e, ref PointF mousePoint, ref PointF mouseValuePoint)
        {
            PointF point1 = selectedValuePoint;
            PointF point2 = mouseValuePoint;

            if ((int)selectedValuePoint.X == (int)mouseValuePoint.X)
                return;
            if ((Control.ModifierKeys & Keys.Shift) != 0)
            {
                point2 = new PointF(mouseValuePoint.X, point1.Y);
            }
            StockLog.Write("DrawingMode: " + this.DrawingMode + "DrawingStep: " + this.DrawingStep);
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
                            try
                            {
                                Line2D newLine = new Line2D(point1, point2, DrawingPen);
                                drawingItems.Add(newLine);
                                drawingItems.RefDate = dateSerie[(int)point1.X];
                                drawingItems.RefDateIndex = (int)point1.X;
                                AddToUndoBuffer(GraphActionType.AddItem, newLine);
                                this.DrawingStep = GraphDrawingStep.SelectItem;
                                this.BackgroundDirty = true; // The new line becomes a part of the background
                                selectedLineIndex = -1;
                                selectedValuePoint = PointF.Empty;
                            }
                            catch (ArithmeticException)
                            {
                            }
                            break;
                        default:   // Shouldn't come there
                            break;
                    }
                    break;
                case GraphDrawMode.AddBox:
                    switch (this.DrawingStep)
                    {
                        case GraphDrawingStep.SelectItem: // Selecting the first point
                            selectedValuePoint = mouseValuePoint;
                            this.DrawingStep = GraphDrawingStep.ItemSelected;
                            break;
                        case GraphDrawingStep.ItemSelected: // Selecting second point
                            try
                            {
                                var newArea = new Box(new PointF((int)point1.X, point1.Y), new PointF((int)point2.X, point2.Y));
                                drawingItems.Add(newArea);
                                drawingItems.RefDate = dateSerie[(int)point1.X];
                                drawingItems.RefDateIndex = (int)point1.X;
                                AddToUndoBuffer(GraphActionType.AddItem, newArea);
                                this.DrawingStep = GraphDrawingStep.SelectItem;
                                this.BackgroundDirty = true; // The new line becomes a part of the background
                                selectedLineIndex = -1;
                                selectedValuePoint = PointF.Empty;
                            }
                            catch (ArithmeticException)
                            {
                            }
                            break;
                        default:   // Shouldn't come there
                            break;
                    }
                    break;
                case GraphDrawMode.AddWinRatio:
                    switch (this.DrawingStep)
                    {
                        case GraphDrawingStep.SelectItem: // Selecting the first point
                            selectedValuePoint = mouseValuePoint;
                            this.DrawingStep = GraphDrawingStep.ItemSelected;
                            newWinRatio = null;
                            break;
                        case GraphDrawingStep.ItemSelected: // Selecting next points
                            try
                            {
                                if (newWinRatio == null)
                                {
                                    if (point1.Y < point2.Y)
                                    {
                                        newWinRatio = new WinRatio(point1, point2, PointF.Empty);
                                    }
                                    else
                                    {
                                        newWinRatio = new WinRatio(point2, point1, PointF.Empty);
                                    }
                                    this.BackgroundDirty = true; // The new line becomes a part of the background
                                    selectedLineIndex = -1;
                                    selectedValuePoint = PointF.Empty;
                                }
                                else
                                {
                                    newWinRatio.Exit = point2;
                                    drawingItems.Add(newWinRatio);
                                    drawingItems.RefDate = dateSerie[(int)point1.X];
                                    drawingItems.RefDateIndex = (int)point1.X;
                                    AddToUndoBuffer(GraphActionType.AddItem, newWinRatio);
                                    this.DrawingStep = GraphDrawingStep.SelectItem;
                                    this.BackgroundDirty = true; // The new line becomes a part of the background
                                    selectedLineIndex = -1;
                                    selectedValuePoint = PointF.Empty;
                                    newWinRatio = null;
                                }
                            }
                            catch (ArithmeticException)
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
                            try
                            {
                                Segment2D newSegment = new Segment2D(point1, point2, DrawingPen);
                                drawingItems.Add(newSegment);
                                drawingItems.RefDate = dateSerie[(int)point1.X];
                                drawingItems.RefDateIndex = (int)point1.X;
                                AddToUndoBuffer(GraphActionType.AddItem, newSegment);
                                this.DrawingStep = GraphDrawingStep.SelectItem;
                                this.BackgroundDirty = true; // The new line becomes a part of the background
                                selectedLineIndex = -1;
                                selectedValuePoint = PointF.Empty;
                            }
                            catch (ArithmeticException)
                            {
                            }
                            break;
                        default:   // Shouldn't come there
                            break;
                    }
                    break;
                case GraphDrawMode.AddCupHandle:
                    switch (this.DrawingStep)
                    {
                        case GraphDrawingStep.SelectItem:
                            // Detect Cap and Handle
                            var cupHandle = DetectCupHandle((int)Math.Round(mouseValuePoint.X));
                            if (cupHandle != null)
                            {
                                drawingItems.Add(cupHandle);
                                drawingItems.RefDate = dateSerie[(int)cupHandle.Point1.X];
                                drawingItems.RefDateIndex = (int)cupHandle.Point1.X;
                                AddToUndoBuffer(GraphActionType.AddItem, cupHandle);
                                this.DrawingStep = GraphDrawingStep.SelectItem;
                                this.BackgroundDirty = true; // The new line becomes a part of the background
                                selectedLineIndex = -1;
                                selectedValuePoint = PointF.Empty;
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
                            try
                            {
                                HalfLine2D newhalfLine = new HalfLine2D(point1, point2, DrawingPen);
                                drawingItems.Add(newhalfLine);
                                drawingItems.RefDate = dateSerie[(int)point1.X];
                                drawingItems.RefDateIndex = (int)point1.X;
                                AddToUndoBuffer(GraphActionType.AddItem, newhalfLine);
                                this.DrawingStep = GraphDrawingStep.SelectItem;
                                this.BackgroundDirty = true; // The new line becomes a part of the background
                                selectedLineIndex = -1;
                                selectedValuePoint = PointF.Empty;
                            }
                            catch (ArithmeticException)
                            {
                            }
                            break;
                        default:   // Shouldn't come there
                            break;
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
                                newLine.Pen = DrawingPen;
                                drawingItems.Add(newLine);
                                AddToUndoBuffer(GraphActionType.AddItem, newLine);
                                this.DrawingStep = GraphDrawingStep.SelectItem;
                                selectedLineIndex = -1;
                                selectedValuePoint = PointF.Empty;
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
                                if (removeLine.IsPersistent)
                                {
                                    this.drawingItems.RemoveAt(selectedLineIndex);
                                    AddToUndoBuffer(GraphActionType.DeleteItem, removeLine);
                                    this.BackgroundDirty = true; // New to redraw the background
                                    HighlightClosestLine(e);
                                }
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
                    this.DrawString(this.foregroundGraphic, "+" + variation, axisFont, Brushes.Green, textBackgroundBrush, new PointF(point2.X + 10, point2.Y - 20), true);
                }
                else
                {
                    this.DrawString(this.foregroundGraphic, variation, axisFont, Brushes.Red, textBackgroundBrush, new PointF(point2.X + 10, point2.Y - 20), true);
                }
            }
        }
        protected override void DrawMousePos(int indexInValues, float y)
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
                        //this.foregroundGraphic.DrawEllipse(greenPen, screenPoint.X - MOUSE_MARQUEE_SIZE, screenPoint.Y - MOUSE_MARQUEE_SIZE, MOUSE_MARQUEE_SIZE * 2, MOUSE_MARQUEE_SIZE * 2);
                    }
                    else
                    {
                        value = low;
                        screenPoint = GetScreenPointFromValuePoint(indexInValues, low);
                        //this.foregroundGraphic.DrawEllipse(redPen, screenPoint.X - MOUSE_MARQUEE_SIZE, screenPoint.Y - MOUSE_MARQUEE_SIZE, MOUSE_MARQUEE_SIZE * 2, MOUSE_MARQUEE_SIZE * 2);
                    }
                }
                else
                {
                    value = closeCurveType.DataSerie[indexInValues];
                    screenPoint = GetScreenPointFromValuePoint(new PointF(indexInValues, value));
                    //this.foregroundGraphic.DrawEllipse(mousePen, screenPoint.X - MOUSE_MARQUEE_SIZE, screenPoint.Y - MOUSE_MARQUEE_SIZE, MOUSE_MARQUEE_SIZE * 2, MOUSE_MARQUEE_SIZE * 2);
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
                this.DrawString(this.foregroundGraphic, dateString, axisFont, Brushes.Black, textBackgroundBrush, dateLocation, true);
                this.DrawString(this.foregroundGraphic, value.ToString("0.####"), axisFont, textBrush, textBackgroundBrush, new PointF(GraphRectangle.Right + 2, screenPoint.Y - 8), true);
            }
        }
        protected void DrawEntry(Graphics graph, Pen pen, float index, float stop, bool showText)
        {
            var p1 = this.GetScreenPointFromValuePoint(index, stop);
            var p2 = new PointF(GraphRectangle.Right, p1.Y);
            graph.DrawLine(pen, p1, p2);
            var points = GetEntryMarqueePoints(stop);
            graph.FillPolygon(new SolidBrush(pen.Color), points);
            if (showText)
                this.DrawString(graph, stop.ToString("0.####") + " ", axisFont, textBrush, textBackgroundBrush, new PointF(GraphRectangle.Right + 2, p1.Y - 8), true);
        }
        protected void DrawStop(Graphics graph, Pen pen, float index, float stop, bool showText)
        {
            var p1 = this.GetScreenPointFromValuePoint(index, stop);
            var p2 = new PointF(GraphRectangle.Right, p1.Y);
            graph.DrawLine(pen, p1, p2);
            var points = GetStopMarqueePoints(stop);
            graph.FillPolygon(new SolidBrush(pen.Color), points);
            if (showText)
                this.DrawString(graph, stop.ToString("0.####") + " ", axisFont, textBrush, textBackgroundBrush, new PointF(GraphRectangle.Right + 2, p1.Y - 8), true);
        }
        protected void DrawOpenedOrder(Graphics graph, Pen pen, float index, float value, bool showText)
        {
            var p1 = this.GetScreenPointFromValuePoint(index, value);
            var p2 = new PointF(GraphRectangle.Right, p1.Y);
            graph.DrawLine(pen, p1, p2);
            var points = GetStopMarqueePoints(value);
            graph.FillPolygon(new SolidBrush(pen.Color), points);
            if (showText)
                this.DrawString(graph, value.ToString("0.####") + " ", axisFont, textBrush, textBackgroundBrush, new PointF(GraphRectangle.Right + 2, p1.Y - 8), true);
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
            if (segment1.Length < segment2.Length)
            {
                returnPoint = new PointF(selectedIndex, highSerie[selectedIndex]);
            }
            else
            {
                returnPoint = new PointF(selectedIndex, lowSerie[selectedIndex]);
            }
            return returnPoint;
        }
        private void HighlightClosestLine(MouseEventArgs e)
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
            foreach (var line2D in drawingItems.Where(di => di.IsPersistent && di is Line2DBase).Cast<Line2DBase>()) // There is an issue here as it supports only persistent items. Does't work with generated line.
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
        #region Order Management
        void addAlertMenu_Click(object sender, EventArgs e)
        {
            StockAnalyzerForm.MainFrame.showAlertDefDialogMenuItem_Click(this, null);
        }
        float FindStopValueFromTheme()
        {
            if (CurveList?.TrailStop?.Series[0] != null)
            {
                if (CurveList.TrailStop.Series[0].Count > 0 && !float.IsNaN(CurveList.TrailStop.Series[0][this.EndIndex]))
                {
                    return CurveList.TrailStop.Series[0][this.EndIndex];
                }
            }
            return 0;
        }
        float FindLongReentryValueFromTheme()
        {
            if (CurveList?.TrailStop?.Series[0] != null && CurveList.TrailStop.Series[0].Count > 1)
            {

                if (!float.IsNaN(CurveList.TrailStop.Series[2][this.EndIndex]))
                {
                    return CurveList.TrailStop.Series[2][this.EndIndex];
                }
            }
            return 0;
        }
        private OpenTradeViewModel openTradeViewModel = null;
        void buyMenu_Click(object sender, EventArgs e)
        {
            if (StockAnalyzerForm.MainFrame.Portfolio == null)
            {
                MessageBox.Show("Please select a valid portfolio", "Invalid Portfolio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (lastMouseIndex == -1 || this.openCurveType == null || this.dateSerie == null)
                return;

            var portfolioValue = Portfolio.TotalValue;
            openTradeViewModel = new OpenTradeViewModel
            {
                StockSerie = this.serie,
                BarDuration = StockAnalyzerForm.MainFrame.ViewModel.BarDuration,
                EntryValue = this.closeCurveType.DataSerie[EndIndex],
                StopValue = FindStopValueFromTheme(),
                LongReentry = FindLongReentryValueFromTheme(),
                Portfolio = this.Portfolio,
                Themes = StockAnalyzerForm.MainFrame.Themes,
                Theme = StockAnalyzerForm.MainFrame.CurrentTheme.Contains("*") ? null : StockAnalyzerForm.MainFrame.CurrentTheme
            };
            openTradeViewModel.OrdersChanged += OpenTradeViewModel_OrdersChanged;
            openTradeViewModel.Refresh(false);

            this.IsBuying = true;
            this.OnMouseValueChanged += openTradeViewModel.OnOrderValueChanged;
            OpenPositionDlg openPositionDlg = new OpenPositionDlg(openTradeViewModel) { StartPosition = FormStartPosition.CenterScreen };
            openPositionDlg.Show(this);
            openPositionDlg.FormClosed += (a, b) =>
            {
                this.IsBuying = false;
                this.OnMouseValueChanged -= openTradeViewModel.OnOrderValueChanged;
                if (openPositionDlg.DialogResult == DialogResult.OK)
                {
                    this.BackgroundDirty = true;
                    PaintGraph();
                }
                this.openTradeViewModel.OrdersChanged -= OpenTradeViewModel_OrdersChanged;
                this.openTradeViewModel = null;
            };
        }

        private void OpenTradeViewModel_OrdersChanged()
        {
            if (openTradeViewModel.StopValue != 0)
                this.DrawOpenedOrder(this.foregroundGraphic, stopPen, this.StartIndex, openTradeViewModel.StopValue, true);
            this.DrawOpenedOrder(this.foregroundGraphic, entryOrderPen, this.EndIndex, openTradeViewModel.EntryValue, true);

            this.PaintForeground();
        }

        void tradeMenu_Click(object sender, EventArgs e)
        {
            var tradeManagerViewModel = new TradeManagerViewModel(Portfolio, this.serie);
            tradeManagerViewModel.OrdersChanged += TradeManagerViewModel_OrdersChanged;
            var tradeManagerDlg = new TradeManagerDlg(tradeManagerViewModel);
            tradeManagerDlg.Show(this);
            tradeManagerDlg.FormClosed += (a, b) =>
            {
                tradeManagerViewModel.OrdersChanged -= TradeManagerViewModel_OrdersChanged;
            };
        }

        private void TradeManagerViewModel_OrdersChanged(TradeManagerViewModel tradeManagerViewModel)
        {
            if (tradeManagerViewModel.EntryStop != 0)
                this.DrawOpenedOrder(this.foregroundGraphic, stopPen, this.StartIndex, tradeManagerViewModel.EntryStop, true);
            this.DrawOpenedOrder(this.foregroundGraphic, entryOrderPen, this.EndIndex, tradeManagerViewModel.Bid, true);

            this.PaintForeground();
        }

        private void CloseTradeViewModel_OrdersChanged()
        {
            this.DrawOpenedOrder(this.foregroundGraphic, stopPen, this.EndIndex, closeTradeViewModel.ExitValue, true);
            this.PaintForeground();
        }


        CloseTradeViewModel closeTradeViewModel = null;
        void sellMenu_Click(object sender, EventArgs e)
        {
            if (StockAnalyzerForm.MainFrame.Portfolio == null)
            {
                MessageBox.Show("Please select a valid portfolio", "Invalid Portfolio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var pos = StockAnalyzerForm.MainFrame.Portfolio.Positions.FirstOrDefault(p => p.StockName == this.serie.StockName && p.IsClosed == false);
            if (pos == null)
            {
                MessageBox.Show("Cannot sell not opened position", "Invalid Order", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            closeTradeViewModel = new CloseTradeViewModel
            {
                StockSerie = this.serie,
                Position = pos,
                ExitValue = this.serie.LastValue.CLOSE,
                ExitQty = pos.EntryQty,
                StockName = this.serie.StockName,
                Portfolio = this.Portfolio
            };
            closeTradeViewModel.CalculateTickSize();

            this.IsSelling = true;
            this.OnMouseValueChanged += closeTradeViewModel.OnOrderValueChanged;
            var closePositionDlg = new ClosePositionDlg(closeTradeViewModel) { StartPosition = FormStartPosition.CenterScreen };
            closePositionDlg.Show(this);
            closePositionDlg.FormClosed += (a, b) =>
            {
                this.IsSelling = false;
                this.OnMouseValueChanged -= closeTradeViewModel.OnOrderValueChanged;
                if (closePositionDlg.DialogResult == DialogResult.OK)
                {
                    this.BackgroundDirty = true;
                    PaintGraph();
                }
                this.closeTradeViewModel.OrdersChanged -= CloseTradeViewModel_OrdersChanged;
                this.closeTradeViewModel = null;
            };
            this.BackgroundDirty = true;
            PaintGraph();
        }
        void cancelMenu_Click(object sender, EventArgs e)
        {
            if (StockAnalyzerForm.MainFrame.Portfolio == null)
            {
                MessageBox.Show("Please select a valid portfolio", "Invalid Portfolio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var order = StockAnalyzerForm.MainFrame.Portfolio.GetActiveOrders(this.serie.StockName).FirstOrDefault(o => o.BuySell == "Buy");
            if (order == null)
            {
                MessageBox.Show("Cannot sell not opened position", "Invalid Order", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            StockAnalyzerForm.MainFrame.Portfolio.SaxoCancelOpenOrder(order.Id);
            this.BackgroundDirty = true;
            PaintGraph();
        }
        void agendaMenu_Click(object sender, EventArgs e)
        {
            StockAnalyzerForm.MainFrame.ShowAgenda();
        }
        void openInZBMenu_Click(object sender, EventArgs e)
        {
            StockAnalyzerForm.MainFrame.OpenInZBMenu();
        }
        void openInTradingViewMenu_Click(object sender, EventArgs e)
        {
            StockAnalyzerForm.MainFrame.openInTradingViewMenu();
        }
        void openInDataProvider_Click(object sender, EventArgs e)
        {
            StockAnalyzerForm.MainFrame.OpenInDataProvider();
        }
        void openInYahoo_Click(object sender, EventArgs e)
        {
            StockAnalyzerForm.MainFrame.OpenInYahoo();
        }
        void openSaxoIntradyConfigDlg_Click(object sender, EventArgs e)
        {
            StockAnalyzerForm.MainFrame.OpenSaxoIntradyConfigDlg(this.serie.SaxoId);
        }
        #endregion
    }
}