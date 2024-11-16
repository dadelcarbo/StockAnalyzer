using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_EMA : StockPaintBarIndicatorEventBase
    {
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                // "Bottom", "Top", "CrossAbove", "CrossBelow", "FirstBarAbove", "FirstBarBelow", "PriceAbove", "PriceBelow", "BarAbove", "BarBelow", "Rising", "Falling" };
                seriePens ??= new Pen[] { new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red) };
                return seriePens;
            }
        }
    }
}
