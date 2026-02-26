using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_ROCEX3 : StockPaintBarIndicatorEventBase
    {
        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red) };
                return seriePens;
            }
        }
    }
}
