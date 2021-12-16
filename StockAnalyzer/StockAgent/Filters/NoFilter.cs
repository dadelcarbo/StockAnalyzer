using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockAgent.Filters
{
    public class NoFilter : IStockFilter
    {
        public float EvaluateRank(StockSerie stockSerie, int index)
        {
            return 0.0f;
        }
    }
}
