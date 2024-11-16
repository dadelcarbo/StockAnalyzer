using System;
using System.Drawing;

namespace StockAnalyzer.StockDrawing
{
    [Serializable]
    public class Bullet2D : DrawingItem
    {
        public PointF Center { get; private set; }
        public float Size { get; private set; }


        public Bullet2D(PointF center, float size)
        {
            this.Center = center;
            this.Size = size;
            this.Pen = DrawingItem.DefaultPen;
        }
        public Bullet2D(PointF center, float size, Brush brush)
        {
            this.Center = center;
            this.Size = size;
            this.Pen = DrawingItem.DefaultPen;
        }
        public override void Draw(System.Drawing.Graphics g, System.Drawing.Pen pen, System.Drawing.Drawing2D.Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            using Brush brush = new SolidBrush(pen.Color);
            PointF[] points = new PointF[] { this.Center };

            this.Transform(matrixValueToScreen, isLog, points);

            if (graphRectangle.Contains(points[0]))
            {
                g.FillEllipse(brush, points[0].X - this.Size, points[0].Y - this.Size, this.Size * 2, this.Size * 2);
            }
        }
        public override void Draw(Graphics g, System.Drawing.Drawing2D.Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            this.Draw(g, this.Pen, matrixValueToScreen, graphRectangle, isLog);
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
        public override void ApplyOffset(int offset)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
