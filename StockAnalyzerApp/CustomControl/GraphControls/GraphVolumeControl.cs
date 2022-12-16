using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockLogging;

namespace StockAnalyzerApp.CustomControl.GraphControls
{
    partial class GraphVolumeControl : GraphControl
    {
        public GraphVolumeControl()
        {
        }
        override protected bool InitializeTransformMatrix()
        {
            if (this.CurveList == null)
            {
                this.IsInitialized = false;
                InvalidSerieException e = new InvalidSerieException("No data to display...");
                StockLog.Write(e);
                throw e;
            }
            if (this.GraphRectangle.Height > 0)
            {
                minValue = float.MaxValue;
                maxValue = float.MinValue;
                this.CurveList.GetMinMax(StartIndex, EndIndex, ref minValue, ref maxValue, this.ScaleInvisible);

                if (maxValue == 0.0f || float.IsNaN(minValue) || float.IsInfinity(minValue) || float.IsNaN(maxValue) || float.IsInfinity(maxValue))
                {
                    this.Deactivate("No volume for this stock", false);
                    return false;
                }
                if (graphic == null)
                {
                    // Initialise graphics
                    this.graphic = this.CreateGraphics();
                    RectangleF rect = this.graphic.VisibleClipBounds;
                    rect.Inflate(new SizeF(-this.XMargin, -this.YMargin));
                    this.GraphRectangle = rect;
                }

                float coefX = (this.GraphRectangle.Width * 0.96f) / (EndIndex - StartIndex);
                float coefY = this.GraphRectangle.Height * 0.96f / maxValue;

                matrixValueToScreen = new System.Drawing.Drawing2D.Matrix();
                matrixValueToScreen.Translate(this.GraphRectangle.X - (StartIndex - 0.5f) * coefX, maxValue * coefY + this.GraphRectangle.Y);
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
        override protected void PaintTmpGraph(Graphics aGraphic)
        {
            #region Draw vertical lines
            DrawVerticalGridLines(aGraphic, false, this.StartIndex, this.EndIndex);
            #endregion

            // Get last value
            this.mainSerie = CurveList.Find(c => c.DataSerie.Name == "EXCHANGED").DataSerie;

            float lastValue = this.mainSerie[EndIndex];
            string lastValueString = string.Empty;
            if (lastValue > 1000000000)
            {
                lastValueString += (lastValue / 1000000000).ToString("0.##") + "G€";
            }
            else
            if (lastValue > 1000000)
            {
                lastValueString += (lastValue / 1000000).ToString("0.##") + "M€";
            }
            else if (lastValue > 1000)
            {
                lastValueString += (lastValue / 1000).ToString("0.##") + "K€";
            }
            else
            {
                lastValueString += lastValue.ToString("0.##") + "€";
            }

            aGraphic.DrawString(lastValueString, axisFont, Brushes.Black, GraphRectangle.Right + 1, GraphRectangle.Top + 8);

            float minValue = float.MaxValue, maxValue = float.MinValue;
            CurveList.GetMinMax(StartIndex, EndIndex, ref minValue, ref maxValue, false);
            if (maxValue == 0)
            {
                return;
            }
            GraphCurveType closeCurveType = CurveList.Find(c => c.DataSerie.Name == "CLOSE");
            PointF[] points = null;
            foreach (GraphCurveType currentCurveType in CurveList)
            {
                if (!currentCurveType.IsVisible)
                {
                    continue;
                }
                points = GetScreenPoints(StartIndex, EndIndex, currentCurveType.DataSerie);

                int startValueIndex = currentCurveType.DataSerie.Count - points.Count();
                if (points.Count() > 1)
                {
                    if (currentCurveType.DataSerie.Name == "EXCHANGED")
                    {
                        float barWidth = Math.Max(1f, 0.80f * GraphRectangle.Width / (float)points.Count());

                        // Draw bar chart
                        int i = 0;
                        float previousClose = 0;
                        float currentClose = 0;
                        foreach (PointF point in points)
                        {
                            if (!float.IsNaN(point.X) && !float.IsNaN(point.Y))
                            {
                                currentClose = closeCurveType.DataSerie[i + StartIndex];

                                // Select brush color
                                Brush currentBrush = Brushes.Red;
                                if (currentClose > previousClose)
                                {
                                    currentBrush = Brushes.Green;
                                }
                                previousClose = currentClose;

                                if (i == 0)
                                {
                                    aGraphic.FillRectangle(currentBrush,
                                        point.X, point.Y,
                                        barWidth / 2, GraphRectangle.Bottom - point.Y);
                                }
                                else if (i == points.Count() - 1)
                                {
                                    aGraphic.FillRectangle(currentBrush,
                                        point.X - barWidth / 2, point.Y,
                                        barWidth / 2, GraphRectangle.Bottom - point.Y);

                                }
                                else
                                {
                                    aGraphic.FillRectangle(currentBrush,
                                        point.X - barWidth / 2, point.Y,
                                        barWidth, GraphRectangle.Bottom - point.Y);
                                }
                                i++;
                            }
                        }
                    }
                    else
                    {
                        // Draw curve
                        aGraphic.DrawLines(currentCurveType.CurvePen, points);
                    }
                }
            }
            aGraphic.DrawRectangle(framePen, GraphRectangle.X, GraphRectangle.Y, GraphRectangle.Width, GraphRectangle.Height - 1);
        }
        override protected void PaintDailyBox(PointF mousePoint)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                string value = string.Empty;
                foreach (GraphCurveType curveType in this.CurveList)
                {
                    if (curveType.IsVisible && !float.IsNaN(curveType.DataSerie[this.lastMouseIndex]))
                    {
                        if (curveType.DataSerie.Name == "EXCHANGED")
                        {
                            var volume = this.serie.GetSerie(StockAnalyzer.StockClasses.StockDataType.VOLUME)[this.lastMouseIndex];
                            var exchanged = curveType.DataSerie[this.lastMouseIndex];
                            value += BuildTabbedString(curveType.DataSerie.Name, volume, 12) + "\r\n";
                            value += BuildTabbedString(curveType.DataSerie.Name + " €", exchanged, 12) + "\r\n";
                        }
                        else
                        {
                            value += BuildTabbedString(curveType.DataSerie.Name, curveType.DataSerie[this.lastMouseIndex], 12) + "\r\n";
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

    }
}
