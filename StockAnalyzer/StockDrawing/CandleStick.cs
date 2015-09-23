using System.Drawing;

namespace StockAnalyzer.StockDrawing
{
    public class CandleStick
    {
        public float Width { get; set; }
        public float X { get; set; }
        public float Open { get; set; }
        public float High { get; set; }
        public float Low { get; set; }
        public float Close { get; set; }
        public CandleStick()
        {
            Width = 5.0f;
            X = 0.0f;
            Open = 0.0f;
            High = 0.0f;
            Low = 0.0f;
            Close = 0.0f;
        }
        public CandleStick(float x, float open, float high, float low, float close)
        {
            Width = 5.0f;
            X = x;
            Open = open;
            High = high;
            Low = low;
            Close = close;
        }
        public void Draw(Graphics graphic, Pen pen, Brush paintBrush)
        {
            Brush candleBrush;
            graphic.DrawLine(pen, X, Low, X, High);
            if (Close == Open)
            {
                graphic.DrawLine(pen, X - Width, Close, X + Width, Close);
            }
            else
            {
                if (Close > Open)
                {
                    candleBrush = paintBrush == null ? Brushes.Red : paintBrush;
                    graphic.FillRectangle(candleBrush, X - Width, Open, 2 * Width, Close - Open);
                    graphic.DrawRectangle(pen, X - Width, Open, 2 * Width, Close - Open);
                }
                else
                {
                    candleBrush = paintBrush == null ? Brushes.Green : paintBrush;
                    graphic.FillRectangle(candleBrush, X - Width, Close, 2 * Width, Open - Close);
                    graphic.DrawRectangle(pen, X - Width, Close, 2 * Width, Open - Close);
                }
            }
        }
    }
}
