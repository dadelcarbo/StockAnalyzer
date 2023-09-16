using System.Drawing;

namespace StockAnalyzer.StockDrawing
{
    public class OHLCBar
    {
        public float Width { get; set; }
        public float X { get; set; }
        public float Open { get; set; }
        public float High { get; set; }
        public float Low { get; set; }
        public float Close { get; set; }
        public OHLCBar()
        {
            this.Width = 5.0f;
            this.X = 0.0f;
            this.Open = 0.0f;
            this.High = 0.0f;
            this.Low = 0.0f;
            this.Close = 0.0f;
        }
        public OHLCBar(float x, float open, float high, float low, float close)
        {
            this.Width = 5.0f;
            this.X = x;
            this.Open = open;
            this.High = high;
            this.Low = low;
            this.Close = close;
        }
        public void Draw(Graphics graphic, Pen pen)
        {
            graphic.DrawLine(pen, X - Width, Open, X, Open);
            graphic.DrawLine(pen, X, Low, X, High);
            graphic.DrawLine(pen, X, Close, X + Width, Close);
        }
    }
}
