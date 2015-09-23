using System;
using System.Drawing;

namespace StockAnalyzer.StockDrawing
{
    public class Rectangle2D
    {
        private float Epsilon = 0.001f;
        public PointF TopLeft { get; set; }
        public PointF BottomRight { get; set; }
        public PointF TopRight { get { return new PointF(BottomRight.X, TopLeft.Y); }}
        public PointF BottomLeft { get { return new PointF(TopLeft.X, BottomRight.Y); } }
        public Rectangle2D(System.Drawing.RectangleF rectF)
        {
            TopLeft = new PointF(rectF.Left, rectF.Top);
            BottomRight = new PointF(rectF.Right, rectF.Bottom);
        }
        public Rectangle2D(PointF p1, PointF p2)
        {
            this.TopLeft = new PointF( Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            this.BottomRight = new PointF(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));
        }
        public float Left
        {
            get { return TopLeft.X;}
        }
        public float Right
        {
            get { return BottomRight.X;}
        }
        public float Top
        {
            get { return TopLeft.Y;}
        }
        public float Bottom
        {
            get { return BottomRight.Y;}
        }
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
    }
}
