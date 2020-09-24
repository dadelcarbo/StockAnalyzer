using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;

namespace StockAnalyzer.StockDrawing
{
    [Serializable]
    public abstract class DrawingItem : IXmlSerializable
    {
        public static bool CreatePersistent = true;
        public static bool CreatedByAlert = false;
        [XmlIgnore]
        public bool IsPersistent { get; set; }
        [XmlIgnore]
        public bool IsAlert { get; set; }

        public DrawingItem()
        {
            this.IsPersistent = DrawingItem.CreatePersistent;
            this.IsAlert = DrawingItem.CreatedByAlert;
            this.Pen = DefaultPen;
        }

        public static Pen DefaultPen = new Pen(Color.Black);
        [XmlIgnore]
        public Pen Pen { get; set; }

        public abstract void Draw(Graphics g, Pen pen, Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog);
        public abstract void Draw(Graphics g, Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog);

        protected void Transform(Matrix matrixValueToScreen, bool isLog, PointF[] points)
        {
            if (isLog)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = new PointF(points[i].X, points[i].Y < 0 ? (float)-Math.Log10(-points[i].Y + 1) : (float)Math.Log10(points[i].Y + 1));
                }
            }
            matrixValueToScreen.TransformPoints(points);
        }

        public static PointF Middle(PointF A, PointF B)
        {
            return new PointF
            {
                X = (A.X + B.X) / 2f,
                Y = (A.Y + B.Y) / 2f
            };
        }

        #region XML SERIALISATION
        public abstract System.Xml.Schema.XmlSchema GetSchema();
        public abstract void ReadXml(System.Xml.XmlReader reader);
        public abstract void WriteXml(System.Xml.XmlWriter writer);
        public abstract void ApplyOffset(int offset);
        #endregion
    }
}
