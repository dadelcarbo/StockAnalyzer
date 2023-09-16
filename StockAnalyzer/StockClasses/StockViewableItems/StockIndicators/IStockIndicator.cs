using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public interface IStockIndicator : IStockViewableSeries, IStockEvent, IStockText
    {
        FloatSerie[] Series { get; }
        HLine[] HorizontalLines { get; }
    }
}
