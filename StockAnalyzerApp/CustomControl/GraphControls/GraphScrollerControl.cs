using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockLogging;
using StockAnalyzerApp.Properties;

namespace StockAnalyzerApp.CustomControl.GraphControls
{
    partial class GraphScrollerControl : GraphControl
    {
        public event OnZoomChangedHandler ZoomChanged;

        public Bitmap leftScrollBitmap;
        public Bitmap rightScrollBitmap;
        public Brush outOfScopeBrush;

        public GraphScrollerControl()
        {
            leftScrollBitmap = Resources.ScrollLeft;
            leftScrollBitmap.MakeTransparent();
            rightScrollBitmap = Resources.ScrollRight;
            rightScrollBitmap.MakeTransparent();
            outOfScopeBrush = new SolidBrush(Color.FromArgb(76, Color.Black));
        }

        protected override bool InitializeTransformMatrix()
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                if (!CheckGraphSanity()) { return false; }
                if (this.GraphRectangle.Height > 0)
                {
                    minValue = float.MaxValue;
                    maxValue = float.MinValue;
                    this.CurveList.GetMinMax(0, dateSerie.Length - 1, ref minValue, ref maxValue, this.ScaleInvisible);

                    if (minValue == maxValue || minValue == float.MaxValue || float.IsNaN(minValue) || float.IsInfinity(minValue) || maxValue == float.MinValue || float.IsNaN(maxValue) || float.IsInfinity(maxValue))
                    {
                        this.Deactivate("Input data is corrupted and cannot be displayed...", false);
                        return false;
                    }

                    if (this.IsLogScale && minValue > 0)
                    {
                        minValue -= (maxValue - minValue) * 0.025f;
                    }
                    else
                    {
                        minValue -= (maxValue - minValue) * 0.05f;
                    }
                    maxValue += (maxValue - minValue) * 0.05f;

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

                    float coefX = (this.GraphRectangle.Width * 0.94f) / (dateSerie.Length - 1);
                    float coefY = this.GraphRectangle.Height / (tmpMaxValue - tmpMinValue);

                    matrixValueToScreen = new System.Drawing.Drawing2D.Matrix();
                    matrixValueToScreen.Translate(this.GraphRectangle.X + 20, tmpMaxValue * coefY + this.GraphRectangle.Y);
                    matrixValueToScreen.Scale(coefX, -coefY);

                    matrixScreenToValue = (System.Drawing.Drawing2D.Matrix)matrixValueToScreen.Clone();
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

        protected override void PaintGraph()
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                if (this.IsInitialized && this.graphic != null)
                {
                    //if (ForceNoRepaint) // Skip one go due to Windows crap refresh management
                    //{
                    //    ForceNoRepaint = false;
                    //    return;
                    //}
                    // Draw the graph on the background image
                    if (BackgroundDirty)
                    {
                        // Create Bitmap graphicMain
                        backgroundBitmap = new Bitmap((int)this.graphic.VisibleClipBounds.Width, (int)this.graphic.VisibleClipBounds.Height, this.graphic);
                        Graphics tmpGraph = Graphics.FromImage(backgroundBitmap);

                        tmpGraph.Clear(this.BackgroundColor);
                        PaintTmpGraph(tmpGraph);
                        PaintGraphTitle(tmpGraph);

                        // Draw background image
                        this.graphic.DrawImage(backgroundBitmap, 0, 0);

                        this.ForegroundDirty = true;
                        this.BackgroundDirty = false;
                    }
                    else
                    {
                        // Draw background image
                        this.graphic.DrawImage(backgroundBitmap, 0, 0);
                    }
                }
                else // Draw alternate text.
                {
                    Graphics gr = this.CreateGraphics();
                    gr.Clear(SystemColors.ControlDark);
                    gr.DrawString(this.alternateString, axisFont, Brushes.Black, 10, 10);
                }
            }
        }

