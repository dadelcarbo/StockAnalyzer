using System;
using System.Drawing;
using System.Linq;
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

            if (minValue == maxValue || float.IsNaN(minValue) || float.IsInfinity(minValue) || float.IsNaN(maxValue) || float.IsInfinity(maxValue))
            {
               this.Deactivate("No volume for this stock", true);
               return true;
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
            float coefY = this.GraphRectangle.Height / (maxValue - minValue);

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
         this.mainSerie = CurveList.Find(c => c.DataSerie.Name == "VOLUME").DataSerie;

         float lastValue = this.mainSerie.Last;
         string lastValueString;
         if (lastValue > 100000000)
         {
            lastValueString = (lastValue / 1000000).ToString("0.#") + "M";
         }
         else if (lastValue > 1000000)
         {
            lastValueString = (lastValue / 1000).ToString("0.#") + "K";
         }
         else
         {
            lastValueString = lastValue.ToString("0.##");
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
               if (currentCurveType.DataSerie.Name == "VOLUME")
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
   }
}
