using Newtonsoft.Json;
using StockAnalyzer.StockWeb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockClasses.StockDataProviders.CitiFirst
{
    public class AdditionalInfo
    {
        public double minY { get; set; }
        public double maxY { get; set; }
        public string id { get; set; }
    }

    public class Datum
    {
        public long x { get; set; }
        public float? y { get; set; }
    }

    public class CitiFirstSeries
    {
        public List<Series> series { get; set; }

        public static CitiFirstSeries FromJson(string json) => JsonConvert.DeserializeObject<CitiFirstSeries>(json, Converter.Settings);
    }

    public class Series
    {
        public List<Datum> data { get; set; }
        public AdditionalInfo additionalInfo { get; set; }
    }
}
