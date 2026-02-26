

namespace StockAnalyzer.StockClasses.StockDataProviders.Bourso
{

    public class BoursoJson
    {
        public Datum d { get; set; }
    }
    public class Datum
    {
        public string Name { get; set; }
        public string SymbolId { get; set; }
        public Quotetab[] QuoteTab { get; set; }
    }
    public class Quotetab
    {
        public long d { get; set; }
        public float o { get; set; }
        public float h { get; set; }
        public float l { get; set; }
        public float c { get; set; }
        public int v { get; set; }
    }

}
