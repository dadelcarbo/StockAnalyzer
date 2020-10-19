using System.Collections.Generic;
using System.Xml.Serialization;
using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockDrawing
{
    public class StockDrawingItems2
    {
        public StockBarDuration Duration { get; set; }
    
        [XmlArray("DrawingItems")]
        [XmlArrayItem("Line2D", typeof(Line2D))]
        [XmlArrayItem("HalfLine2D", typeof(HalfLine2D))]
        [XmlArrayItem("Segment2D", typeof(Segment2D))]
        [XmlArrayItem("CupHandle2D", typeof(CupHandle2D))]
        public List<DrawingItem> DrawingItems { get; set; }

        public StockDrawingItems2()
        {
            this.Duration = StockBarDuration.Daily;
            this.DrawingItems = new List<DrawingItem>();
        }
        public StockDrawingItems2(StockBarDuration duration)
        {
            this.Duration = duration;
            this.DrawingItems = new List<DrawingItem>();
        }
    }
}
