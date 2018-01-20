using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace StockAnalyzer.StockDrawing
{
    public class HarmonicPatternRatio
    {
        public string Name { get; set; }
        public FloatRange XB { get; set; } // AB/XA
        public FloatRange AC { get; set; } // BC/AB
        public FloatRange BD { get; set; } // CD/BC
        public FloatRange XD { get; set; } // XB/BD


        //static const float accuracy = 0.1f;
        //static public List<HarmonicPatternRatio> HarmonicPatterns = new List<HarmonicPatternRatio> {
        //    new HarmonicPatternRatio { Name = "Bat", AB = new FloatRange(0.5f,0.62f), BC = new FloatRange(0.62f,0.886f), CD = FloatRange.FromAccuracy(0.786f,accuracy)},
        //    new HarmonicPatternRatio { Name = "Gartley", AB = new FloatRange(0.5f,0.62f), BC = new FloatRange(0.62f,0.886f), CD = FloatRange.FromAccuracy(0.786f,accuracy)},
        //    new HarmonicPatternRatio { Name = "Gartley", AB = new FloatRange(0.5f,0.62f), BC = new FloatRange(0.5f,0.62f), CD = new FloatRange(0.5f,0.62f)}
        //};
    }

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

        private float XA => Math.Abs(A.Y - X.Y);
        private float AB => Math.Abs(B.Y - A.Y);
        private float AD => Math.Abs(D.Y - A.Y);
        private float BC => Math.Abs(C.Y - B.Y);
        private float CD => Math.Abs(D.Y - C.Y);

        public float AC => BC / AB;
        public float XB => AB / XA;
        public float BD => CD / BC;
        public float XD => AD / XA;

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
        static Pen fillPen = new Pen(Color.FromArgb(100, Color.Purple)) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
        public IEnumerable<DrawingItem> GetLines(Pen drawingPen)
        {
            var lines = new List<DrawingItem>();

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
                lines.Add(new Text2D(Text2D.Middle(X, B), this.XB.ToString("#.###")));
                lines.Add(new Triangle2D(X, A, B, fillPen));
            }
            if (NbPoint > 3)
            {
                lines.Add(new Segment2D(this.B, this.C));
                lines.Add(new Bullet2D(this.C, 3f));
                lines.Add(new Segment2D(this.A, this.C, dashPen));
                lines.Add(new Text2D(Text2D.Middle(A, C), this.AC.ToString("#.###")));
            }
            if (NbPoint > 4)
            {
                lines.Add(new Segment2D(this.C, this.D));
                lines.Add(new Bullet2D(this.D, 3f));
                lines.Add(new Segment2D(this.B, this.D, dashPen));
                lines.Add(new Text2D(Text2D.Middle(B, D), BD.ToString("#.###")));
                lines.Add(new Segment2D(this.X, this.D, dashPen));
                lines.Add(new Text2D(Text2D.Middle(X, D), this.XD.ToString("#.###")));
                lines.Add(new Triangle2D(B, C, D, fillPen));
            }

            return lines;
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
