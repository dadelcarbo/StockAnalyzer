using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockDrawing
{
    public class Area
    {
        public Area(int count)
        {
            this.UpLine = new FloatSerie(count, float.NaN);
            this.DownLine = new FloatSerie(count, float.NaN);
        }
        public Brush Brush { get; set; }
        public FloatSerie UpLine { get; set; }
        public FloatSerie DownLine { get; set; }
    }
}
