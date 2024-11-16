using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_HLAVG : StockPaintBarIndicatorEventBase
    {
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                //  "Bottom", "Top", "CloseAbove", "CloseBelow", "FirstBarAbove", "FirstBarBelow", "Bullish", "Bearish" , "BullRun", "BearRun"
                seriePens ??= new Pen[] { new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red) };
                return seriePens;
            }
        }
    }
}
