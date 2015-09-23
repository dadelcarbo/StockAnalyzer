using StockAnalyzer.StockMath;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrails
{
    public interface IStockTrail : IStockIndicator
    {
        string TrailedItem { get; set; }
    }
}
