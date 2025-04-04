﻿using System;
using System.Drawing;

namespace StockAnalyzer.StockDrawing
{
    public class Rectangle2D : DrawingItem
    {

        private readonly float Epsilon = 0.001f;
        public PointF TopLeft { get; set; }
        public PointF BottomRight { get; set; }
        public PointF TopRight => new PointF(BottomRight.X, TopLeft.Y);
        public PointF BottomLeft => new PointF(TopLeft.X, BottomRight.Y);
        public bool Fill { get; set; }

        public Rectangle2D(RectangleF rectF)
        {
            TopLeft = new PointF(rectF.Left, rectF.Top);
            BottomRight = new PointF(rectF.Right, rectF.Bottom);
        }
        public Rectangle2D(PointF p1, PointF p2)
        {
            this.TopLeft = new PointF(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            this.BottomRight = new PointF(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));
        }
        public float Left => TopLeft.X;
        public float Right => BottomRight.X;
        public float Top => TopLeft.Y;
        public float Bottom => BottomRight.Y;
        public bool Contains(PointF point)
        {
            return (point.X >= (Left - Epsilon) && point.X <= (Right + Epsilon) && point.Y >= (Top - Epsilon) && point.Y <= (Bottom + Epsilon));
        }
        public Segment2D[] GetSegments()
        {
            return new Segment2D[] {new Segment2D(TopLeft, TopRight),
                new Segment2D(TopRight, BottomRight),
                new Segment2D(BottomRight, BottomLeft),
                new Segment2D(BottomLeft, TopLeft)};
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
                    using Brush brush = new SolidBrush(Color.FromArgb(128, pen.Color));
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
        public override void ApplyOffset(int offset)
        {
            throw new NotImplementedException();
        }
    }
}
