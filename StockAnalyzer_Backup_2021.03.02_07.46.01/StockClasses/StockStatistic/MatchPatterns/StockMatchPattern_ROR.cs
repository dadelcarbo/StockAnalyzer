namespace StockAnalyzer.StockClasses.StockStatistic.MatchPatterns
{
    public class StockMatchPattern_ROR : IStockMatchPattern
    {
        public StockMatchPattern_ROR(float trigger)
        {
            this.Trigger = trigger;
        }

        public float Trigger { get; set; }

        int lookback= 50;

        public bool MatchPattern(StockSerie stockSerie, int index)
        {
            if (index < lookback) return false;
            var ror = stockSerie.GetIndicator("ROR("+lookback+")").Series[1];
            for (int i = index - lookback; i < index; i++)
            {
                if (ror[i] > Trigger) return false;
            }
            return ror[index]>Trigger;
        }

        public string Suffix
        {
            get { return "ROR(" + lookback + ")"; }
        }
    }
}