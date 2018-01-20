using System;
using System.Drawing;

namespace StockAnalyzer.StockDrawing
{
    [Serializable]
    public class Text2D : DrawingItem
    {
        public PointF Center { get; private set; }
        public string Text { get; private set; }

        static Font font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace), 8);


        public Text2D(PointF center, string text)
        {
            this.Center = center;
            this.Pen = DrawingItem.DefaultPen;
            this.Text = text;
        }
        public Text2D(PointF center, Pen pen, string text)
        {
            this.Center = center;
            this.Pen = pen;
            this.Text = text;
        }
        public override void Draw(Graphics g, Pen pen, System.Drawing.Drawing2D.Matrix matrixValueToScreen, Rectangle2D graphRectangle, bool isLog)
        {
            using (Brush brush = new SolidBrush(pen.Color))
            {
                PointF[] points = new PointF[] { this.Center };

                this.Transform(matrixValueToScreen, isLog, points);

                if (graphRectangle.Contains(points[0]))
                {
                    g.DrawString(this.Text, font, brush, points[0].X, points[0].Y, StringFormat.GenericDefault);
                }
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
        #endregion
    }
}
