using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_HIGHLOWBARS : StockPaintBarIndicatorEventBase
    {
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] {
                   new Pen(Color.Green) { Width = 3 },
                   new Pen(Color.Red) { Width = 3 },
                   new Pen(Color.Red) { Width = 3 },
                   new Pen(Color.Green) { Width = 3 },
                   new Pen(Color.Green),
                   new Pen(Color.Red) };
                return seriePens;
            }
        }
    }
}
