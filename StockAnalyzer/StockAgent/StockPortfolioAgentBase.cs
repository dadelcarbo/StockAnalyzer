using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent
{
    public abstract class StockPortfolioAgentBase : StockAgentBase, IStockPortfolioAgent
    {
        public FloatSerie RankSerie { get; set; }
    }
}
