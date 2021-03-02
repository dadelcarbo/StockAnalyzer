using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_ROR : StockPaintBarIndicatorEventBase
    {
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.DarkRed), new Pen(Color.DarkGreen), new Pen(Color.DarkRed), new Pen(Color.DarkGreen), new Pen(Color.DarkGreen) };
                }
                return seriePens;
            }
        }
    }
}
