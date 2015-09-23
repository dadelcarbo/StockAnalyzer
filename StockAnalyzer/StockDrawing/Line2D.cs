using System;
using System.Linq;
using System.Drawing;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockDrawing
{
    [Serializable]
    public class Line2D : Line2DBase
    {
        public Line2D()
        {
        }
        public Line2D(PointF p1, PointF p2)
            : base(p1, p2)
        {
        }
        public Line2D(PointF p1, PointF p2, Pen pen)
            : base(p1, p2)
        {
            this.Pen = pen;
        }
        public Line2D(PointF p1, float vx, float vy)
            : base(p1, vx, vy)
        {
        }
        public Line2D(PointF p1, float vx, float vy, Pen pen)
            : base(p1, vx, vy)
        {
            this.Pen = pen;
        }
        public Line2D(Segment2D segment)
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
            Line2D orthoLine = GetNormalLine(point);

            // Calculate Intersection
            PointF point3 = this.Intersection(orthoLine);

            // Calculate the length on the ortho segment
            if (point != point3)
            {
                Segment2D segment = new Segment2D(point, point3);
                return segment.Length();
            }
            else
            {
                return 0;
            }
        }

        public override Line2DBase Transform(System.Drawing.Drawing2D.Matrix matrix, bool isLog)
        {
            PointF[] points = new PointF[] { this.Point1, this.Point2 };

            this.Transform(matrix, isLog, points);

            return new Line2D(points[0], points[1]);
        }

        public override Segment2D Trim(Rectangle2D rect)
        {
            PointF[] points = this.Intersection(rect);
            if (points.Length < 2)
            {
                return new Segment2D();
            }
            return new Segment2D(points[0], points[1]);
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

        public override Line2DBase Cut(float x, bool cutRight)
        {
            if (this.IsVertical)
            {
                return null;
            }
            HalfLine2D cutLine = null;
            PointF cutPoint = new PointF(x, this.ValueAtX(x));
            if (this.VX > 0 && cutRight)
            {
                cutLine = new HalfLine2D(cutPoint, -this.VX, -this.VY, this.Pen);
            }
            else
            {
                cutLine = new HalfLine2D(cutPoint, this.VX, this.VY, this.Pen);
            }
            return cutLine;
        }
        public override void Draw(Graphics g, Pen pen, System.Drawing.Drawing2D.Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            // Calculate intersection with bounding rectangle
            Line2D newLine = (Line2D)this.Transform(matrixValueToScreen, isLog);
            Segment2D trimmedSegment = newLine.Trim(graphRectangle);

            if (trimmedSegment.Point1 != PointF.Empty && trimmedSegment.Point2 != PointF.Empty)
            {
                g.DrawLine(pen, trimmedSegment.Point1, trimmedSegment.Point2);
            }
            else
            {
                StockLog.Write("Invalid line to be drawn");
            }
        }

        public override void Draw(Graphics g, System.Drawing.Drawing2D.Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            // Calculate intersection with bounding rectangle
            Line2D newLine = (Line2D)this.Transform(matrixValueToScreen, isLog);
            Segment2D trimmedSegment = newLine.Trim(graphRectangle);

            if (trimmedSegment.Point1 != PointF.Empty && trimmedSegment.Point2 != PointF.Empty)
            {
                g.DrawLine(this.Pen, trimmedSegment.Point1, trimmedSegment.Point2);
            }
            else
            {
                StockLog.Write("Invalid line to be drawn");
            }
        }
    }
}
