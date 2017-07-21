namespace StockAnalyzer.StockClasses.StockStatistic.MatchPatterns
{
    public class StockMatchPattern_ROD : IStockMatchPattern
    {
        public StockMatchPattern_ROD(float trigger)
        {
            this.Trigger = trigger;
        }

        public float Trigger { get; set; }

        public bool MatchPattern(StockSerie stockSerie, int index)
        {
            var ror = stockSerie.GetIndicator("ROD(50,1,1)").Series[1];
            return ror[index] > Trigger;
        }
    }
}