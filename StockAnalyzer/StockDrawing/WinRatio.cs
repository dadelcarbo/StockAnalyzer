using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockDrawing
{
    public class WinRatio : DrawingItem
    {
        static readonly SolidBrush RedBrush = new SolidBrush(Color.FromArgb(50, Color.Red));
        static readonly SolidBrush GreenBrush = new SolidBrush(Color.FromArgb(50, Color.Green));

        public PointF Stop { get; set; }
        public PointF Entry { get; set; }

        PointF p3 = PointF.Empty;
        public PointF Exit { get; set; }

        public WinRatio(PointF stop, PointF entry, PointF exit)
        {
            this.Stop = stop;
            this.Entry = entry;
            this.Exit = exit;
            this.IsPersistent = false;
        }

        public WinRatio(float startX, float endX, float entry, float stop, float exit)
        {
            this.Stop = new PointF(startX, stop);
            this.Entry = new PointF(startX, entry);
            this.Exit = new PointF(endX, exit);
            this.IsPersistent = false;
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
            var width = Math.Max(10, right - left);

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
                        var R = (Entry.Y - Stop.Y);
                        var ratio = (Exit.Y - Entry.Y) / R;
                        var loss = Math.Abs(points[0].Y - points[1].Y);
                        g.FillRectangle(RedBrush, left, Math.Min(points[0].Y, points[1].Y), width, loss);
                        g.FillRectangle(ratio > 0 ? GreenBrush : RedBrush, left, Math.Min(points[1].Y, points[2].Y), width, Math.Abs(points[1].Y - points[2].Y));
                        g.DrawRectangle(Pens.Red, left, Math.Min(points[0].Y, points[1].Y), width, loss);
                        g.DrawRectangle(ratio > 0 ? Pens.Green : Pens.Red, left, Math.Min(points[1].Y, points[2].Y), width, Math.Abs(points[1].Y - points[2].Y));
                        if (ratio > 0)
                        {
                            this.DrawText(g, $"R={ratio.ToString("0.##")}", font, Brushes.Black, Brushes.White, new PointF(left - 35, points[0].Y + 2), true, Pens.Black);

                            if (ratio > 1f)
                            {
                                var rPoints = new PointF[(int)ratio];
                                for (int i = 1; i < ratio; i++)
                                {
                                    rPoints[i - 1] = new PointF(0, Entry.Y + i * R);
                                }
                                this.Transform(matrixValueToScreen, isLog, rPoints);
                                for (int i = 0; i < (int)ratio; i++)
                                {
                                    g.DrawLine(Pens.Green, left, rPoints[i].Y, right, rPoints[i].Y);
                                }
                            }
                        }
                        else
                        {
                            this.DrawText(g, $"R= {ratio.ToString("0.##")}", font, Brushes.Black, Brushes.White, new PointF(left - 40, points[1].Y - 13), true, Pens.Black);

                            if (ratio < -1f)
                            {
                                var rPoints = new PointF[(int)-ratio];
                                for (int i = 1; i < -ratio; i++)
                                {
                                    rPoints[i - 1] = new PointF(0, Entry.Y - i * R);
                                }
                                this.Transform(matrixValueToScreen, isLog, rPoints);
                                for (int i = 0; i < (int)-ratio; i++)
                                {
                                    g.DrawLine(Pens.Red, left, rPoints[i].Y, right, rPoints[i].Y);
                                }
                            }
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
