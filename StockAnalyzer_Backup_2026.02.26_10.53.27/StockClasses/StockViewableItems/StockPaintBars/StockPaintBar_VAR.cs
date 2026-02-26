using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_VAR : StockPaintBarIndicatorEventBase
    {
        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.BlueViolet), new Pen(Color.Green), new Pen(Color.Red) };
                return seriePens;
            }
        }
    }
}
