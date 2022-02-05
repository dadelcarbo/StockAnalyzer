using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockDrawing
{
    public class Box : Rectangle2D
    {

        public Box(PointF p1, PointF p2) : base(p1, p2)
        {
        }

        public override void Draw(Graphics g, Pen pen, System.Drawing.Drawing2D.Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            PointF[] points = new PointF[] { this.TopLeft, this.TopRight, this.BottomRight, this.BottomLeft, this.TopLeft };

            this.Transform(matrixValueToScreen, isLog, points);

            if (graphRectangle.Contains(points[0]))
            {
                g.DrawLines(pen, points);
                if (this.Fill)
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(128, pen.Color)))
                    {
                        g.FillPolygon(brush, points);
                        var range = (this.Top - this.Bottom) / this.Bottom;
                        var right = points.Max(p => p.X);
                        this.DrawText(g, range.ToString("P2"), font, Brushes.Black, Brushes.White, new PointF(right - 30, points[2].Y +2), false, Pens.Black);
                    }
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
