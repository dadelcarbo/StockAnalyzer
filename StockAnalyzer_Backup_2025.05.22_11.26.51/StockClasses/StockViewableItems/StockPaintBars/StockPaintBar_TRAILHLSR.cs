using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_TRAILHLSR : StockPaintBarIndicatorEventBase
    {
        //static string[] eventNames = new string[] { 
        //"SupportDetected", "ResistanceDetected",
        //"Pullback", "EndOfTrend", 
        //"HigherLow", "LowerHigh", 
        //"ResistanceBroken", "SupportBroken",
        //"Bullish", "Bearish" };

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red) };
                return seriePens;
            }
        }
    }
}
