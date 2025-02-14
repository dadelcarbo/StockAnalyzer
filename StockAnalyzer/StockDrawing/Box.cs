using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockDrawing
{
    public class Box : Rectangle2D
    {
        static readonly Brush fillBrush = new SolidBrush(Color.FromArgb(32, Color.LightGreen));

        public Box(PointF p1, PointF p2) : this(p1, p2, false)
        {
        }
        public Box(PointF p1, PointF p2, bool isOpened) : base(p1, p2)
        {
            this.Fill = true;
            this.Pen = new Pen(Color.Green) { Width = 1 };
        }


        public override void Draw(Graphics g, Pen pen, System.Drawing.Drawing2D.Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            PointF[] points = new PointF[] { this.TopLeft, this.TopRight, this.BottomRight, this.BottomLeft, this.TopLeft };

            this.Transform(matrixValueToScreen, isLog, points);

            if (graphRectangle.Contains(points[0]) && graphRectangle.Contains(points[2]))
            {
                g.DrawLines(pen, points);
                var mid = (2f * points[0].Y + points[2].Y) / 3f;
                g.DrawLine(pen, points[0].X, mid, points[1].X, mid);
                if (this.Fill)
                {
                    g.FillPolygon(fillBrush, points);
                    var range = (this.Bottom - this.Top) / this.Bottom; // Top and Botton inversed
                    var left = points.Max(p => p.X);
                    var text = $"Bars: {Right - Left}\r\nVar:{range.ToString("P2")}";
                    this.DrawText(g, text, font, Brushes.Black, Brushes.White, new PointF(left - 40, points[0].Y + 2), false, Pens.Black);
                }
            }
        }

        public override void Draw(Graphics g, System.Drawing.Drawing2D.Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            this.Draw(g, this.Pen, matrixValueToScreen, graphRectangle, isLog);
        }

        public override System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            throw new NotImplementedException();
        }
        public override void ApplyOffset(int offset)
        {
            throw new NotImplementedException();
        }
    }
}
