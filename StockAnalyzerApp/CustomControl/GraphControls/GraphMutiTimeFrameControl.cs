using System.Collections.Generic;
using System.Drawing;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;

namespace StockAnalyzerApp.CustomControl.GraphControls
{
    partial class GraphMutiTimeFrameControl : GraphControl
    {
        private List<BoolSerie> EventSeries { get; set; }

        public GraphMutiTimeFrameControl()
        {
            EventSeries = new List<BoolSerie>();
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
                EventSeries.Clear();

                // Create fake Event Series;
                for (int i = 0; i < 5; i++)
                {
                    EventSeries.Add(new BoolSerie(this.EndIndex, "Test" + i, i % 2 == 0));
                }

                minValue = 0.0f;
                maxValue = EventSeries.Count + 1;

                if (graphic == null)
                {
                    // Initialise graphics
                    this.graphic = this.CreateGraphics();
                    RectangleF rect = this.graphic.VisibleClipBounds;
                    rect.Inflate(new SizeF(-this.XMargin, -this.YMargin));
                    this.GraphRectangle = rect;
                }

                float coefX = (this.GraphRectangle.Width - this.XMargin) / (EndIndex - StartIndex + 1);
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

            int lineCount = EventSeries.Count;

            //
            for (int i = 0; i < lineCount; i++)
            {
                for (int j = StartIndex; j < EndIndex; j++)
                {
                    PointF point = this.GetScreenPointFromValuePoint(j, i);
                    if (EventSeries[i][j])
                    {

                        aGraphic.FillEllipse(Brushes.Green, point.X, point.Y, 6, 6);
                    }
                    else
                    {


                        aGraphic.FillEllipse(Brushes.Red, point.X, point.Y, 6, 6);
                    }
                }
            }

        }
    }
}
