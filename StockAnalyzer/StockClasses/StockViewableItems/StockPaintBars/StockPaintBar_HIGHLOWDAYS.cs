using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
   public class StockPaintBar_HIGHLOWDAYS : StockPaintBarIndicatorEventBase
   {
      public override System.Drawing.Pen[] SeriePens
      {
         get
         {
            if (seriePens == null)
            {
               seriePens = new Pen[] { new Pen(Color.Green) { Width = 3 }, new Pen(Color.Red) { Width = 3 }, new Pen(Color.Green), new Pen(Color.Red) };
            }
            return seriePens;
         }
      }
   }
}
