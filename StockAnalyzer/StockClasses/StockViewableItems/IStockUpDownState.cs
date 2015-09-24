namespace StockAnalyzer.StockClasses.StockViewableItems
{
   public interface IStockUpDownState
   {
      StockSerie.Trend[] UpDownState { get; }
   }
}
