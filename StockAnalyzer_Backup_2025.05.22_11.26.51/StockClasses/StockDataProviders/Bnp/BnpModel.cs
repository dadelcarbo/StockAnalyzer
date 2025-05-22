namespace StockAnalyzer.StockClasses.StockDataProviders.Bnp
{

    public class BnpResponse
    {
        public Datum[] Data { get; set; }
    }

    public class Datum
    {
        public string id { get; set; }
        public string labelId { get; set; }
        public float[][] ticks { get; set; }
        public string type { get; set; }
    }

}
