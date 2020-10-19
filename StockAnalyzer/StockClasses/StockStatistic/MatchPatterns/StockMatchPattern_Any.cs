namespace StockAnalyzer.StockClasses.StockStatistic.MatchPatterns
{
    public class StockMatchPattern_Any : IStockMatchPattern
    {
        public bool MatchPattern(StockSerie stockSerie, int index)
        {
            return true;
        }

        public string Suffix
        {
            get { return "ANY"; }
        }
    }
}