        protected override void PaintTmpGraph(Graphics aGraphic)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                if (this.matrixValueToScreen == null)
                    return;
                #region Draw vertical lines
                if (this.ShowGrid && this.IsInitialized)
                {
                    DrawVerticalGridLines(aGraphic, false, 0, dateSerie.Length - 1);
                }
                #endregion

                int i = 0;
                Rectangle2D rect2D = new Rectangle2D(GraphRectangle);
                PointF[] points = null;
                foreach (GraphCurveType currentCurveType in CurveList)
                {
                    if (currentCurveType.IsVisible)
                    {
                        points = GetScreenPoints(0, dateSerie.Length - 1, currentCurveType.DataSerie);
                        if (points != null && points.Count() > 1)
                        {
                            aGraphic.DrawLines(currentCurveType.CurvePen, points);
                        }
                    }
                    i++;
                }

                this.DrawSliders(aGraphic);

                aGraphic.DrawRectangle(framePen, GraphRectangle.X, GraphRectangle.Y, GraphRectangle.Width, GraphRectangle.Height);
            }
        }

        protected override void PaintGraphTitle(Graphics aGraphic)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                string graphTitle = "PREVIEW ";
                this.DrawString(aGraphic, graphTitle, this.axisFont, Brushes.Black, this.textBackgroundBrush, new PointF(1, 1), true);
            }
        }
        private void DrawSliders(Graphics aGraphic)
        {
            if (aGraphic != null)
            {
                PointF startPoint = GetScreenPointFromValuePoint(this.StartIndex, this.GraphRectangle.Top);
                aGraphic.FillRectangle(outOfScopeBrush, this.GraphRectangle.Left, this.GraphRectangle.Top, startPoint.X - this.GraphRectangle.Left, this.GraphRectangle.Height);
                aGraphic.DrawLine(Pens.Gray, startPoint.X, this.GraphRectangle.Top, startPoint.X, this.GraphRectangle.Bottom);
                aGraphic.DrawImage(this.leftScrollBitmap, startPoint.X - tolerance, this.GraphRectangle.Top + (this.GraphRectangle.Height - this.leftScrollBitmap.Height) / 2);
                PointF endPoint = GetScreenPointFromValuePoint(this.EndIndex, this.GraphRectangle.Top);
                aGraphic.FillRectangle(outOfScopeBrush, endPoint.X, this.GraphRectangle.Top, this.GraphRectangle.Right - endPoint.X, this.GraphRectangle.Height);
                aGraphic.DrawLine(Pens.Gray, endPoint.X, this.GraphRectangle.Top, endPoint.X, this.GraphRectangle.Bottom);
                aGraphic.DrawImage(this.rightScrollBitmap, endPoint.X - tolerance, this.GraphRectangle.Top + (this.GraphRectangle.Height - this.rightScrollBitmap.Height) / 2);
            }
        }

        protected int GetMouseIndex(PointF screenPoint2D)
        {
            PointF[] points = new PointF[] { screenPoint2D };
            this.matrixScreenToValue.TransformPoints(points);
            int index = Math.Max(Math.Min((int)Math.Round(points[0].X), this.dateSerie.Length - 1), 0);
            return index;
        }
        #region MOUSE EVENTS
        private bool holdingStartSlider = false;
        private bool holdingEndSlider = false;
        private bool holdingViewWindow = false;
        private float tolerance = Resources.ScrollLeft.Width / 2;

        override protected void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.GraphRectangle.Contains(e.Location))
            {
                mouseDown = true;
                mouseDownPos = e.Location;
                PointF screenStartIndex = this.GetScreenPointFromValuePoint(this.StartIndex, 0);
                PointF screenEndIndex = this.GetScreenPointFromValuePoint(this.EndIndex, 0);
                if ((screenStartIndex.X - tolerance) <= mouseDownPos.X && (screenStartIndex.X + tolerance) >= mouseDownPos.X)
                {
                    holdingStartSlider = true;
                    holdingEndSlider = false;
                    holdingViewWindow = false;
                    this.Cursor = Cursors.Hand;
                }
                else if ((screenEndIndex.X - tolerance) <= mouseDownPos.X && (screenEndIndex.X + tolerance) >= mouseDownPos.X)
                {
                    holdingStartSlider = false;
                    holdingEndSlider = true;
                    holdingViewWindow = false;
                    this.Cursor = Cursors.Hand;
                }
                else if ((screenStartIndex.X - tolerance) <= mouseDownPos.X && (screenEndIndex.X + tolerance) >= mouseDownPos.X)
                {
                    holdingStartSlider = false;
                    holdingEndSlider = false;
                    holdingViewWindow = true;
                    lastMouseIndex = GetMouseIndex(mouseDownPos);
                    this.Cursor = Cursors.SizeWE;
                }
            }
        }
        override protected void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                mouseDown = false;

                holdingStartSlider = false;
                holdingEndSlider = false;
                holdingViewWindow = false;
                this.Cursor = Cursors.Arrow;

                this.BackgroundDirty = true;
                if (this.ZoomChanged != null)
                {
                    this.ZoomChanged(this.StartIndex, this.EndIndex);
                }
            }
        }
        override public void MouseMoveOverControl(System.Windows.Forms.MouseEventArgs e, Keys key, bool mouseOverThis)
        {
            if (!this.IsInitialized || this.curveList == null)
            {
                return;
            }
            if (mouseOverThis)
            {
                if (mouseDown)
                {
                    int currentMouseIndex = GetMouseIndex(e.Location);
                    bool dirty = false;
                    if (currentMouseIndex != lastMouseIndex)
                    {
                        if (holdingStartSlider)
                        {
                            if (this.StartIndex != currentMouseIndex && (this.EndIndex - currentMouseIndex) > 30)
                            {
                                dirty = true;
                                this.StartIndex = currentMouseIndex;
                            }
                        }
                        else if (holdingEndSlider)
                        {
                            if (this.EndIndex != currentMouseIndex && (currentMouseIndex - this.StartIndex) > 30)
                            {
                                dirty = true;
                                this.EndIndex = currentMouseIndex;
                            }
                        }
                        else if (holdingViewWindow)
                        {
                            this.StartIndex = Math.Max(0, this.StartIndex - lastMouseIndex + currentMouseIndex);
                            this.EndIndex = Math.Min(this.dateSerie.Length - 1, this.EndIndex - lastMouseIndex + currentMouseIndex);
                            dirty = true;
                        }
                        if (dirty)
                        {
                            lastMouseIndex = currentMouseIndex;
                            this.DrawSliders(this.foregroundGraphic);
                            this.PaintForeground();
                        }
                    }
                }
                else
                {
                    PointF screenStartIndex = this.GetScreenPointFromValuePoint(this.StartIndex, 0);
                    PointF screenEndIndex = this.GetScreenPointFromValuePoint(this.EndIndex, 0);
                    if ((screenStartIndex.X - tolerance) <= e.Location.X && (screenStartIndex.X + tolerance) >= e.Location.X)
                    {
                        this.Cursor = Cursors.Hand;
                    }
                    else if ((screenEndIndex.X - tolerance) <= e.Location.X && (screenEndIndex.X + tolerance) >= e.Location.X)
                    {
                        this.Cursor = Cursors.Hand;
                    }
                    else if ((screenStartIndex.X - tolerance) <= e.Location.X && (screenEndIndex.X + tolerance) >= e.Location.X)
                    {
                        this.Cursor = Cursors.SizeWE;
                    }
                    else
                    {
                        this.Cursor = Cursors.Arrow;
                    }
                }
            }
        }
        override public void GraphControl_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!this.IsInitialized || this.curveList == null)
            {
                return;
            }

        }
        public void InitZoom(int startIndex, int endIndex)
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

                if (this.ZoomChanged != null)
                {
                    this.ZoomChanged(startIndex, endIndex);
                }
            }
        }
        #endregion
    }
}
