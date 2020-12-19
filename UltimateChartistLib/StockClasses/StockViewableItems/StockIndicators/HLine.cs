using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class HLine
   {
      public float Level { get; set; }
      public Pen LinePen { get; private set; }

      public HLine(float level, Pen pen)
      {
         this.Level = level;
         this.LinePen = pen;
      }
   }
}
