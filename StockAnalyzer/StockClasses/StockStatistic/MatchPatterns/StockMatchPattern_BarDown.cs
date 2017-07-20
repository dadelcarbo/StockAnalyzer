namespace StockAnalyzer.StockClasses.StockStatistic.MatchPatterns
{
    public class StockMatchPattern_BarDown : IStockMatchPattern
    {
        public bool MatchPattern(StockSerie stockSerie, int index)
        {
            if (index < stockSerie.Count) return stockSerie.GetValue(StockDataType.VARIATION, index) < 0.0f;
            return false;
        }
    }
}