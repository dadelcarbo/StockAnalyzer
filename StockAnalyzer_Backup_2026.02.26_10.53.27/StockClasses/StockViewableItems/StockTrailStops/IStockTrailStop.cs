using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public interface IStockTrailStop : IStockViewableSeries, IStockEvent, IStockText
    {
        FloatSerie[] Series { get; }
        FloatSerie[] Extras { get; }
    }
}
