using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrails;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.GraphControls
{
    public class InvalidSerieException : Exception
    {
        public InvalidSerieException(string message)
            : base(message)
        {
        }
    }

    public enum GraphChartMode
    {
        Line,
        BarChart,
        CandleStick
    }
    public enum GraphDrawMode
    {
        Normal,
        AddLine,
        AddBox,
        AddWinRatio,
        AddSegment,
        AddCupHandle,
        AddHalfLine,
        CopyLine,
        CutLine,
        DeleteItem
    }
    public enum GraphDrawingStep
    {
        SelectItem,
        ItemSelected,
        Done
    }

    public delegate void OnZoomChangedHandler(int startIndex, int endIndex);
    public delegate void MouseValueChangedHandler(FullGraphUserControl sender, DateTime date, float value, bool crossMode);

    public partial class GraphControl : Panel
    {
        public event MouseValueChangedHandler OnMouseValueChanged;

        // Constants
        protected const int HEIGHT_MARGIN_SIZE = 18;
        protected const int WIDTH_MARGIN_SIZE = 30;
        protected const int MOUSE_MARQUEE_SIZE = 3;
        protected const int EVENT_MARQUEE_SIZE = 4;
        protected const int ORDER_AREA_WITDH = 20;

        virtual public GraphDrawMode DrawingMode { get; set; }
        public GraphDrawingStep DrawingStep { get; set; }

        // DataMembers
        protected GraphCurveTypeList curveList;
        public virtual GraphCurveTypeList CurveList
        {
            get
            {
                return this.curveList;
            }
            set
            {
                this.curveList = value;
            }
        }
        public List<HLine> horizontalLines { get; set; }

        public GraphChartMode ChartMode { get; set; }

        protected float minValue = float.MaxValue;
        protected float maxValue = float.MinValue;
        public bool IsLogScale { get; set; }
        public bool IsInverse { get; set; }
        public bool ScaleInvisible { get; set; }
        public bool ShowGrid { get; set; }
        protected List<GraphAction> GraphActions { get; set; }
        protected int currentActionIndex;

        public bool IsInitialized { get; protected set; }
        protected DateTime[] dateSerie;

        protected StockSerie serie;
        public int EndIndex { get; set; }
        public int StartIndex { get; set; }

        protected StockDrawingItems drawingItems;

        private bool graphBackgroundDirty;
        protected bool BackgroundDirty
        {
            get { return graphBackgroundDirty; }
            set
            {
                graphBackgroundDirty = value;
                if (this.graphBackgroundDirty)
                {
                    PaintGraph();
                    this.ForegroundDirty = true;
                }
            }
        }
        private bool graphForegroundDirty;
        protected bool ForegroundDirty
        {
            get { return graphForegroundDirty; }
            set
            {
                graphForegroundDirty = value;
                if (graphForegroundDirty)
                {
                    if (this.backgroundBitmap != null)
                    {
                        if (foregroundBitmap != null) { this.foregroundBitmap.Dispose(); }
                        this.foregroundBitmap = (Bitmap)this.backgroundBitmap.Clone();
                        if (this.foregroundGraphic != null) { this.foregroundGraphic.Dispose(); }
                        this.foregroundGraphic = Graphics.FromImage(this.foregroundBitmap);
                    }
                    else
                    {
                        //StockLog.Write("Background Bitmap is null");
                    }
                }
            }
        }
        protected Bitmap backgroundBitmap = null;
        protected Bitmap foregroundBitmap = null;
        protected Graphics foregroundGraphic = null;
        protected Graphics graphic = null;
        public RectangleF GraphRectangle { get; protected set; }
        protected int XMargin = 0;
        protected int YMargin = 0;

        // Last mouse index in visible points
        protected int lastMouseIndex = -1;
        protected FloatSerie mainSerie = null;
        protected System.Drawing.Point mouseDownPos = Point.Empty;
        protected bool forceNoValueBoxDisplay = false;

        // Resources
        protected Font axisFont;
        protected Font toolTipFont;
        protected Pen framePen;
        protected Pen textFramePen;
        protected Pen axisPen;
        protected Pen axisDashPen;
        protected Pen mousePen;
        protected Pen HigherHighPen;
        protected Pen LowerLowPen;

        static protected Pen selectedLinePen = new Pen(Color.Turquoise, 2.0f);
        static protected Pen redPen = new Pen(Color.Red, 1.0f);
        static protected Pen greenPen = new Pen(Color.Green, 1.0f);
        static protected Brush greenBrush = new SolidBrush(Color.FromArgb(30, Color.Green));
        static protected Brush redBrush = new SolidBrush(Color.FromArgb(30, Color.Red));
        static protected Brush textBrush = Brushes.Black;

        static protected Pen entryPen = new Pen(Color.Black, 2.0f) { DashStyle = DashStyle.Solid, EndCap = LineCap.DiamondAnchor, StartCap = LineCap.RoundAnchor };
        static protected Pen entryOrderPen = new Pen(Color.Black, 2.0f) { DashStyle = DashStyle.Dot, EndCap = LineCap.DiamondAnchor, StartCap = LineCap.RoundAnchor };
        static protected Pen stopPen = new Pen(Color.Red, 2.0f);
        static protected Pen trailStopPen = new Pen(Color.Red, 2.0f) { DashStyle = DashStyle.Dot, EndCap = LineCap.DiamondAnchor, StartCap = LineCap.RoundAnchor };
        static protected Brush orderAreaBrush => new SolidBrush(Color.AliceBlue);

        protected bool mouseDown = false;

        protected Brush backgroundBrush;
        private Color backgroundColor;
        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                if (this.backgroundBrush != null)
                {
                    this.backgroundBrush.Dispose();
                }
                this.backgroundBrush = new SolidBrush(value);
                backgroundColor = value;
            }
        }

        protected Brush textBackgroundBrush;
        private Color textBackgroundColor;
        public Color TextBackgroundColor
        {
            get { return textBackgroundColor; }
            set
            {
                if (this.textBackgroundBrush != null)
                {
                    this.textBackgroundBrush.Dispose();
                }
                this.textBackgroundBrush = new SolidBrush(value);
                textBackgroundColor = value;
            }
        }

        protected Pen gridPen;
        private Color gridColor;
        public Color GridColor
        {
            get { return gridColor; }
            set
            {
                if (gridPen != null)
                {
                    gridPen.Dispose();
                }
                gridPen = new Pen(value);
                gridColor = value;
            }
        }
        public Pen DrawingPen { get; set; }
        static public Pen MouseCursorPen => new Pen(Brushes.Black, 2);

        public static Brush CupHandleBrush => new SolidBrush(Color.FromArgb(32, Color.LightGreen));
        public static Brush CupHandleInvBrush => new SolidBrush(Color.FromArgb(32, Color.LightCoral));

        // Transformation Matrix
        protected Matrix matrixScreenToValue;
        protected Matrix matrixValueToScreen;
        static protected Matrix matrixIdentity = new Matrix();

        protected string alternateString = string.Empty;

        public GraphControl()
        {
            InitializeComponent();
            this.IsLogScale = false;
            this.IsInitialized = false;
            this.ScaleInvisible = false;
            this.ShowGrid = false;
            this.graphBackgroundDirty = true;
            this.graphBackgroundDirty = true;

            // Pens
            framePen = new Pen(Color.Black, 1.0f);
            textFramePen = new Pen(Color.Black, 1.0f);
            gridPen = new Pen(Color.LightGray, 1.0f);
            axisPen = new Pen(Color.Black, 1.0f);
            axisDashPen = new Pen(Color.FromArgb(0x60, 0x60, 0x60), 1.0f) { DashStyle = DashStyle.Dash };
            axisFont = new Font(FontFamily.GenericSansSerif, 7);
            toolTipFont = new Font(FontFamily.GenericMonospace, 8);
            mousePen = new Pen(Color.Black, 1.0f);
            HigherHighPen = new Pen(Color.Green, 1.0f);
            LowerLowPen = new Pen(Color.Red, 1.0f);
            this.BackgroundColor = Color.White;
            SetFrameMargin();
        }
        public void Initialize(GraphCurveTypeList curveList, List<HLine> horizontallines, DateTime[] dateSerie, StockSerie serie, StockDrawingItems drawingItems, int startIndex, int endIndex)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                this.dateSerie = dateSerie;
                this.CurveList = curveList;
                this.StartIndex = startIndex;
                this.EndIndex = endIndex;
                this.serie = serie;
                this.drawingItems = drawingItems;
                this.horizontalLines = horizontallines;

                // Initialise undo buffer
                this.GraphActions = new List<GraphAction>();
                this.currentActionIndex = -1;

                // Initialise graphics
                this.graphic = this.CreateGraphics();
                RectangleF rect = this.graphic.VisibleClipBounds;
                rect.Inflate(new SizeF(-this.XMargin * 1.5f, -this.YMargin));
                rect.Offset(new PointF(this.XMargin * -.5f, 0));
                this.GraphRectangle = rect;

                this.IsInitialized = true;
                this.alternateString = string.Empty;

                this.DrawingStep = GraphDrawingStep.SelectItem;

                drawingItems.ApplyDateOffset(dateSerie);
            }
        }
        public void Deactivate(string msg, bool setInitialisedTo)
        {
            using (MethodLogger ml = new MethodLogger(this, false, $"{msg},{setInitialisedTo}"))
            {
                //StockLog.Write(msg);
                this.alternateString = msg;
                this.graphic = null;
                this.IsInitialized = setInitialisedTo;
                this.ForceRefresh();
            }
        }
        protected bool CheckGraphSanity()
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                if (!this.IsInitialized || this.dateSerie == null)
                {
                    if (string.IsNullOrWhiteSpace(this.alternateString))
                    {
                        this.Deactivate("Graph Not initialised", false);
                    }
                    return false;
                }
                if (this.CurveList == null || this.CurveList.GetNbVisible() == 0)
                {
                    this.Deactivate("No data to display...", false);
                    return false;
                }
                return true;
            }
        }
        public void OnZoomChanged(int startIndex, int endIndex)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                if (!CheckGraphSanity()) { return; }
                if (startIndex == endIndex || startIndex < 0 || endIndex > this.dateSerie.Length - 1)
                {
                    this.Deactivate("Invalid input data range...", false);
                    return;
                }
                this.StartIndex = startIndex;
                this.EndIndex = endIndex;

                // Initialise transformation matrix
                this.IsInitialized = InitializeTransformMatrix();
                this.ForceRefresh();
            }
        }
        virtual protected bool InitializeTransformMatrix()
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                if (!CheckGraphSanity()) { return false; }
                if (this.StartIndex == this.EndIndex || this.EndIndex > this.dateSerie.Length - 1)
                {
                    this.IsInitialized = false;
                    InvalidSerieException e = new InvalidSerieException("Invalid input data range...");
                    StockLog.Write(e);
                    throw e;
                }
                if (this.GraphRectangle.Height > 0)
                {
                    minValue = float.MaxValue;
                    maxValue = float.MinValue;
                    this.CurveList.GetMinMax(StartIndex, EndIndex, ref minValue, ref maxValue, this.ScaleInvisible);

                    if (minValue == 0.0f && maxValue == 0.0f)
                    {
                        minValue = -1.0f;
                        maxValue = 1.0f;
                    }
                    else if (minValue == maxValue || minValue == float.MaxValue || float.IsNaN(minValue) || float.IsInfinity(minValue) || maxValue == float.MinValue || float.IsNaN(maxValue) || float.IsInfinity(maxValue))
                    {
                        this.Deactivate("Input data is corrupted and cannot be displayed...", false);
                        return false;
                    }

                    if (this.IsLogScale && minValue > 0)
                    {
                        minValue *= 0.95f;
                    }
                    else
                    {
                        minValue -= (maxValue - minValue) * 0.1f;
                    }
                    maxValue += (maxValue - minValue) * 0.1f;

                    float tmpMinValue, tmpMaxValue;
                    if (this.IsLogScale)
                    {
                        tmpMinValue = minValue < 0 ? (float)-Math.Log10(-minValue + 1) : (float)Math.Log10(minValue + 1);
                        tmpMaxValue = maxValue < 0 ? (float)-Math.Log10(-maxValue + 1) : (float)Math.Log10(maxValue + 1);
                    }
                    else
                    {
                        tmpMinValue = minValue;
                        tmpMaxValue = maxValue;
                    }

                    float coefX = (this.GraphRectangle.Width - this.XMargin) / (EndIndex - StartIndex + 1);
                    float coefY = this.GraphRectangle.Height / (tmpMaxValue - tmpMinValue);

                    matrixValueToScreen = new Matrix();
                    matrixValueToScreen.Translate(this.GraphRectangle.X - (StartIndex - 0.5f) * coefX, tmpMaxValue * coefY + this.GraphRectangle.Y);
                    if (IsInverse)
                    {
                        coefY = -coefY;
                    }
                    matrixValueToScreen.Scale(coefX, -coefY);

                    matrixScreenToValue = (Matrix)matrixValueToScreen.Clone();
                    matrixScreenToValue.Invert();
                }
                else
                {
                    this.Deactivate("App too small...", false);
                    return false;
                }
                return true;
            }
        }
        protected virtual void SetFrameMargin()
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                this.XMargin = WIDTH_MARGIN_SIZE;
                this.YMargin = 0;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            using (MethodLogger ml = new MethodLogger(this, false))
            {
                if (this.GetType().ToString() == "StockAnalyzerApp.CustomControl.GraphControls.GraphCloseControl")
                {
                    if (this.alternateString == "App too small..." && this.FindForm().WindowState != FormWindowState.Normal)
                    {
                        this.Deactivate("", true);
                    }
                }
                if (!CheckGraphSanity()) { return; }
                try
                {
                    // Initialise graphics
                    if (this.graphic != null)
                    {
                        this.graphic.Dispose();
                    }
                    if (this.Size.Height == 1 || this.Size.Width == 1)
                    {
                        this.graphic = null;
                        return;
                    }
                    this.graphic = this.CreateGraphics();
                    RectangleF rect = this.graphic.VisibleClipBounds;
                    rect.Inflate(new SizeF(-this.XMargin * 1.5f, -this.YMargin));
                    rect.Offset(new PointF(this.XMargin * -.5f, 0));
                    this.GraphRectangle = rect;

                    // Initialise transformation Matrix
                    this.IsInitialized = InitializeTransformMatrix();
                    if (this.IsInitialized)
                    {
                        this.alternateString = string.Empty;
                    }
                    this.BackgroundDirty = true;
                }
                catch (Exception exception)
                {
                    StockLog.Write(exception);
                }
            }
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                PaintGraph();
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                PaintGraph();
            }
        }
        protected virtual void PaintGraph()
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                if (this.IsInitialized && this.graphic != null)
                {
                    try
                    {
                        // Draw the graph on the background image
                        if (BackgroundDirty)
                        {
                            // Create Bitmap graphicMain
                            backgroundBitmap = new Bitmap((int)this.graphic.VisibleClipBounds.Width, (int)this.graphic.VisibleClipBounds.Height, this.graphic);
                            Graphics tmpGraph = Graphics.FromImage(backgroundBitmap);

                            tmpGraph.Clear(this.backgroundColor);
                            PaintTmpGraph(tmpGraph);
                            PaintGraphTitle(tmpGraph);
                            PaintCopyright(tmpGraph);

                            // Draw background image
                            this.graphic.DrawImage(backgroundBitmap, 0, 0);

                            this.BackgroundDirty = false;
                            this.ForegroundDirty = true;
                        }
                        else
                        {
                            // Draw background image
                            this.graphic.DrawImage(backgroundBitmap, 0, 0);
                        }
                    }
                    catch (Exception e)
                    {
                        this.Deactivate("Software Error: " + e.Message, false);
                    }
                }
                else // Draw alternate text.
                {
                    Graphics gr = this.CreateGraphics();
                    gr.Clear(this.backgroundColor);
                    gr.DrawString(this.alternateString, axisFont, Brushes.Black, 10, 20);
                }
            }
        }

        protected virtual void PaintCopyright(Graphics aGraphic)
        {
        }
        public void PaintForeground()
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                if (this.graphic != null)
                {
                    this.graphic.DrawImage(foregroundBitmap, 0, 0);
                    this.ForegroundDirty = true;
                }
            }
        }
        public void ForceRefresh()
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                this.BackgroundDirty = true;
            }
        }
        public Bitmap GetSnapshot()
        {
            if (this.IsInitialized && this.backgroundBitmap != null)
            {
                Bitmap snapshot = (Bitmap)this.backgroundBitmap.Clone();
                Graphics g = Graphics.FromImage(snapshot);
                g.DrawRectangle(Pens.Black, 0, 0, snapshot.Width - 1, snapshot.Height - 1);

                using (var stream = new MemoryStream())
                {
                    snapshot.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                }

                return snapshot;
            }
            else
                return null;
        }

        public string GetSnapshotAsHTML()
        {
            if (this.IsInitialized && this.backgroundBitmap != null)
            {
                Bitmap snapshot = (Bitmap)this.backgroundBitmap.Clone();
                Graphics g = Graphics.FromImage(snapshot);
                g.DrawRectangle(Pens.Black, 0, 0, snapshot.Width - 1, snapshot.Height - 1);

                using (var stream = new MemoryStream())
                {
                    snapshot.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    return "data:image/png;base64," + Convert.ToBase64String(stream.ToArray());
                }
            }
            return null;
        }

        protected virtual void PaintTmpGraph(Graphics aGraphic)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                #region Draw vertical lines
                if (this.ShowGrid && this.matrixValueToScreen != null)
                {
                    DrawVerticalGridLines(aGraphic, false, this.StartIndex, this.EndIndex);
                }
                #endregion

                // Paint horizontal lines first
                this.PaintHorizontalLines(aGraphic);

                PointF[] points = null;
                int i = 0;
                #region DISPLAY INDICATORS
                this.mainSerie = null;
                foreach (IStockIndicator stockIndicator in CurveList.Indicators)
                {
                    if (stockIndicator is IStockTrail)
                    {
                        BoolSerie upTrend = (stockIndicator as IStockEvent).Events[0];
                        for (i = 0; i < stockIndicator.SeriesCount; i++)
                        {
                            if (stockIndicator.SerieVisibility[i] && stockIndicator.Series[i].Count > 0)
                            {
                                points = GetScreenPoints(StartIndex, EndIndex, stockIndicator.Series[i]);
                                int index = StartIndex;
                                if (points != null)
                                {
                                    Brush upBrush = Brushes.DarkGreen;
                                    Brush downBrush = Brushes.DarkRed;
                                    float pointSize = stockIndicator.SeriePens[i].Width;
                                    foreach (var srPoint in points)
                                    {
                                        if (upTrend[index])
                                        {
                                            aGraphic.FillEllipse(upBrush, srPoint.X - pointSize, srPoint.Y - pointSize, 2 * pointSize, 2 * pointSize);
                                        }
                                        else
                                        {
                                            aGraphic.FillEllipse(downBrush, srPoint.X - pointSize, srPoint.Y - pointSize, 2 * pointSize, 2 * pointSize);
                                        }
                                        index++;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (i = 0; i < stockIndicator.SeriesCount; i++)
                        {
                            if (stockIndicator.SerieVisibility[i] && stockIndicator.Series[i].Count > 0)
                            {
                                points = GetScreenPoints(StartIndex, EndIndex, stockIndicator.Series[i]);
                                if (points != null)
                                {
                                    // The first serie is the one to use for decorator if any. We make the best of this test to display the last value on the right
                                    if (this.mainSerie == null)
                                    {
                                        this.mainSerie = stockIndicator.Series[i];
                                        // Display values and dates
                                        float lastValue = this.mainSerie[EndIndex];
                                        string lastValueString;
                                        if (lastValue > 100000000)
                                        {
                                            lastValueString = (lastValue / 1000000).ToString("0.#") + "M";
                                        }
                                        else if (lastValue > 100000)
                                        {
                                            lastValueString = (lastValue / 1000).ToString("0.#") + "K";
                                        }
                                        else
                                        {
                                            lastValueString = lastValue.ToString("0.##");
                                        }

                                        aGraphic.DrawString(lastValueString, axisFont, Brushes.Black, GraphRectangle.Right + 1, Math.Max(points.Last().Y - 8, GraphRectangle.Top));
                                    }
                                    if (stockIndicator.SeriePens[i].DashStyle == DashStyle.Custom)
                                    {
                                        PointF center = GetScreenPointFromValuePoint(0, 0f);
                                        if (stockIndicator is IRange)
                                        {
                                            var range = stockIndicator as IRange;
                                            center = GetScreenPointFromValuePoint(0, (range.Max + range.Min) / 2.0f);
                                        }
                                        int pointIndex = 0;
                                        float barWidth = Math.Max(1f, 0.80f * GraphRectangle.Width / (float)points.Count());
                                        foreach (PointF point in points)
                                        {
                                            // Select brush color
                                            if (point.Y < center.Y)
                                            {
                                                if (pointIndex == 0)
                                                {
                                                    aGraphic.FillRectangle(Brushes.Green, point.X, point.Y, barWidth / 2, center.Y - point.Y);
                                                }
                                                else if (i == points.Count() - 1)
                                                {
                                                    aGraphic.FillRectangle(Brushes.Green, point.X - barWidth / 2, point.Y, barWidth / 2, center.Y - point.Y);
                                                }
                                                else
                                                {
                                                    aGraphic.FillRectangle(Brushes.Green, point.X - barWidth / 2, point.Y, barWidth, center.Y - point.Y);
                                                }
                                            }
                                            else
                                            {
                                                if (pointIndex == 0)
                                                {
                                                    aGraphic.FillRectangle(Brushes.Red, point.X, center.Y, barWidth / 2, point.Y - center.Y);
                                                }
                                                else if (i == points.Count() - 1)
                                                {
                                                    aGraphic.FillRectangle(Brushes.Red, point.X - barWidth / 2, center.Y, barWidth / 2, point.Y - center.Y);
                                                }
                                                else
                                                {
                                                    aGraphic.FillRectangle(Brushes.Red, point.X - barWidth / 2, center.Y, barWidth, point.Y - center.Y);
                                                }
                                            }
                                            pointIndex++;
                                        }
                                    }
                                    else
                                    {
                                        aGraphic.DrawLines(stockIndicator.SeriePens[i], points);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
                #region DISPLAY DECORATORS
                if (CurveList.Decorator != null && this.mainSerie != null)
                {
                    for (i = 0; i < CurveList.Decorator.SeriesCount; i++)
                    {
                        if (CurveList.Decorator.SerieVisibility[i])
                        {
                            Pen pen = CurveList.Decorator.SeriePens[i];
                            using (Brush brush = new SolidBrush(pen.Color))
                            {
                                points = GetScreenPoints(StartIndex, EndIndex, CurveList.Decorator.Series[i]);
                                if (points != null)
                                {

                                    aGraphic.DrawLines(CurveList.Decorator.SeriePens[i], points);
                                }
                            }
                        }
                    }
                    for (i = 0; i < CurveList.Decorator.EventCount; i++)
                    {
                        if (CurveList.Decorator.EventVisibility[i] && CurveList.Decorator.IsEvent[i])
                        {
                            Pen pen = CurveList.Decorator.EventPens[i];
                            using (Brush brush = new SolidBrush(pen.Color))
                            {
                                BoolSerie decoSerie = CurveList.Decorator.Events[i];
                                for (int index = this.StartIndex; index <= this.EndIndex; index++)
                                {
                                    if (decoSerie[index])
                                    {
                                        PointF point = new PointF(index, mainSerie[index]);
                                        PointF point2 = GetScreenPointFromValuePoint(point);
                                        aGraphic.FillEllipse(brush, point2.X - pen.Width * 1.5f, point2.Y - pen.Width * 1.5f, pen.Width * 3f, pen.Width * 3f);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }
        }
        protected virtual void PaintGraphTitle(Graphics aGraphic)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                bool first = true;
                string graphTitle = string.Empty;
                foreach (GraphCurveType curveType in this.CurveList)
                {
                    if (curveType.IsVisible)
                    {
                        if (first)
                        {
                            graphTitle = curveType.DataSerie.Name;
                            first = false;
                        }
                        else
                        {
                            graphTitle += "  " + curveType.ToString();
                        }
                    }
                }
                // Add indicators
                foreach (IStockIndicator stockIndicator in CurveList.Indicators)
                {
                    graphTitle += "  " + stockIndicator.Name;
                }
                this.DrawString(aGraphic, graphTitle, this.axisFont, Brushes.Black, this.textBackgroundBrush, new PointF(1, 1), true);
            }
        }
        protected virtual void PaintDailyBox(PointF mousePoint)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                string value = string.Empty;
                foreach (GraphCurveType curveType in this.CurveList)
                {
                    if (curveType.IsVisible && !float.IsNaN(curveType.DataSerie[this.lastMouseIndex]))
                    {
                        value += BuildTabbedString(curveType.DataSerie.Name, curveType.DataSerie[this.lastMouseIndex], 12) + "\r\n";
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
                // Remove last new line.
                if (value.Length != 0)
                {
                    value = value.Remove(value.LastIndexOf("\r\n"));
                }
                if (!string.IsNullOrWhiteSpace(value))
                {
                    using (Font font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 8))
                    {
                        Size size = TextRenderer.MeasureText(value, font);

                        PointF point = new PointF(Math.Min(mousePoint.X + 10, GraphRectangle.Right - size.Width), GraphRectangle.Top + 5);

                        this.DrawString(this.foregroundGraphic, value, font, Brushes.Black, this.backgroundBrush, point, true);
                    }
                }
            }
        }
        protected void PaintHorizontalLines(Graphics g)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                if (this.horizontalLines != null)
                {
                    foreach (HLine hLine in this.horizontalLines)
                    {
                        PointF levelPoint = this.GetScreenPointFromValuePoint(this.StartIndex, hLine.Level);
                        g.DrawLine(hLine.LinePen, this.GraphRectangle.X, levelPoint.Y, this.GraphRectangle.Right, levelPoint.Y);
                    }
                }
            }
        }
        #region POINTS TRANSFORMATION
        public PointF[] GetScreenPoints(int startIndex, int endIndex, FloatSerie floatSerie)
        {
            PointF[] points = null;
            if (floatSerie.Count > 0)
            {
                points = new PointF[endIndex - startIndex + 1];
                int count = 0;
                for (int i = startIndex; i <= endIndex; i++)
                {
                    points[count++] = GetScreenPointFromValuePoint(new PointF(i, floatSerie.Values[i]));
                }
            }
            return points;
        }
        protected PointF GetValuePointFromScreenPoint(PointF point2D)
        {
            PointF[] points = new PointF[] { point2D };

            this.matrixScreenToValue.TransformPoints(points);
            if (this.IsLogScale)
            {
                points[0].Y = points[0].Y < 0 ? (float)-Math.Pow(10, -points[0].Y) + 1 : (float)Math.Pow(10, points[0].Y) - 1;
            }
            points[0].X = Math.Min(this.EndIndex, points[0].X);
            return points[0];
        }
        protected PointF GetValuePointFromScreenPoint(float x, float y)
        {
            PointF[] points = new PointF[] { new PointF(x, y) };
            this.matrixScreenToValue.TransformPoints(points);
            if (this.IsLogScale)
            {
                points[0].Y = points[0].Y < 0 ? (float)-Math.Pow(10, -points[0].Y) + 1 : (float)Math.Pow(10, points[0].Y) - 1;
            }
            points[0].X = Math.Min(this.EndIndex, points[0].X);
            return points[0];
        }
        protected PointF GetScreenPointFromValuePoint(PointF point2D)
        {
            PointF[] points;
            if (this.IsLogScale)
            {
                points = point2D.Y < 0 ? new PointF[] { new PointF(point2D.X, (float)-Math.Log10(-point2D.Y + 1)) } :
                    new PointF[] { new PointF(point2D.X, (float)Math.Log10(point2D.Y + 1)) };
            }
            else
            {
                points = new PointF[] { point2D };
            }
            this.matrixValueToScreen.TransformPoints(points);
            return points[0];
        }
        protected PointF GetScreenPointFromValuePoint(float x, float y)
        {
            PointF[] points;
            if (this.IsLogScale)
            {
                points = y < 0 ? new PointF[] { new PointF(x, (float)-Math.Log10(-y + 1)) } :
                    new PointF[] { new PointF(x, (float)Math.Log10(y + 1)) };
            }
            else
            {
                points = new PointF[] { new PointF(x, y) };
            }
            this.matrixValueToScreen.TransformPoints(points);
            return points[0];
        }
        public int IndexOf(DateTime date, int startIndex, int endIndex)
        {
            if (date < dateSerie[startIndex]) { return -1; }
            if (date > dateSerie[endIndex]) { return endIndex; }
            return IndexOfRec(date, startIndex + 1, endIndex);
        }
        private int IndexOfRec(DateTime date, int startIndex, int endIndex)
        {
            if (date <= dateSerie[startIndex]) { return startIndex; }
            if (date >= dateSerie[endIndex]) { return endIndex; }

            int midIndex = (startIndex + endIndex) / 2;
            switch (date.CompareTo(dateSerie[midIndex]))
            {
                case 0:
                    return midIndex;
                case -1:
                    return IndexOfRec(date, startIndex + 1, midIndex);
                case 1:
                    return IndexOfRec(date, midIndex, endIndex - 1);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
        #region MOUSE EVENTS

        public void MouseDateChanged(FullGraphUserControl sender, DateTime date, float value, bool crossMode)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                if (this.IsInitialized)
                {
                    int index = IndexOf(date, this.StartIndex, this.EndIndex);
                    if (index == -1) return;
                    PointF valuePoint = new PointF(index, value);
                    PointF mousePoint = GetScreenPointFromValuePoint(valuePoint);

                    if (mousePoint.X > this.GraphRectangle.Left && mousePoint.X < this.GraphRectangle.Right)
                    {
                        DrawMouseValueCross(valuePoint, crossMode, true, this.axisDashPen, false);
                        PaintForeground();
                    }
                }
            }
        }
        protected void RaiseDateChangedEvent(FullGraphUserControl sender, DateTime date, float value, bool crossMode)
        {
            if (this.OnMouseValueChanged != null)
            {
                this.OnMouseValueChanged(sender, date, value, crossMode);
            }
        }
        virtual public void MouseMoveOverControl(System.Windows.Forms.MouseEventArgs e, Keys key, bool mouseOverThis)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                if (this.IsInitialized)
                {
                    PointF mousePoint = new PointF(e.X, e.Y);
                    if ((key & Keys.Control) != 0 && this.DrawingMode == GraphDrawMode.Normal)
                    {
                        if (e.X > this.GraphRectangle.Left && e.X < this.GraphRectangle.Right)
                        {
                            var valuePoint = GetValuePointFromScreenPoint(mousePoint);
                            DrawMouseValueCross(valuePoint, mouseOverThis, true, this.axisPen, mouseOverThis);
                            PaintForeground();

                            if (mouseOverThis && this.OnMouseValueChanged != null)
                            {
                                int index = RoundToIndex(mousePoint);
                                this.OnMouseValueChanged(null, this.dateSerie[index], 0, true);
                            }
                        }
                        return;
                    }

                    if (!mouseOverThis) // Event come when the mouse moved over another control
                    {
                        if (e.X > this.GraphRectangle.Left && e.X < this.GraphRectangle.Right)
                        {
                            // Refresh the mouse marquee
                            int index = RoundToIndex(mousePoint);

                            // Display under mouse info
                            DrawMousePos(index);

                            lastMouseIndex = index;
                            PaintForeground();
                        }
                        return;
                    }
                    else  // The mouse is moving over this control
                    {
                        if (mouseDown)
                        {
                            DrawSelectionZone(e, key);
                        }
                        else
                        {
                            PointF mouseValuePoint = GetValuePointFromScreenPoint(mousePoint);
                            // Refresh the mouse marquee
                            int index = RoundToIndex(mousePoint);

                            // Display under mouse info
                            DrawMousePos(index, mouseValuePoint.Y);

                            lastMouseIndex = index;

                            if (this.OnMouseValueChanged != null)
                            {
                                this.OnMouseValueChanged(null, this.dateSerie[index], 0, false);
                            }
                        }
                    }
                    this.PaintForeground();
                }
            }
        }
        protected PointF RoundToIndexValue(PointF screenPoint2D)
        {
            PointF point = GetValuePointFromScreenPoint(screenPoint2D);
            int index = Math.Max(Math.Min((int)Math.Round(point.X), this.EndIndex), this.StartIndex);
            point = GetScreenPointFromValuePoint(index, point.Y);
            return point;
        }
        protected int RoundToIndex(PointF screenPoint2D)
        {
            PointF point = GetValuePointFromScreenPoint(screenPoint2D);
            int index = Math.Max(Math.Min((int)Math.Round(point.X), this.EndIndex), this.StartIndex);
            if (index >= dateSerie.Length)
            {
                index = dateSerie.Length - 1;
            }
            else if (index < 0)
            {
                index = 0;
            }
            return index;
        }
        virtual protected void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.GraphRectangle.Contains(e.Location))
            {
                mouseDown = true;
                mouseDownPos = e.Location;
            }
        }
        virtual protected void Form1_MouseLeave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.alternateString))
                Cursor.Show();
            this.PaintForeground();
            if (mouseDown)
            {
                mouseDown = false;
                forceNoValueBoxDisplay = false;
            }
        }
        virtual protected void Form1_MouseEnter(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.alternateString))
                Cursor.Hide();
            if (mouseDown)
            {
                forceNoValueBoxDisplay = false;
                mouseDown = false;
            }
        }
        virtual protected void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                forceNoValueBoxDisplay = false;
                mouseDown = false;
            }
        }
        virtual public void GraphControl_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                if (this.IsInitialized && !forceNoValueBoxDisplay)
                {
                    PointF mousePoint = new PointF(e.X, e.Y);
                    this.PaintDailyBox(mousePoint);
                    this.PaintForeground();
                }
            }
        }
        #endregion
        #region DRAWING FUNCTIONS
        protected virtual void DrawMousePos(int mouseIndex, float y = 0)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                if (this.IsInitialized)
                {
                    if (mouseIndex != -1 && CurveList.Count != 0 && this.mainSerie != null)
                    {
                        // GetMarquee value
                        float value = this.mainSerie[mouseIndex];
                        if (float.IsNaN(value)) return;
                        PointF point = new PointF(mouseIndex, y);
                        PointF point2 = GetScreenPointFromValuePoint(point);

                        DrawMouseValueCross(point, y != 0, true, this.axisDashPen, false);

                        string valueString;
                        if (value > 100000000)
                        {
                            valueString = (value / 1000000).ToString("0.#") + "M";
                        }
                        else
                            if (value > 1000000)
                        {
                            valueString = (value / 1000).ToString("0.#") + "K";
                        }
                        else
                        {
                            valueString = value.ToString("0.##");
                        }
                        this.DrawString(this.foregroundGraphic, valueString, axisFont, textBrush, backgroundBrush, new PointF(GraphRectangle.Right + 2, point2.Y - 8), true);
                    }
                }
            }
        }
        static float[] fibonacciRetracements = new float[] { 0.25f, 0.5f, 0.75f };
        protected void DrawSelectionZone(MouseEventArgs e, Keys key)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                int width = Math.Abs(e.Location.X - mouseDownPos.X);
                int height = Math.Abs(e.Location.Y - mouseDownPos.Y);
                int x = Math.Min(mouseDownPos.X, e.Location.X);
                int y = Math.Min(mouseDownPos.Y, e.Location.Y);

                // Retrieve the real values
                PointF initialValue = GetValuePointFromScreenPoint(mouseDownPos);
                PointF newValue = GetValuePointFromScreenPoint(e.Location);

                float diff = newValue.Y - initialValue.Y;
                float variation = diff / initialValue.Y;

                this.DrawHorizontalLine(mouseDownPos.Y, initialValue.Y, axisDashPen);
                this.DrawHorizontalLine(e.Location.Y, newValue.Y, axisDashPen);

                // Draw selection zone and Fibonacci retracements
                float fiboY = 0.0f;
                if (e.Location.Y < mouseDownPos.Y)
                {
                    // Draw selection zone
                    this.foregroundGraphic.FillRectangle(greenBrush, x, y, width, height);
                    this.foregroundGraphic.DrawRectangle(greenPen, x, y, width, height);

                    // Draw Fibonacci
                    if (height >= 30)
                    {
                        foreach (float fibo in fibonacciRetracements)
                        {
                            fiboY = y + height * fibo;
                            string fiboString = (1.0f - fibo).ToString("P2");
                            this.DrawString(foregroundGraphic, fiboString, axisFont, Brushes.Green, this.backgroundBrush, x - 36, fiboY - 5, false);
                            if (fibo == 0.5f)
                            {
                                greenPen.Width = 2;
                            }
                            else
                            {
                                greenPen.Width = 1;
                            }
                            this.foregroundGraphic.DrawLine(greenPen, x, fiboY, x + width, fiboY);
                        }
                    }
                }
                else
                {
                    this.foregroundGraphic.FillRectangle(redBrush, x, y, width, height);
                    this.foregroundGraphic.DrawRectangle(redPen, x, y, width, height);

                    // Draw Fibonacci
                    if (height >= 30)
                    {
                        foreach (float fibo in fibonacciRetracements)
                        {
                            fiboY = y + height * (1.0f - fibo);
                            string fiboString = (1.0f - fibo).ToString("P2");
                            this.foregroundGraphic.DrawString(fiboString, axisFont, Brushes.Red, x - 36, fiboY - 5);
                            if (fibo == 0.5f)
                            {
                                redPen.Width = 2;
                            }
                            else
                            {
                                redPen.Width = 1;
                            }
                            this.foregroundGraphic.DrawLine(redPen, x, fiboY, x + width, fiboY);
                        }
                    }
                }

                // Display tooltip
                string diffString = diff.ToString("0.####");
                this.DrawString(foregroundGraphic,
                    "Bars:\t" + ((int)(newValue.X - initialValue.X)).ToString() + Environment.NewLine +
                    "Var:\t" + variation.ToString("P2") + "     " + Environment.NewLine +
                    "Diff:\t" + diffString,
                    toolTipFont, Brushes.Black, this.backgroundBrush, new PointF(x + width + 4, y), true);

                // force the value box not to display.
                forceNoValueBoxDisplay = true;
            }
        }
        protected void DrawTmpItem(Graphics graph, DrawingItem item, bool useTransform)
        {
            // Calculate intersection with bounding rectangle
            Rectangle2D rect2D = new Rectangle2D(GraphRectangle);
            if (useTransform)
            {
                item.Draw(graph, this.matrixValueToScreen, rect2D, this.IsLogScale);
            }
            else
            {
                item.Draw(graph, GraphControl.matrixIdentity, rect2D, this.IsLogScale);
            }
        }
        protected void DrawTmpSegment(Graphics graph, Pen pen, PointF point1, PointF point2, bool useTransform)
        {
            // Calculate intersection with bounding rectangle
            Rectangle2D rect2D = new Rectangle2D(GraphRectangle);
            Segment2D newLine = new Segment2D(point1, point2);
            if (useTransform)
            {
                newLine.Draw(graph, pen, this.matrixValueToScreen, rect2D, this.IsLogScale);
            }
            else
            {
                newLine.Draw(graph, pen, GraphControl.matrixIdentity, rect2D, false);
            }
        }

        protected void DrawTmpHalfLine(Graphics graph, Pen pen, PointF point1, PointF point2, bool useTransform)
        {
            // Calculate intersection with bounding rectangle
            Rectangle2D rect2D = new Rectangle2D(GraphRectangle);
            HalfLine2D newLine = new HalfLine2D(point1, point2, null);
            if (useTransform)
            {
                newLine.Draw(graph, pen, this.matrixValueToScreen, rect2D, this.IsLogScale);
            }
            else
            {
                newLine.Draw(graph, pen, GraphControl.matrixIdentity, rect2D, false);
            }
        }
        protected void DrawVerticalGridLines(Graphics aGraphic, bool drawDate, int startIndex, int endIndex)
        {
            TimeSpan duration = this.dateSerie[endIndex] - this.dateSerie[startIndex];
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;
            PointF p1;
            int previousMonth = -1;
            int previousYear = -1;
            int previousWeek = -1;
            int previousDay = -1;
            int previousHour = -1;
            if (startIndex > 0)
            {
                previousMonth = this.dateSerie[startIndex - 1].Month;
                previousYear = this.dateSerie[startIndex - 1].Year;
                previousWeek = cal.GetWeekOfYear(this.dateSerie[startIndex - 1], dfi.CalendarWeekRule, DayOfWeek.Monday);
                previousDay = this.dateSerie[startIndex - 1].DayOfYear;
                previousHour = this.dateSerie[startIndex - 1].Hour;
            }
            if (duration.Days > 1000) // greater the 5 years, display years only 
            {
                for (int i = startIndex; i <= endIndex; i++)
                {
                    if (this.dateSerie[i].Year != previousYear)
                    {
                        p1 = GetScreenPointFromValuePoint(i, 100);
                        previousYear = this.dateSerie[i].Year;
                        if (drawDate)
                        {
                            aGraphic.DrawString(this.dateSerie[i].ToString("dd/MM"), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height);
                            aGraphic.DrawString(this.dateSerie[i].ToString("yyyy"), axisFont, Brushes.Black, p1.X - 11, GraphRectangle.Y + GraphRectangle.Height + 8);
                        }
                        aGraphic.DrawLine(gridPen, p1.X, GraphRectangle.Y, p1.X, GraphRectangle.Y + GraphRectangle.Height);
                    }
                }
            }
            else if (duration.Days > 100) // greater the 6 months, display Months only 
            {
                for (int i = startIndex; i <= endIndex; i++)
                {
                    if (this.dateSerie[i].Month != previousMonth)
                    {
                        p1 = GetScreenPointFromValuePoint(i, 100);
                        previousMonth = this.dateSerie[i].Month;
                        aGraphic.DrawLine(gridPen, p1.X, GraphRectangle.Y, p1.X, GraphRectangle.Y + GraphRectangle.Height);
                        if (this.dateSerie[i].Year != previousYear)
                        {
                            previousYear = this.dateSerie[i].Year;
                            if (drawDate)
                            {
                                aGraphic.DrawString(this.dateSerie[i].ToString("dd/MM"), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height);
                                aGraphic.DrawString(this.dateSerie[i].ToString("yyyy"), axisFont, Brushes.Black, p1.X - 11, GraphRectangle.Y + GraphRectangle.Height + 8);
                            }
                        }
                        else
                        {
                            if (drawDate)
                            {
                                aGraphic.DrawString(this.dateSerie[i].ToString("dd/MM"), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height);
                            }
                        }
                    }
                }
            }
            else if (duration.Days > 50) // Display only the first day of the week
            {
                int currentWeekNumber;
                for (int i = startIndex; i <= endIndex; i++)
                {
                    currentWeekNumber = cal.GetWeekOfYear(this.dateSerie[i], dfi.CalendarWeekRule, DayOfWeek.Monday);
                    if (currentWeekNumber != previousWeek)
                    {
                        p1 = GetScreenPointFromValuePoint(i, 100);
                        previousWeek = currentWeekNumber;
                        aGraphic.DrawLine(gridPen, p1.X, GraphRectangle.Y, p1.X, GraphRectangle.Y + GraphRectangle.Height);
                        if (this.dateSerie[i].Year != previousYear)
                        {
                            previousYear = this.dateSerie[i].Year;
                            if (drawDate)
                            {
                                aGraphic.DrawString(this.dateSerie[i].ToString("dd/MM"), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height);
                                aGraphic.DrawString(this.dateSerie[i].ToString("yyyy"), axisFont, Brushes.Black, p1.X - 11, GraphRectangle.Y + GraphRectangle.Height + 8);
                            }
                        }
                        else
                        {
                            if (drawDate)
                            {
                                aGraphic.DrawString(this.dateSerie[i].ToString("dd/MM"), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height);
                            }
                        }
                    }
                }
            }
            else if (this.dateSerie[StartIndex + 1] - this.dateSerie[StartIndex] >= new TimeSpan(1, 0, 0, 0)) // Display every day, but remains in daily time frame
            {
                for (int i = startIndex; i <= endIndex; i++)
                {
                    p1 = GetScreenPointFromValuePoint(i, 100);
                    aGraphic.DrawLine(gridPen, p1.X, GraphRectangle.Y, p1.X, GraphRectangle.Y + GraphRectangle.Height);
                    if (this.dateSerie[i].Year != previousYear)
                    {
                        previousYear = this.dateSerie[i].Year;
                        if (drawDate)
                        {
                            aGraphic.DrawString(this.dateSerie[i].ToString("dd/MM"), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height);
                            aGraphic.DrawString(this.dateSerie[i].ToString("yyyy"), axisFont, Brushes.Black, p1.X - 11, GraphRectangle.Y + GraphRectangle.Height + 8);
                        }
                    }
                    else
                    {
                        if (drawDate)
                        {
                            aGraphic.DrawString(this.dateSerie[i].ToString("dd/MM"), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height);
                        }
                    }
                }
            }
            else if (duration.Days > 5)
            {
                for (int i = startIndex; i <= endIndex; i++)
                {
                    p1 = GetScreenPointFromValuePoint(i, 100);
                    if (this.dateSerie[i].DayOfYear != previousDay)
                    {
                        previousDay = this.dateSerie[i].DayOfYear;
                        p1 = GetScreenPointFromValuePoint(i, 100);
                        aGraphic.DrawLine(gridPen, p1.X, GraphRectangle.Y, p1.X, GraphRectangle.Y + GraphRectangle.Height);
                        if (drawDate)
                        {
                            aGraphic.DrawString(this.dateSerie[i].ToShortTimeString(), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height);
                            aGraphic.DrawString(this.dateSerie[i].ToString("dd/MM"), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height + 8);
                        }
                    }
                }
            }
            else if (duration.Days > 2)
            {
                for (int i = startIndex; i <= endIndex; i++)
                {
                    if (this.dateSerie[i].DayOfYear != previousDay)
                    {
                        previousDay = this.dateSerie[i].DayOfYear;
                        p1 = GetScreenPointFromValuePoint(i, 100);
                        aGraphic.DrawLine(gridPen, p1.X, GraphRectangle.Y, p1.X, GraphRectangle.Y + GraphRectangle.Height);
                        if (drawDate)
                        {
                            aGraphic.DrawString(this.dateSerie[i].ToShortTimeString(), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height);
                            aGraphic.DrawString(this.dateSerie[i].ToString("dd/MM"), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height + 8);
                        }
                    }
                    else
                    {
                        if (this.dateSerie[i].Minute == 0)
                        {
                            p1 = GetScreenPointFromValuePoint(i, 100);
                            aGraphic.DrawLine(gridPen, p1.X, GraphRectangle.Y, p1.X, GraphRectangle.Y + GraphRectangle.Height);
                            if (drawDate)
                            {
                                aGraphic.DrawString(this.dateSerie[i].ToShortTimeString(), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height);
                            }
                        }
                    }
                }
            }
            else if (duration.Days >= 1 || endIndex - startIndex > 50)
            {
                int barCount = 0;
                for (int i = startIndex; i <= endIndex; i++)
                {
                    if (this.dateSerie[i].DayOfYear != previousDay)
                    {
                        previousDay = this.dateSerie[i].DayOfYear;
                        p1 = GetScreenPointFromValuePoint(i, 100);
                        aGraphic.DrawLine(gridPen, p1.X, GraphRectangle.Y, p1.X, GraphRectangle.Y + GraphRectangle.Height);
                        if (drawDate)
                        {
                            aGraphic.DrawString(this.dateSerie[i].ToShortTimeString(), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height);
                            aGraphic.DrawString(this.dateSerie[i].ToString("dd/MM"), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height + 8);
                        }
                        barCount = 0;
                    }
                    else
                    {
                        if (this.dateSerie[i].Minute == 0 && barCount >= 50)
                        {
                            p1 = GetScreenPointFromValuePoint(i, 100);
                            aGraphic.DrawLine(gridPen, p1.X, GraphRectangle.Y, p1.X, GraphRectangle.Y + GraphRectangle.Height);
                            if (drawDate)
                            {
                                aGraphic.DrawString(this.dateSerie[i].ToShortTimeString(), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height);
                            }
                            barCount = 0;
                        }
                        barCount++;
                    }
                }
            }
            else
            {
                for (int i = startIndex; i <= endIndex; i++)
                {
                    p1 = GetScreenPointFromValuePoint(i, 100);
                    if (this.dateSerie[i].DayOfYear != previousDay)
                    {
                        previousDay = this.dateSerie[i].DayOfYear;
                        aGraphic.DrawLine(gridPen, p1.X, GraphRectangle.Y, p1.X, GraphRectangle.Y + GraphRectangle.Height);
                        if (drawDate)
                        {
                            aGraphic.DrawString(this.dateSerie[i].ToShortTimeString(), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height);
                            aGraphic.DrawString(this.dateSerie[i].ToString("dd/MM"), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height + 8);
                        }
                    }
                    else
                    {
                        if (this.dateSerie[i].Minute % 5 == 0)
                        {
                            aGraphic.DrawLine(gridPen, p1.X, GraphRectangle.Y, p1.X, GraphRectangle.Y + GraphRectangle.Height);
                            if (drawDate)
                            {
                                aGraphic.DrawString(this.dateSerie[i].ToShortTimeString(), axisFont, Brushes.Black, p1.X - 13, GraphRectangle.Y + GraphRectangle.Height);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Draw a string in the graphic. Return the right position of the string box
        /// </summary>
        /// <param name="aGraphic"></param>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="brush"></param>
        /// <param name="backgroundBrush"></param>
        /// <param name="location"></param>
        /// <param name="drawFrame"></param>
        /// <returns></returns>
        protected float DrawString(Graphics aGraphic, string text, Font font, Brush brush, Brush backgroundBrush, PointF location, bool drawFrame, Pen pen = null)
        {
            string trimmedText = text.Trim();
            Size size = TextRenderer.MeasureText(trimmedText, font);
            pen ??= textFramePen;

            RectangleF rect = new RectangleF(location.X, location.Y, size.Width, size.Height);
            if (drawFrame)
            {
                aGraphic.FillRectangle(backgroundBrush, location.X - 1, location.Y - 1, size.Width - 6, size.Height + 1);
                aGraphic.DrawRectangle(pen, location.X - 1, location.Y - 1, size.Width - 6, size.Height + 1);
            }
            aGraphic.DrawString(trimmedText, font, brush, rect);
            return location.X + size.Width - 26;
        }
        protected float DrawString(Graphics aGraphic, string text, Font font, Brush brush, Brush backgroundBrush, float x, float y, bool drawFrame)
        {
            string trimmedText = text.Trim();
            Size size = TextRenderer.MeasureText(trimmedText, font);

            RectangleF rect = new RectangleF(x, y, size.Width, size.Height);
            if (drawFrame)
            {
                aGraphic.FillRectangle(textBackgroundBrush, x - 1, y - 1, size.Width - 6, size.Height + 1);
                aGraphic.DrawRectangle(textFramePen, x - 1, y - 1, size.Width - 6, size.Height + 1);
            }
            aGraphic.DrawString(trimmedText, font, brush, rect);
            return x + size.Width - 26;
        }
        protected void DrawHorizontalLine(int y, float value, Pen pen)
        {
            this.foregroundGraphic.DrawLine(pen, GraphRectangle.Left, y, GraphRectangle.Right, y);
            // Print current value
            this.DrawString(this.foregroundGraphic, value.ToString("0.####"), axisFont, textBrush, backgroundBrush, new PointF(GraphRectangle.Right + 2, y - 8), true);

        }
        protected void DrawMouseValueCross(PointF mouseValuePoint, bool drawHorizontalLine, bool drawVerticalLine, Pen pen, bool printValue)
        {
            // Draw straight Line
            PointF screenPoint = RoundToIndexValue(GetScreenPointFromValuePoint(mouseValuePoint));
            if (drawHorizontalLine)
            {
                this.foregroundGraphic.DrawLine(pen, GraphRectangle.Left, screenPoint.Y, GraphRectangle.Right, screenPoint.Y);
                // Print current value
                if (printValue)
                    this.DrawString(this.foregroundGraphic, mouseValuePoint.Y.ToString("0.####"), axisFont, textBrush, backgroundBrush, new PointF(GraphRectangle.Right + 2, screenPoint.Y - 8), true);
            }
            if (drawVerticalLine)
            {
                this.foregroundGraphic.DrawLine(pen, screenPoint.X, GraphRectangle.Bottom, screenPoint.X, GraphRectangle.Top);
            }
        }
        protected void DrawMouseCross(PointF mousePoint, bool drawHorizontalLine, bool drawVerticalLine, Pen pen)
        {
            // Draw straight Line
            if (drawHorizontalLine)
            {
                this.foregroundGraphic.DrawLine(pen, GraphRectangle.Left, mousePoint.Y, GraphRectangle.Right, mousePoint.Y);
            }
            if (drawVerticalLine)
            {
                this.foregroundGraphic.DrawLine(pen, mousePoint.X, GraphRectangle.Bottom, mousePoint.X, GraphRectangle.Top);
            }
        }
        public void DrawMouseCursor(Point mousePoint)
        {
            if (this.foregroundGraphic != null)
            {
                this.foregroundGraphic.DrawLine(MouseCursorPen, mousePoint.X - 8, mousePoint.Y, mousePoint.X + 8, mousePoint.Y);
                this.foregroundGraphic.DrawLine(MouseCursorPen, mousePoint.X, mousePoint.Y - 8, mousePoint.X, mousePoint.Y + 8);
                this.PaintForeground();
            }
        }
        #endregion
        #region UNDO BUFFER MANAGEMENT
        public void Undo()
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                if (currentActionIndex >= 0)
                {
                    GraphAction action = this.GraphActions.ElementAt(this.currentActionIndex);
                    switch (action.ActionType)
                    {
                        case GraphActionType.AddItem:
                            this.drawingItems.Remove(action.TargetItem);
                            break;
                        case GraphActionType.DeleteItem:
                            this.drawingItems.Add(action.TargetItem);
                            break;
                        case GraphActionType.CutItem:
                            this.drawingItems.Add(action.TargetItem);
                            this.drawingItems.Remove(action.TargetItem2);
                            break;
                        default:
                            break;
                    }
                    currentActionIndex--;

                    // Redraw
                    BackgroundDirty = true; // The new line becomes a part of the background
                }
            }
        }
        public void Redo()
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                if (currentActionIndex < (this.GraphActions.Count - 1))
                {
                    currentActionIndex++;
                    GraphAction action = this.GraphActions.ElementAt(this.currentActionIndex);
                    switch (action.ActionType)
                    {
                        case GraphActionType.AddItem:
                            this.drawingItems.Add(action.TargetItem);
                            break;
                        case GraphActionType.DeleteItem:
                            this.drawingItems.Remove(action.TargetItem);
                            break;
                        case GraphActionType.CutItem:
                            this.drawingItems.Remove(action.TargetItem);
                            this.drawingItems.Add(action.TargetItem2);
                            break;
                        default:
                            break;
                    }

                    // Redraw
                    BackgroundDirty = true; // The new line becomes a part of the background
                }
            }
        }
        protected void AddToUndoBuffer(GraphActionType actionType, DrawingItem newLine)
        {
            if (this.currentActionIndex != (this.GraphActions.Count - 1)) // Some undo occured before
            {
                // Delete all that has been undone
                this.GraphActions = this.GraphActions.Take(this.currentActionIndex + 1).ToList();
            }
            this.GraphActions.Add(new GraphAction(actionType, newLine));
            this.currentActionIndex++;
        }
        protected void AddToUndoBuffer(GraphActionType actionType, DrawingItem cutItem, DrawingItem newItem)
        {
            if (this.currentActionIndex != (this.GraphActions.Count - 1)) // Some undo occured before
            {
                // Delete all that has been undid
                this.GraphActions = this.GraphActions.Take(this.currentActionIndex + 1).ToList();
            }
            this.GraphActions.Add(new GraphAction(actionType, cutItem, newItem));
            this.currentActionIndex++;
        }
        #endregion
        public void EraseAllDrawingItems()
        {
            if (this.drawingItems != null)
            {
                foreach (DrawingItem drawingItem in this.drawingItems.Where(di => di.IsPersistent))
                {
                    AddToUndoBuffer(GraphActionType.DeleteItem, drawingItem);

                }
                this.drawingItems.RemoveAll(di => di.IsPersistent);
            }
        }
        protected string BuildTabbedString(string type, float value, int tabLocation)
        {
            string valueString;
            if (value > 10000000)
            {
                valueString = (value / 1000000).ToString("0.###") + "M";
            }
            else
                if (value > 100000)
            {
                valueString = (value / 1000).ToString("0.###") + "K";
            }
            if (value > 10)
            {
                valueString = value.ToString("0.##");
            }
            else
            {
                valueString = value.ToString("0.####");
            }
            return (type + ":").PadRight(tabLocation) + valueString;
        }
        protected string BuildTabbedString(string type, string value, int tabLocation)
        {
            return (type + ":").PadRight(tabLocation) + value;
        }
        protected int selectedLineIndex = -1;
        public void ResetDrawingMode()
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                this.DrawingMode = GraphDrawMode.Normal;
                this.DrawingStep = GraphDrawingStep.Done;
                selectedLineIndex = -1;
                this.ForegroundDirty = true;
            }
        }
    }
}