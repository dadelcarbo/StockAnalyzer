using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockDrawing
{
    [Serializable]
    public class Segment2D : Line2DBase
    {
        public Segment2D() : base()
        {
        }
        public Segment2D(PointF point1, PointF point2)
            : base(point1, point2)
        {
        }
        public Segment2D(PointF point1, PointF point2, Pen pen)
            : base(point1, point2, pen)
        {
        }
        public Segment2D(float x1, float y1, float x2, float y2) : base( new PointF(x1, y1), new PointF(x2, y2))
        {
        }
        
        public float Length()
        {
            return (float)Math.Sqrt((Point1.X - Point2.X) * (Point1.X - Point2.X) + (Point1.Y - Point2.Y) * (Point1.Y - Point2.Y));
        }

        public override Line2DBase Transform(System.Drawing.Drawing2D.Matrix matrix, bool isLog)
        {
            PointF[] points = new PointF[] { this.Point1, this.Point2};

            this.Transform(matrix, isLog, points);

            return new Segment2D(points[0], points[1]);
        }
        #region INTERSECTION
        public override PointF Intersection(Line2D line)
        {
            float k;
            k = line.VX * VY - line.VY * VX;
            if (k == 0) return PointF.Empty;
            k = (VY * (Point1.X - line.Point1.X) + VX * (line.Point1.Y - Point1.Y)) / k;
            PointF intersectionPoint = new PointF(line.Point1.X + k * line.VX, line.Point1.Y + k * line.VY);
            if (new Rectangle2D(this.Point1, this.Point2).Contains(intersectionPoint))
            {
                return intersectionPoint;
            }
            else
            {
                return PointF.Empty;
            }
        }
        public PointF Intersection(HalfLine2D line)
        {
            float k;
            k = line.VX * VY - line.VY * VX;
            if (k == 0) return PointF.Empty;
            k = (VY * (Point1.X - line.Point1.X) + VX * (line.Point1.Y - Point1.Y)) / k;
            PointF intersectionPoint = new PointF(line.Point1.X + k * line.VX, line.Point1.Y + k * line.VY);
            if (line.ContainsAbsciss(intersectionPoint.X) && new Rectangle2D(this.Point1, this.Point2).Contains(intersectionPoint))
            {
                return intersectionPoint;
            }
            else
            {
                return PointF.Empty;
            }
        }
        public PointF Intersection(Segment2D segment)
        {
            Line2D line1 = new Line2D(this.Point1, this.Point2);
            Line2D line2 = new Line2D(segment.Point1, segment.Point2);
            PointF result = line1.Intersection(line2);

            if (result != PointF.Empty)
            {
                if (!new Rectangle2D(this.Point1, this.Point2).Contains(result) || !new Rectangle2D(segment.Point1, segment.Point2).Contains(result))
                {
                    result = PointF.Empty;
                }
            }
            return result;
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
        #endregion
        public override Segment2D Trim(Rectangle2D rect)
        {
            Segment2D segment = new Segment2D();
            if (rect.Contains(this.Point1))
            {
                if (rect.Contains(this.Point2))
                {
                    return this;
                }
                else
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
            }
            else
            {
                if (rect.Contains(this.Point2))
                {
                    segment.Point2 = this.Point2;
                    PointF[] points = this.Intersection(rect);
                    if (points.Length == 1)
                    {
                        segment.Point1 = points[0];
                    }
                    else
                    {
                        StockLog.Write("Exception");
                    }
                }
            }
            return segment;
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
            PointF point3 = this.Intersection(orthoLine);

            if (new Rectangle2D(this.Point1, this.Point2).Contains(point3))
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
        public bool ContainsAbsciss(float x)
        {
            return (x >= Math.Min(this.Point1.X, this.Point2.X)) && (x <= Math.Max(this.Point1.X, this.Point2.X));
        }
        public override Line2DBase Cut(float x, bool cutRight)
        {
            if (this.IsVertical || !this.ContainsAbsciss(x))
            {
                return null;
            }
            Line2DBase cutLine = null;
            PointF cutPoint = new PointF(x, this.ValueAtX(x));
            if (cutPoint == this.Point1 || cutPoint == this.Point2) { return null; } // Don't create an empty segment
            if (this.VX > 0) // First Point is the leftmost point
            {
                if (cutRight)
                {
                    cutLine = new Segment2D(this.Point1, cutPoint, this.Pen);
                }
                else
                {
                    cutLine = new Segment2D(cutPoint, this.Point2, this.Pen);
                }
            }
            else // HalfLine is infinite on the left
            {
                if (cutRight)
                {
                    cutLine = new Segment2D(cutPoint, this.Point2, this.Pen);
                }
                else
                {
                    cutLine = new Segment2D(this.Point1, cutPoint, this.Pen);
                }
            }
            return cutLine;
        }

        public override void Draw(Graphics g, Pen pen, Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            // Calculate intersection with bounding rectangle
            Segment2D newSegment = (Segment2D)this.Transform(matrixValueToScreen, isLog);
            Segment2D trimmedSegment = newSegment.Trim(graphRectangle);
            
            if (trimmedSegment.Point1 != PointF.Empty && trimmedSegment.Point2 != PointF.Empty)
            {
                g.DrawLine(this.Pen, trimmedSegment.Point1, trimmedSegment.Point2);
            }
        }
        public override void Draw(Graphics g, Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            this.Draw(g, this.Pen, matrixValueToScreen, graphRectangle, isLog);
        }
    }
}
