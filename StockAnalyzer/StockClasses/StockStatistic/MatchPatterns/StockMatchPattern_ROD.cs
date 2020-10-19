namespace StockAnalyzer.StockClasses.StockStatistic.MatchPatterns
{
    public class StockMatchPattern_ROD : IStockMatchPattern
    {
        public StockMatchPattern_ROD(float trigger)
        {
            this.Trigger = trigger;
        }

        public float Trigger { get; set; }

        int lookback = 50;
        public bool MatchPattern(StockSerie stockSerie, int index)
        {
            if (index < lookback) return false;
            var rod = stockSerie.GetIndicator("ROD(" + lookback + ",1)").Series[1];
            for (int i = index - lookback; i < index; i++)
            {
                if (rod[i] > Trigger) return false;
            }
            return rod[index] > Trigger;
        }

        public string Suffix
        {
            get { return "ROD(" + lookback + ",1)"; }
        }
    }
}