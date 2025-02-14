using StockAnalyzer.StockMath;
using System.Drawing;

namespace StockAnalyzer.StockDrawing
{
    public class Area
    {
        public Area()
        {

        }
        public Area(int count)
        {
            this.UpLine = new FloatSerie(count, float.NaN);
            this.DownLine = new FloatSerie(count, float.NaN);
        }
        public string Name { get; set; } = "Fill";
        public bool Visibility { get; set; } = true;
        private Color color;
        public Color Color { get { return color; } set { if (color != value) { color = value; brush = null; } } }

        private Brush brush;
        public Brush Brush => brush ??= new SolidBrush(Color);
        public FloatSerie UpLine { get; set; }
        public FloatSerie DownLine { get; set; }
    }
}
