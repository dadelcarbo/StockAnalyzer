using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
    public class StockPaintBar_ER : StockPaintBarIndicatorEventBase
    {
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    //  "Positive", "Negative", "TurnedPositive", "TurnedNegative", "Overbought", "Oversold", "OutOfOversold", "OutOfOverbought" };
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red) };
                }
                return seriePens;
            }
        }
    }
}
