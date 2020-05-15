using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Xml.Serialization;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockDrawing
{
    [Serializable]
    public class CupHandle2D : Segment2D
    {
        public CupHandle2D()
        {

        }
        public CupHandle2D(PointF point1, PointF point2, PointF pivot, PointF leftLow, PointF rightLow, Pen pen, bool inverse = false)
           : base(point1, point2, pen)
        {
            this.Pivot = pivot;
            this.LeftLow = leftLow;
            this.RightLow = rightLow;
            this.Inverse = inverse;
        }

        public PointF Pivot { get; set; }
        public PointF RightLow { get; set; }
        public PointF LeftLow { get; set; }
        public bool Inverse { get; set; }

        const int PIVOT_SIZE = 6;
        public override void Draw(Graphics g, Pen pen, Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            // Calculate intersection with bounding rectangle
            Segment2D newSegment = (Segment2D)this.Transform(matrixValueToScreen, isLog);
            Segment2D trimmedSegment = newSegment.Trim(graphRectangle);

            if (trimmedSegment.Point1 != PointF.Empty && trimmedSegment.Point2 != PointF.Empty)
            {
                g.DrawLine(pen, trimmedSegment.Point1, trimmedSegment.Point2);
            }

            PointF[] points = new PointF[] { this.Pivot };
            this.Transform(matrixValueToScreen, isLog, points);
            foreach (var transformedPivot in points)
            {
                if (graphRectangle.Contains(transformedPivot))
                {
                    var offset = Inverse ? 0 : PIVOT_SIZE;
                    g.FillEllipse(Brushes.Black, transformedPivot.X - (PIVOT_SIZE / 2), transformedPivot.Y - offset, PIVOT_SIZE, PIVOT_SIZE);
                }
            }

            points = new PointF[] { this.LeftLow, this.RightLow };
            this.Transform(matrixValueToScreen, isLog, points);
            foreach (var transformedPivot in points)
            {
                if (graphRectangle.Contains(transformedPivot))
                {
                    var offset = Inverse ? PIVOT_SIZE : 0;
                    g.FillEllipse(Brushes.Black, transformedPivot.X - (PIVOT_SIZE / 2), transformedPivot.Y - offset, PIVOT_SIZE, PIVOT_SIZE);
                }
            }
        }
        public override void Draw(Graphics g, Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            this.Draw(g, this.Pen, matrixValueToScreen, graphRectangle, isLog);
        }
        public override void ApplyOffset(int offset)
        {
            this.Point1 = new PointF(this.Point1.X + offset, this.Point1.Y);
            this.Point2 = new PointF(this.Point2.X + offset, this.Point2.Y);
            this.Pivot = new PointF(this.Point2.X + offset, this.Point2.Y);
            this.RightLow = new PointF(this.Point2.X + offset, this.Point2.Y);
            this.LeftLow = new PointF(this.Point2.X + offset, this.Point2.Y);
        }
        #region XML SERIALISATION
        public override System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
        public override void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();

            // Point1
            reader.ReadStartElement();
            float x, y;
            x = reader.ReadElementContentAsFloat();
            y = reader.ReadElementContentAsFloat();
            this.Point1 = new PointF(x, y);
            reader.ReadEndElement();

            // Point2
            reader.ReadStartElement();
            x = reader.ReadElementContentAsFloat();
            y = reader.ReadElementContentAsFloat();
            this.Point2 = new PointF(x, y);
            reader.ReadEndElement();

            if (reader.Name == "PointF")
            {
                // Pivot
                reader.ReadStartElement();
                x = reader.ReadElementContentAsFloat();
                y = reader.ReadElementContentAsFloat();
                this.Pivot = new PointF(x, y);
                reader.ReadEndElement();

                // RightLow
                reader.ReadStartElement();
                x = reader.ReadElementContentAsFloat();
                y = reader.ReadElementContentAsFloat();
                this.RightLow = new PointF(x, y);
                reader.ReadEndElement();

                // LeftLow
                reader.ReadStartElement();
                x = reader.ReadElementContentAsFloat();
                y = reader.ReadElementContentAsFloat();
                this.LeftLow = new PointF(x, y);
                reader.ReadEndElement();
            }

            // Pen if exists
            if (reader.Name == "Pen")
            {
                this.Pen = GraphCurveType.PenFromString(reader.ReadElementContentAsString());
            }
            else
            {
                this.Pen = DefaultPen;
            }
            reader.ReadEndElement();
        }
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PointF));
            serializer.Serialize(writer, this.Point1);
            serializer.Serialize(writer, this.Point2);
            serializer.Serialize(writer, this.Pivot);
            serializer.Serialize(writer, this.RightLow);
            serializer.Serialize(writer, this.LeftLow);

            if (this.Pen != null && !Pen.Equals(DefaultPen))
            {
                writer.WriteElementString("Pen", GraphCurveType.PenToString(this.Pen));
            }
        }
        #endregion

    }
}
