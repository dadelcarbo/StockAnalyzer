using System.Drawing;
using System.Xml.Serialization;

namespace StockAnalyzer.StockDrawing
{
    public abstract class Line2DBase : DrawingItem
    {
        public bool IsHorizontal { get { return VY == 0.0f; } }
        public bool IsVertical { get { return VX == 0.0f; } }
        public PointF Point1 { get; set; }
        public PointF Point2 { get; set; }

        public float VX { get { return Point2.X - Point1.X; } }
        public float VY { get { return Point2.Y - Point1.Y; } }

        public float a
        {
            get
            {
                if (VX == 0.0f) { return float.MaxValue; }
                else { return VY / VX; }
            }
        } 


        public Line2DBase()
        {
            this.Pen = DrawingItem.DefaultPen;
        }
        public Line2DBase(PointF p1, PointF p2)
        {
            this.Pen = DrawingItem.DefaultPen;
            if (!p1.Equals(p2))
            {
                Point1 = p1;
                Point2 = p2;
            }
            else
            {
                throw new System.ArithmeticException("Points are equals");
            }
        }
        public Line2DBase(PointF p1, PointF p2, Pen pen)
        {
            this.Pen = pen;
            if (!p1.Equals(p2))
            {
                Point1 = p1;
                Point2 = p2;
            }
            else
            {
                throw new System.ArithmeticException("Points are equals");
            }
        }
        public Line2DBase(PointF p1, float vx, float vy)
        {
            this.Pen = DrawingItem.DefaultPen;
            if (vx == 0.0f && vy == 0.0f)
            {
                throw new System.ArithmeticException("Points are equals");
            }
            else
            {
                Point1 = p1;
                Point2 = new PointF(p1.X + vx, p1.Y + vy); ;
            }
        }
        public Line2DBase(PointF p1, float vx, float vy, Pen pen)
        {
            this.Pen = pen;
            if (vx == 0.0f && vy == 0.0f)
            {
                throw new System.ArithmeticException("Points are equals");
            }
            else
            {
                Point1 = p1;
                Point2 = new PointF(p1.X + vx, p1.Y + vy);
            }
        }
        public abstract Line2DBase Transform(System.Drawing.Drawing2D.Matrix matrix, bool isLog);
        public abstract PointF Intersection(Line2D line);
        public abstract PointF[] Intersection(Rectangle2D rect);
        public abstract float DistanceTo(PointF point);
        public virtual bool ContainsAbsciss(float x)
        {
            return VX == 0 ? x == Point1.X : true;
        }
        public bool IsAbovePoint(PointF point)
        {
            Line2D line = new Line2D(point, 0.0f, 1.0f);
            PointF intersection = this.Intersection(line);
            if (intersection != PointF.Empty && intersection.Y > point.Y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public float ValueAtX(float x)
        {
            Line2D line = new Line2D(new PointF(x, 0.0f), 0.0f, 1.0f);
            PointF intersection = this.Intersection(line);
            if (intersection != PointF.Empty)
            {
                return intersection.Y;
            }
            else
            {
                return float.NegativeInfinity;
            }
        }
        public Line2D GetNormalLine(PointF point)
        {
            PointF point2 = new PointF(point.X + VY, point.Y - VX);
            Line2D orthoLine = new Line2D(point, point2);
            return orthoLine;
        }
        public Line2D GetParallelLine(PointF point)
        {
            PointF point2 = new PointF(point.X + VX, point.Y + VY);
            return new Line2D(point, point2);
        }

        public abstract Segment2D Trim(Rectangle2D rect);

        public abstract Line2DBase Cut(float x, bool cutRight);

        #region XML SERIALISATION
        public override System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
        public override void ReadXml(System.Xml.XmlReader reader)
        {
            //XmlSerializer serializer = new XmlSerializer(typeof(PointF));

            //reader.Get("P"), StockAnalyzerApp.Global.EnglishCulture

            //this.Point1= (PointF)serializer.Deserialize(reader);
            //this.Point2= (PointF)serializer.Deserialize(reader);
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

            if (this.Pen != null && !Pen.Equals(DefaultPen))
            {
                writer.WriteElementString("Pen", GraphCurveType.PenToString(this.Pen));
            }
        }
        #endregion
    }
}
