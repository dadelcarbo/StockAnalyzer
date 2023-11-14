using Newtonsoft.Json;
using StockAnalyzer.StockWeb;
using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockDataProviders.CitiFirst
{

    public class CitiFirstSeries
    {
        public Bid[] Bid { get; set; }
        public static CitiFirstSeries FromJson(string json) => JsonConvert.DeserializeObject<CitiFirstSeries>(json, Converter.Settings);
    }

    public class Bid
    {
        public long x { get; set; }
        public float y { get; set; }
    }
}
