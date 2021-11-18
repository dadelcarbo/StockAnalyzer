using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockDrawing
{
    public class WinRatio : DrawingItem
    {
        static SolidBrush RedBrush = new SolidBrush(Color.FromArgb(50, Color.Red));
        static SolidBrush GreenBrush = new SolidBrush(Color.FromArgb(50, Color.Green));
        Font font = new Font(FontFamily.GenericSansSerif, 7);

        public PointF P1 { get; set; }
        public PointF P2 { get; set; }

        PointF p3 = PointF.Empty;
        public PointF P3
        {
            get => p3; 
            set
            {
                p3 = value;
                if (value == PointF.Empty) return;
                if ((p3.Y > P1.Y && P2.Y < P1.Y) || (p3.Y < P1.Y && P2.Y > P1.Y))
                {
                    float tmp = P1.Y;
                    P1 = new PointF(P1.X, P2.Y);
                    P2 = new PointF(P2.X, tmp);
                }
            }
        }

        public WinRatio(PointF p1, PointF p2, PointF p3)
        {
            this.P1 = p1;
            this.P2 = p2;
            this.P3 = p3;
        }
        public bool Contains(PointF point)
        {
            return false;
        }

        public override void Draw(Graphics g, Pen pen, System.Drawing.Drawing2D.Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            PointF[] points = new PointF[] { this.P1, this.P2, this.P3 }.Where(p => p != PointF.Empty).ToArray();

            this.Transform(matrixValueToScreen, isLog, points);
            var left = points.Min(p => p.X);
            var right = points.Max(p => p.X);
            var width = right - left;

            switch (points.Length)
            {
                case 2:
                    {
                        var loss = Math.Abs(points[0].Y - points[1].Y);
                        g.FillRectangle(RedBrush, left, Math.Min(points[0].Y, points[1].Y), width, Math.Abs(points[0].Y - points[1].Y));
                        g.DrawRectangle(Pens.Red, left, Math.Min(points[0].Y, points[1].Y), width, loss);
                    }
                    break;
                case 3:
                    {
                        var ratio = (P3.Y - P2.Y) / (P2.Y - P1.Y);
                        var loss = Math.Abs(points[0].Y - points[1].Y);
                        g.FillRectangle(RedBrush, left, Math.Min(points[0].Y, points[1].Y), width, loss);
                        g.FillRectangle(GreenBrush, left, Math.Min(points[1].Y, points[2].Y), width, Math.Abs(points[1].Y - points[2].Y));
                        g.DrawRectangle(Pens.Red, left, Math.Min(points[0].Y, points[1].Y), width, loss);
                        g.DrawRectangle(Pens.Green, left, Math.Min(points[1].Y, points[2].Y), width, Math.Abs(points[1].Y - points[2].Y));
                        g.DrawString(ratio.ToString("#.##"), font, Brushes.Black, right, points[2].Y);
                        for (float i = 1; i < ratio; i++)
                        {
                            var height = points[0].Y > points[1].Y ? Math.Max(points[1].Y, points[2].Y) - i * loss : Math.Min(points[1].Y, points[2].Y) + i * loss;
                            g.DrawLine(Pens.Green, left, height, right, height);
                        }
                    }
                    break;
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
