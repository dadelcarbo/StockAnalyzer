using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockScripting
{
    public interface IStockFilter
    {
        bool MatchFilter(StockSerie stockSerie, BarDuration barDuration, int index = -1);
    }
    public abstract class StockFilterBase : IStockFilter
    {
        public bool MatchFilter(StockSerie stockSerie, BarDuration barDuration, int index = -1)
        {
            if (!stockSerie.Initialise())
                return false;
            var previousBarDuration = stockSerie.BarDuration;
            stockSerie.BarDuration = barDuration;

            if (index > stockSerie.ValueArray.Length - 1)
                return false;

            StockDailyValue currentBar = index == -1 ? stockSerie.LastValue : stockSerie.ValueArray[index];

            return MatchFilter(stockSerie, currentBar, index == -1 ? stockSerie.LastIndex : index);
        }
        protected abstract bool MatchFilter(StockSerie stockSerie, StockDailyValue bar, int index);
    }
}
