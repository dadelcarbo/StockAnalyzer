using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems
{
   public interface IStockEvent
   {
      int EventCount { get; }
      string[] EventNames { get; }
      bool[] IsEvent { get; }
      BoolSerie[] Events { get; }
   }
}
