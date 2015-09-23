using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockDrawing
{
    [Serializable]
    public class AndrewPitchFork : DrawingItem
    {
        public PointF Point1 { get; set; }
        public PointF Point2 { get;  set; }
        public PointF Point3 { get;  set; }

        protected Brush brush;

        public AndrewPitchFork()
        {
            this.Point1 = PointF.Empty;
            this.Point2 = PointF.Empty;
            this.Point3 = PointF.Empty;
            this.brush = new SolidBrush(DrawingItem.DefaultPen.Color);
        }

        public AndrewPitchFork(PointF point1, PointF point2, PointF point3)
        {
            this.Point1 = point1;
            this.Point2 = point2;
            this.Point3 = point3;
            this.brush = new SolidBrush(DrawingItem.DefaultPen.Color);
        }
        public AndrewPitchFork(PointF point1, PointF point2, PointF point3, Brush brush)
        {
            this.Point1 = point1;
            this.Point2 = point2;
            this.Point3 = point3;
            this.brush = brush;
        }
        public List<Line2DBase> GetLines(Pen pen)
        {
            List<Line2DBase> lines = new List<Line2DBase>();

            if (this.Point1 == PointF.Empty || this.Point2 == PointF.Empty || this.Point3 == PointF.Empty)
            {
                return null;
            }
            // Calculate projected line
            Segment2D segment2D = new Segment2D(this.Point2, this.Point3);
            
            PointF endPoint = new PointF(this.Point3.X + segment2D.VX, this.Point3.Y + segment2D.VY);

            Line2DBase medianLine = new HalfLine2D(this.Point1, this.Point3, pen);
            lines.Add(medianLine);

            // Calculate channel lines.
            lines.Add(new HalfLine2D(this.Point2, medianLine.VX, medianLine.VY, pen));
            lines.Add(new HalfLine2D(endPoint, medianLine.VX, medianLine.VY, pen));
            lines.Add(new Segment2D(this.Point2, endPoint, pen));

            return lines;
        }
        //public List<Line2DBase> GetLines(Pen pen)
        //{
        //    List<Line2DBase> lines = new List<Line2DBase>();

        //    if (this.Point1 == PointF.Empty || this.Point2 == PointF.Empty || this.Point3 == PointF.Empty)
        //    {
        //        return null;
        //    }
        //    // Calculate Median line
        //    PointF medianPoint = new PointF((this.Point2.X+this.Point3.X)/2.0f, (this.Point2.Y+this.Point3.Y)/2.0f);
        //    Line2DBase medianLine = new HalfLine2D(this.Point1, medianPoint, pen);
        //    lines.Add(medianLine);

        //    // Calculate channel lines.
        //    lines.Add(new HalfLine2D(this.Point2, medianLine.VX, medianLine.VY,pen));
        //    lines.Add(new HalfLine2D(this.Point3, medianLine.VX, medianLine.VY,pen));
        //    lines.Add(new Segment2D(this.Point2, this.Point3,pen));

        //    return lines;
        //}


        public override void Draw(System.Drawing.Graphics g, System.Drawing.Pen pen, System.Drawing.Drawing2D.Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            throw new NotImplementedException();
        }
        public override void Draw(Graphics g, System.Drawing.Drawing2D.Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            throw new NotImplementedException();
        }
        #region XML SERIALISATION
        public override System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
