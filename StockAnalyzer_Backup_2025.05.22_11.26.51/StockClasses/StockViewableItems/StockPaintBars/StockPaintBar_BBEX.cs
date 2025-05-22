using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_BBEX : StockPaintBarIndicatorEventBase
    {
        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] {
                   new Pen(Color.Green) { Width = 3 },
                   new Pen(Color.Red) { Width = 3 },
                   new Pen(Color.Green) { Width = 2 },
                   new Pen(Color.Red) { Width = 2 },
                   new Pen(Color.Green) { Width = 3 },
                   new Pen(Color.Red) { Width = 3 } };
                return seriePens;
            }
        }
    }
}