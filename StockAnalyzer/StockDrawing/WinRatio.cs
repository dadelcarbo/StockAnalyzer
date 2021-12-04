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

        public PointF Stop { get; set; }
        public PointF Entry { get; set; }

        PointF p3 = PointF.Empty;
        public PointF Exit { get; set; }

        public WinRatio(PointF stop, PointF entry, PointF exit)
        {
            this.Stop = stop;
            this.Entry = entry;
            this.Exit = exit;
        }

        public WinRatio(float startX, float endX, float entry, float stop, float exit)
        {
            this.Stop = new PointF(startX, stop);
            this.Entry = new PointF(startX, entry);
            this.Exit = new PointF(endX, exit);
        }
        public bool Contains(PointF point)
        {
            return false;
        }

        public override void Draw(Graphics g, Pen pen, System.Drawing.Drawing2D.Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            PointF[] points = new PointF[] { this.Stop, this.Entry, this.Exit }.Where(p => p != PointF.Empty).ToArray();

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
                        var ratio = (Exit.Y - Entry.Y) / (Entry.Y - Stop.Y);
                        var loss = Math.Abs(points[0].Y - points[1].Y);
                        g.FillRectangle(RedBrush, left, Math.Min(points[0].Y, points[1].Y), width, loss);
                        g.FillRectangle(ratio > 0 ? GreenBrush : RedBrush, left, Math.Min(points[1].Y, points[2].Y), width, Math.Abs(points[1].Y - points[2].Y));
                        g.DrawRectangle(Pens.Red, left, Math.Min(points[0].Y, points[1].Y), width, loss);
                        g.DrawRectangle(ratio > 0 ? Pens.Green : Pens.Red, left, Math.Min(points[1].Y, points[2].Y), width, Math.Abs(points[1].Y - points[2].Y));
                        if (ratio > 0)
                        {
                            g.DrawString($"R={ratio.ToString("0.##")}", font, Brushes.Black, right, points[0].Y);
                        }
                        else
                        {
                            g.DrawString($"R= {ratio.ToString("0.##")}", font, Brushes.Black, right, points[1].Y);
                        }
                        for (float i = 1; i < ratio; i++)
                        {
                            var height = points[0].Y > points[1].Y ? Math.Max(points[1].Y, points[2].Y) - i * loss : Math.Min(points[1].Y, points[2].Y) + i * loss;
                            g.DrawLine(ratio > 0 ? Pens.Green : Pens.Red, left, height, right, height);
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
