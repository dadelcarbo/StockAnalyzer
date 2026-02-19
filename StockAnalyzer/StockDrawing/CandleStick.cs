using StockAnalyzerSettings;
using System.Drawing;

namespace StockAnalyzer.StockDrawing
{
    public class CandleStick
    {
        public int Width { get; set; }
        public int X { get; set; }
        public int Open { get; set; }
        public int High { get; set; }
        public int Low { get; set; }
        public int Close { get; set; }
        public CandleStick()
        {
            Width = 5;
        }
        public CandleStick(float x, float open, float high, float low, float close)
        {
            Width = 5;
            X = (int)x;
            Open = (int)open;
            High = (int)high;
            Low = (int)low;
            Close = (int)close;
        }

        public void Draw(Graphics graphic)
        {
            if (Close == Open)
            {
                Pen pen = ColorManager.GetPen("Graph.CandleWick.Up");
                graphic.DrawLine(pen, X, Low, X, High);
                graphic.DrawLine(pen, X - Width, Close, X + Width, Close);
            }
            else
            {
                if (Close > Open)
                {
                    Pen pen = ColorManager.GetPen("Graph.CandleWick.Down"); // Upside down in pixel coordinates
                    var candleBrush = ColorManager.GetBrush("Graph.Candle.Down");

                    graphic.DrawLine(pen, X, Low, X, High);
                    graphic.FillRectangle(candleBrush, X - Width, Open, 2 * Width, Close - Open);
                    graphic.DrawRectangle(pen, X - Width, Open, 2 * Width, Close - Open);
                }
                else
                {
                    Pen pen = ColorManager.GetPen("Graph.CandleWick.Up"); // Upside down in pixel coordinates
                    var candleBrush = ColorManager.GetBrush("Graph.Candle.Up");

                    graphic.DrawLine(pen, X, Low, X, High);
                    graphic.FillRectangle(candleBrush, X - Width, Close, 2 * Width, Open - Close);
                    graphic.DrawRectangle(pen, X - Width, Close, 2 * Width, Open - Close);
                }
            }
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
