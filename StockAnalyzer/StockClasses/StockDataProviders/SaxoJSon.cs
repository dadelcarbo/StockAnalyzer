using System;
using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockDataProviders.Saxo
{
    public class R
    {
        public DateTime low { get; set; }
        public DateTime high { get; set; }
    }

    public class Datum
    {
        public DateTime x { get; set; }
        public float y { get; set; }
        public float c { get; set; }
        public float h { get; set; }
        public float l { get; set; }
        public R r { get; set; }
    }

    public class Series
    {
        public int sin { get; set; }
        public string name { get; set; }
        public string currency { get; set; }
        public List<Datum> data { get; set; }
    }

    public class Line
    {
        public int sin { get; set; }
        public string id { get; set; }
        public float value { get; set; }
    }
    public class SaxoJSon
    {
        //public string timespan { get; set; }
        public List<Series> series { get; set; }
        // public List<Line> lines { get; set; }
    }
}
