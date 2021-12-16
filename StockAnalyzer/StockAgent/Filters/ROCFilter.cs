using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockAgent.Filters
{
    public class ROCFilter : IStockFilter
    {
        public float EvaluateRank(StockSerie stockSerie, int index)
        {
            return -stockSerie.GetIndicator("ROC(100)").Series[0][index];
        }
    }
}
