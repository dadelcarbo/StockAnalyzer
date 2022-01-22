using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace StockAnalyzer.StockDrawing
{
    [Serializable]
    public abstract class DrawingItem : IXmlSerializable
    {
        public static bool CreatePersistent = true;
        public static bool KeepTransient = false;
        [XmlIgnore]
        public bool IsPersistent { get; set; }

        public DrawingItem()
        {
            this.IsPersistent = DrawingItem.CreatePersistent;
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

        public void DrawText(Graphics g, string text, Font font, Brush brush, Brush backgroundBrush, PointF location, bool drawFrame, Pen pen)
        {
            string trimmedText = text.Trim();
            Size size = TextRenderer.MeasureText(trimmedText, font);

            RectangleF rect = new RectangleF(location.X, location.Y, size.Width, size.Height);
            if (drawFrame)
            {
                g.FillRectangle(backgroundBrush, location.X - 1, location.Y - 1, size.Width - 6, size.Height + 1);
                g.DrawRectangle(pen, location.X - 1, location.Y - 1, size.Width - 6, size.Height + 1);
            }
            g.DrawString(trimmedText, font, brush, rect);
        }
        #region XML SERIALISATION
        public abstract System.Xml.Schema.XmlSchema GetSchema();
        public abstract void ReadXml(System.Xml.XmlReader reader);
        public abstract void WriteXml(System.Xml.XmlWriter writer);
        public abstract void ApplyOffset(int offset);
        #endregion
    }
}
