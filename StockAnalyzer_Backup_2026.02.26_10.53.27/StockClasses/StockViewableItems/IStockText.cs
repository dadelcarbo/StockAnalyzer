using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockViewableItems
{
    public interface IStockText
    {
        List<StockText> StockTexts { get; }
    }

    public class StockText
    {
        public string Text { get; set; }
        public int Index { get; set; }
        public bool AbovePrice { get; set; }

        public float Price { get; set; } = float.NaN;
    }
}
