using System;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockDrawing
{
    [Serializable]
    public class HalfLine2D : Line2DBase
    {
        public HalfLine2D()
        {
        }
        public HalfLine2D(PointF p1, PointF p2)
           : base(p1, p2)
        {
        }

        public HalfLine2D(PointF p1, PointF p2, Pen pen)
           : base(p1, p2, pen)
        {
        }
        public HalfLine2D(PointF p1, float vx, float vy)
           : base(p1, vx, vy)
        {
        }
        public HalfLine2D(PointF p1, float vx, float vy, Pen pen)
           : base(p1, vx, vy, pen)
        {
        }
        public HalfLine2D(Segment2D segment)
           : base(segment.Point1, segment.Point2)
        {
        }

        public override float DistanceTo(PointF point)
        {
            if (this.Point1 == point || this.Point2 == point)
            {
                return 0;
            }

            // Calculate orhogonal line
            Line2D orthoLine = this.GetNormalLine(point);

            // Calculate Intersection
            PointF point3 = new Line2D(this.Point1, this.Point2).Intersection(orthoLine);

            if (this.ContainsAbsciss(point3.X))
            {
                // Calculate the length on the ortho segment
                Segment2D segment = new Segment2D(point, point3);
                return segment.Length();
            }
            else
            {
                Segment2D segment1 = new Segment2D(point, this.Point1);
                Segment2D segment2 = new Segment2D(point, this.Point2);
                return Math.Min(segment1.Length(), segment2.Length());
            }
        }

        public override Line2DBase Transform(System.Drawing.Drawing2D.Matrix matrix, bool isLog)
        {
            PointF[] points = new PointF[] { this.Point1, this.Point2 };

            this.Transform(matrix, isLog, points);

            return new HalfLine2D(points[0], points[1]);
        }
        public override Segment2D Trim(Rectangle2D rect)
        {
            Segment2D segment = new Segment2D();
            if (rect.Contains(this.Point1))
            {
                segment.Point1 = this.Point1;
                PointF[] points = this.Intersection(rect);
                if (points.Length == 1)
                {
                    segment.Point2 = points[0];
                }
                else
                {
                    StockLog.Write("Exception");
                }
            }
            else
            {
                return new Line2D(this.Point1, this.Point2).Trim(rect);
            }
            return segment;
        }

        public override PointF Intersection(Line2D line)
        {
            float k;
            k = line.VX * VY - line.VY * VX;
            if (k == 0) return PointF.Empty;
            k = (VY * (Point1.X - line.Point1.X) + VX * (line.Point1.Y - Point1.Y)) / k;
            return new PointF(line.Point1.X + k * line.VX, line.Point1.Y + k * line.VY);
        }

        public override PointF[] Intersection(Rectangle2D rect)
        {
            Segment2D[] segmentArray = rect.GetSegments();

            PointF[] pointArray = new PointF[] { PointF.Empty, PointF.Empty };
            int nbPoints = 0;

            PointF intersectionPoint = PointF.Empty;
            foreach (Segment2D segment in segmentArray)
            {
                intersectionPoint = segment.Intersection(this);
                if (intersectionPoint != PointF.Empty && !pointArray.Contains(intersectionPoint))
                {
                    pointArray[nbPoints++] = intersectionPoint;
                }
            }
            return pointArray.Take(nbPoints).ToArray();
        }

        public override bool ContainsAbsciss(float x)
        {
            if (this.VX == 0 && Point1.X == x)
            {
                return true;
            }
            if (this.VX > 0 && x >= Point1.X) return true;
            if (this.VX < 0 && x <= Point1.X) return true;
            return false;
        }
        public override Line2DBase Cut(float x, bool cutRight)
        {
            if (this.IsVertical || !this.ContainsAbsciss(x))
            {
                return null;
            }
            Line2DBase cutLine = null;
            PointF cutPoint = new PointF(x, this.ValueAtX(x));
            if (cutPoint == this.Point1) { return null; } // Don't create an empty segment
            if (this.VX > 0) // HalfLine is infinite on the right
            {
                if (cutRight)
                {
                    cutLine = new Segment2D(this.Point1, cutPoint, this.Pen);
                }
                else
                {
                    cutLine = new HalfLine2D(cutPoint, this.VX, this.VY, this.Pen);
                }
            }
            else // HalfLine is infinite on the left
            {
                if (cutRight)
                {
                    cutLine = new HalfLine2D(cutPoint, this.VX, this.VY, this.Pen);
                }
                else
                {
                    cutLine = new Segment2D(this.Point1, cutPoint, this.Pen);
                }
            }
            cutLine.Pen = this.Pen;
            return cutLine;
        }
        public override void Draw(Graphics g, Pen pen, System.Drawing.Drawing2D.Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            // Calculate intersection with bounding rectangle
            HalfLine2D newLine = (HalfLine2D)this.Transform(matrixValueToScreen, isLog);
            Segment2D trimmedSegment = newLine.Trim(graphRectangle);

            if (trimmedSegment.Point1 != PointF.Empty && trimmedSegment.Point2 != PointF.Empty)
            {
                g.DrawLine(pen, trimmedSegment.Point1, trimmedSegment.Point2);
            }
            else
            {
                StockLog.Write("Invalid half line to be drawn");
            }
        }
        public override void Draw(Graphics g, System.Drawing.Drawing2D.Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            // Calculate intersection with bounding rectangle
            HalfLine2D newLine = (HalfLine2D)this.Transform(matrixValueToScreen, isLog);
            Segment2D trimmedSegment = newLine.Trim(graphRectangle);

            if (trimmedSegment.Point1 != PointF.Empty && trimmedSegment.Point2 != PointF.Empty)
            {
                g.DrawLine(this.Pen, trimmedSegment.Point1, trimmedSegment.Point2);
            }
            else
            {
                StockLog.Write("Invalid half line to be drawn");
            }
        }
    }
}
