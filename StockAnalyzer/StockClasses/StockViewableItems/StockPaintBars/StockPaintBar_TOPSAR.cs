using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_TOPSAR : StockPaintBarIndicatorEventBase
    {
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { 
                   new Pen(Color.Green), new Pen(Color.Red), //"SupportDetected", "ResistanceDetected", // 0,1
                   new Pen(Color.Green), new Pen(Color.Red), //"Pullback", "EndOfTrend",                // 2,3
                   new Pen(Color.Green), new Pen(Color.Red), //"HigherLow", "LowerHigh",                // 4,5
                   new Pen(Color.Green), new Pen(Color.Red), //"ResistanceBroken", "SupportBroken",     // 6,7
                   new Pen(Color.Green), new Pen(Color.Red)  //"Bullish", "Bearish"                     // 8,9
                    };
                }
                return seriePens;
            }
        }
    }
}
