using System;
using System.Drawing;

namespace StockAnalyzer.StockDrawing
{
    public class Triangle2D : DrawingItem
    {
        private float Epsilon = 0.001f;
        public PointF A { get; set; }
        public PointF B { get; set; }
        public PointF C { get; set; }

        public Triangle2D(PointF a, PointF b, PointF c, Pen pen)
        {
            A = a;
            B = b;
            C = c;
            Pen = pen;
        }        

        public override void Draw(Graphics g, Pen pen, System.Drawing.Drawing2D.Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            using (Brush brush = new SolidBrush(pen.Color))
            {
                PointF[] points = new PointF[] { this.A, this.B, this.C, this.A };

                this.Transform(matrixValueToScreen, isLog, points);

                if (graphRectangle.Contains(points[0]))
                {
                    g.FillPolygon(brush, points);
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
    }
}
