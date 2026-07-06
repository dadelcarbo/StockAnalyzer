using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData;

namespace StockAnalyzer.StockScripting
{
    public interface IStockFilter
    {
        bool MatchFilter(DataSerie dataSerie, int index = -1);
    }
    public abstract class StockFilterBase : IStockFilter
    {
        public bool MatchFilter(DataSerie dataSerie, int index = -1)
        {
            if (index > dataSerie.Values.Length - 1)
                return false;

            StockDailyValue currentBar = index == -1 ? dataSerie.LastValue : dataSerie.Values[index];

            return MatchFilter(dataSerie, currentBar, index == -1 ? dataSerie.LastIndex : index);
        }
        protected abstract bool MatchFilter(DataSerie dataSerie, StockDailyValue bar, int index);
    }
}
