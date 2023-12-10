using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_EMA3Lines : StockPaintBarIndicatorEventBase
    {
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Green), new Pen(Color.Red) };
                return seriePens;
            }
        }
    }
}
