namespace StockAnalyzer.StockPortfolio
{
    public class StockNameMapping
    {
        public StockNameMapping()
        {
            this.Leverage = 1;
        }
        public string SaxoName { get; set; }
        public string StockName { get; set; }
        public float Leverage { get; set; }
    }
}
