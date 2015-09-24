using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
   public interface IStockTrailStop : IStockViewableSeries, IStockEvent
   {
      FloatSerie[] Series { get; }
   }
}
