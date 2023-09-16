namespace StockAnalyzerApp.CustomControl.GraphControls
{
    partial class GraphRangedControl : GraphControl
    {
        public float RangeMin { get; set; }
        public float RangeMax { get; set; }

        public GraphRangedControl()
        {
        }
        override protected bool InitializeTransformMatrix()
        {
            if (float.IsNaN(this.RangeMin) || float.IsNaN(this.RangeMax))
            {
                return base.InitializeTransformMatrix();
            }

            if (this.CurveList == null)
            {
                this.IsInitialized = false;
                InvalidSerieException e = new InvalidSerieException("No data to display...");
                throw e;
            }
            if (this.CurveList.GetNbVisible() == 0)
            {
                this.Deactivate("No data to display...", false);
                return false;
            }
            if (this.StartIndex == this.EndIndex || this.EndIndex > this.dateSerie.Length - 1)
            {
                this.IsInitialized = false;
                InvalidSerieException e = new InvalidSerieException("Invalid input data range...");
                throw e;
            }
            if (this.GraphRectangle.Height > 0)
            {
                float minValue = this.RangeMin, maxValue = this.RangeMax;
                float coefX = (this.GraphRectangle.Width - this.XMargin) / (EndIndex - StartIndex + 1);
                float coefY = this.GraphRectangle.Height * 0.90f / (maxValue - minValue);

                matrixValueToScreen = new System.Drawing.Drawing2D.Matrix();
                matrixValueToScreen.Translate(this.GraphRectangle.X - (StartIndex - 0.5f) * coefX, maxValue * coefY + this.GraphRectangle.Y);
                matrixValueToScreen.Scale(coefX, -coefY);

                matrixScreenToValue = (System.Drawing.Drawing2D.Matrix)matrixValueToScreen.Clone();
                matrixScreenToValue.Invert();
                return true;
            }
            return false;
        }

    }
}
