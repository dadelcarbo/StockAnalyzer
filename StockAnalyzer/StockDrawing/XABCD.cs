using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace StockAnalyzer.StockDrawing
{

    [Serializable]
    public class XABCD : DrawingItem
    {
        public int NbPoint { get; set; }

        public XABCD()
        {
            X = PointF.Empty;
            A = PointF.Empty;
            B = PointF.Empty;
            C = PointF.Empty;
            D = PointF.Empty;
        }

        public PointF X { get; set; }
        public PointF A { get; set; }
        public PointF B { get; set; }
        public PointF C { get; set; }
        public PointF D { get; set; }

        public bool IsBullish => IsComplete && X.Y < A.Y;

        public bool IsComplete => NbPoint == 5;


        private float xa => Math.Abs(A.Y - X.Y);
        private float ab => Math.Abs(B.Y - A.Y);
        private float ad => Math.Abs(D.Y - A.Y);
        private float bc => Math.Abs(C.Y - B.Y);
        private float cd => Math.Abs(D.Y - C.Y);

        public float XD => ad / xa;
        public float AB => ab / xa;
        public float AC => bc / ab;
        public float BD => cd / bc;
        public float XB => ab / xa;

        public PointF GetLastPoint()
        {
            switch (NbPoint)
            {
                case 1:
                    return X;
                case 2:
                    return A;
                case 3:
                    return B;
                case 4:
                    return C;
            }
            throw new InvalidOperationException("There are already five points in the XABCD pattern");
        }

        public void AddPoint(PointF point)
        {
            if (NbPoint == 5) throw new InvalidOperationException("There are already five points in the XABCD pattern");
            switch (NbPoint)
            {
                case 0:
                    X = point;
                    break;
                case 1:
                    A = point;
                    break;
                case 2:
                    B = point;
                    break;
                case 3:
                    C = point;
                    break;
                case 4:
                    D = point;
                    break;
            }
            NbPoint++;
        }

        static Pen dashPen = new Pen(Color.Black) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
        static Pen fillBullishPen = new Pen(Color.FromArgb(80, Color.DarkGreen)) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
        static Pen fillBearishPen = new Pen(Color.FromArgb(80, Color.DarkRed)) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
        public IEnumerable<DrawingItem> GetLines(Pen drawingPen, bool showRatioText = true)
        {
            var lines = new List<DrawingItem>();

            var fillPen = IsBullish ? fillBullishPen : fillBearishPen;

            if (NbPoint > 1)
            {
                lines.Add(new Segment2D(this.X, this.A));
                lines.Add(new Bullet2D(this.X, 3f));
                lines.Add(new Bullet2D(this.A, 3f));
            }
            if (NbPoint > 2)
            {
                lines.Add(new Segment2D(this.A, this.B));
                lines.Add(new Bullet2D(this.B, 3f));
                lines.Add(new Segment2D(this.X, this.B, dashPen));
                if (showRatioText) lines.Add(new Text2D(Middle(X, B), this.XB.ToString("#.###")));
                lines.Add(new Triangle2D(X, A, B, fillPen));
            }
            if (NbPoint > 3)
            {
                lines.Add(new Segment2D(this.B, this.C));
                lines.Add(new Bullet2D(this.C, 3f));
                lines.Add(new Segment2D(this.A, this.C, dashPen));
                if (showRatioText) lines.Add(new Text2D(Middle(A, C), this.AC.ToString("#.###")));
            }
            if (NbPoint > 4)
            {
                lines.Add(new Segment2D(this.C, this.D));
                lines.Add(new Bullet2D(this.D, 3f));
                lines.Add(new Segment2D(this.B, this.D, dashPen));
                if (showRatioText) lines.Add(new Text2D(Middle(B, D), BD.ToString("#.###")));
                lines.Add(new Segment2D(this.X, this.D, dashPen));
                if (showRatioText) lines.Add(new Text2D(Middle(X, D), this.XD.ToString("#.###")));
                lines.Add(new Triangle2D(B, C, D, fillPen));
                var name = GetPatternName();
                if (name != null)
                {
                    if (IsBullish)
                    {
                        lines.Add(new Text2D(new PointF(A.X, D.Y), name));
                    }
                    else
                    {
                        lines.Add(new Text2D(new PointF(A.X, D.Y + (D.Y - C.Y) / 5f), name));
                    }
                }
            }

            return lines;
        }

        public string GetPatternName()
        {
            return StockHarmonicPatternRatio.MatchPattern(this);
        }

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
