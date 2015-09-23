using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockDecorators
{
    public interface IStockDecorator : IStockViewableSeries, IStockEvent
    {
        string DecoratedItem { get; set; }
        BoolSerie[] Series { get; }
    }
}
