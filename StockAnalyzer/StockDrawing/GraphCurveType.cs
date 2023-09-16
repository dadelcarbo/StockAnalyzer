using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;

namespace StockAnalyzer.StockDrawing
{
    public class GraphCurveType
    {
        [XmlIgnore()]
        public Pen CurvePen { get; set; }
        [XmlIgnore()]
        public FloatSerie DataSerie { get; set; }
        public bool IsVisible { get; set; }

        public override string ToString()
        {
            if (this.DataSerie != null)
            {
                return this.DataSerie.Name;
            }
            else
            {
                return string.Empty;
            }
        }
        [XmlElement("XMLPen")]
        public string XmlPen
        {
            get
            {
                return GraphCurveType.PenToString(this.CurvePen);
            }
            set
            {
                this.CurvePen = GraphCurveType.PenFromString(value);
            }
        }
        public static Pen PenFromString(string penString)
        {
            string[] pieces = penString.Split(':');
            Pen pen = new Pen(Color.FromArgb(byte.Parse(pieces[1]), byte.Parse(pieces[2]), byte.Parse(pieces[3]), byte.Parse(pieces[4])), float.Parse(pieces[0]));
            if (pieces.Length == 6)
            {
                pen.DashStyle = (DashStyle)Enum.Parse(typeof(DashStyle), pieces[5]);
            }
            return pen;
        }
        public static string PenToString(Pen pen)
        {
            return pen.Width.ToString() + ":" + pen.Color.A.ToString() + ":" + pen.Color.R.ToString() + ":" + pen.Color.G.ToString() + ":" + pen.Color.B.ToString() + ":" + pen.DashStyle.ToString();
        }
        public GraphCurveType(FloatSerie serie, string penString, bool isVisible)
        {
            this.XmlPen = penString;
            this.IsVisible = isVisible;
            this.DataSerie = serie;
        }
        public GraphCurveType(FloatSerie serie, Pen pen, bool isVisible)
        {
            this.CurvePen = pen;
            this.IsVisible = isVisible;
            this.DataSerie = serie;
        }
    }
}