using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_RISKSR : StockPaintBarIndicatorEventBase
    {
        //"SupportBroken", "ResistanceBroken",        // 0,1
        //"Pullback", "EndOfTrend",                   // 2,3
        //"HigherLow", "LowerHigh",                   // 4,5
        //"ResistanceBroken", "SupportBroken",        // 6,7
        //"Bullish", "Bearish"                        // 8,9

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] {
                        new Pen(Color.Red), new Pen(Color.Green),
                        new Pen(Color.Red), new Pen(Color.Green),
                        new Pen(Color.Green), new Pen(Color.Red),
                        new Pen(Color.Green), new Pen(Color.Red),
                        new Pen(Color.Green), new Pen(Color.Red) };
                }
                return seriePens;
            }
        }
    }
}
