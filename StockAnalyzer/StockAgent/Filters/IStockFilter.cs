using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockAgent.Filters
{
    public interface IStockFilter
    {
        float EvaluateRank(StockSerie stockSerie, int index);
    }
}
