using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public interface IStockIndicator : IStockViewableSeries, IStockEvent
    {
        FloatSerie[] Series { get; }
        HLine[] HorizontalLines { get; }
    }
}
