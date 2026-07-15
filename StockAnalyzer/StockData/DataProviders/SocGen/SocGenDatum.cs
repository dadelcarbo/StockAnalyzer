using System;

namespace StockAnalyzer.StockData.DataProviders.SocGen
{
    public class SocGenDatum
    {
        public float Ask { get; set; }
        public float Bid { get; set; }
        public DateTime Date { get; set; }
        public float UnderlyingPrice { get; set; }
    }
}
