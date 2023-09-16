namespace StockAnalyzer.StockClasses.StockDataProviders.Vontobel
{
    public class VontobelJSon
    {
        public Payload payload { get; set; }
        public bool isSuccess { get; set; }
        public string errorCode { get; set; }
    }

    public class Payload
    {
        public int chartType { get; set; }
        public Xaxis xAxis { get; set; }
        public Yaxi[] yAxis { get; set; }
        public Series[] series { get; set; }
    }

    public class Xaxis
    {
        public long minTimestamp { get; set; }
        public long maxTimestamp { get; set; }
    }

    public class Yaxi
    {
        public int id { get; set; }
        public string currency { get; set; }
        public bool opposite { get; set; }
    }

    public class Series
    {
        public Item item { get; set; }
        public Point[] points { get; set; }
    }

    public class Item
    {
        public int priceType { get; set; }
        public int yAxisId { get; set; }
        public string priceIdentifier { get; set; }
        public int roundDigits { get; set; }
        public string name { get; set; }
    }

    public class Point
    {
        public long timestamp { get; set; }
        public float bid { get; set; }
        public float ask { get; set; }
        public float close { get; set; }
    }

}
