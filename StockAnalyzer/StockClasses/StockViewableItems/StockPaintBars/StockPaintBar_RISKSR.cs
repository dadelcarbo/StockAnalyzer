using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_RISKSR : StockPaintBarIndicatorEventBase
    {
        //static string[] eventNames = new string[] {
        //    "SupportBroken", "ResistanceBroken",
        //    "Bullish", "Bearish" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Green), new Pen(Color.Red) };
                }
                return seriePens;
            }
        }
    }
}
