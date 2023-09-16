using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog
{
    #region Underlying
    public class Entry
    {
        public string key { get; set; }
        public string title { get; set; }
        public List<Entry> entries { get; set; }
        public string value { get; set; }
        public string url { get; set; }
    }

    public class SaxoUnderlyings
    {
        public List<Entry> entries { get; set; }
    }

    #endregion
    #region Product
    public class Ask
    {
        public Value value { get; set; }
        public string type { get; set; }
        public int precisionMin { get; set; }
        public int precisionMax { get; set; }
        public bool pushable { get; set; }
        // public PushableMetadata pushableMetadata { get; set; }
    }

    public class Bid
    {
        public Value value { get; set; }
        public string type { get; set; }
        public int precisionMin { get; set; }
        public int precisionMax { get; set; }
        public bool pushable { get; set; }
        //public PushableMetadata pushableMetadata { get; set; }
    }

    public class Data
    {
        public Groups groups { get; set; }
        public Pagination pagination { get; set; }
    }

    public class Groups
    {
        public List<Product> products { get; set; }
    }

    public class Isin
    {
        public string value { get; set; }
        public string type { get; set; }
        public bool pushable { get; set; }
    }

    public class Leverage
    {
        public string type { get; set; }
        public string value { get; set; }
        public int precisionMin { get; set; }
        public int precisionMax { get; set; }
        public string currency { get; set; }
        public bool pushable { get; set; }
        //public PushableMetadata pushableMetadata { get; set; }
    }

    public class Name
    {
        public string value { get; set; }
        public string type { get; set; }
        public bool pushable { get; set; }
    }

    public class Pagination
    {
        public int page { get; set; }
        public int rowsPerPage { get; set; }
        public int filteredCount { get; set; }
        public int totalCount { get; set; }
        public List<int> options { get; set; }
    }

    public class Product
    {
        public RatioCalculated ratioCalculated { get; set; }
        public RealPriceCurrency realPriceCurrency { get; set; }
        public Leverage leverage { get; set; }
        public Name name { get; set; }
        public Ask ask { get; set; }
        public Underlying underlying { get; set; }
        public Type type { get; set; }
        public Bid bid { get; set; }
        public Isin isin { get; set; }
    }

    public class PushableMetadata
    {
        public Value value { get; set; }
        public int sin { get; set; }
        public int fieldId { get; set; }
        public int bestMarket { get; set; }
        public string valueRender { get; set; }
        public bool pushable { get; set; }
        public string valueFormat { get; set; }
        public string timestampFormat { get; set; }
        public string sizeFormat { get; set; }
        public int referenceSin { get; set; }
        public long timestamp { get; set; }
        public int size { get; set; }
    }

    public class RatioCalculated
    {
        public double value { get; set; }
        public string type { get; set; }
        public int precisionMin { get; set; }
        public int precisionMax { get; set; }
        public string currency { get; set; }
        public bool pushable { get; set; }
    }

    public class RealPriceCurrency
    {
        public string value { get; set; }
        public string type { get; set; }
        public bool pushable { get; set; }
    }

    public class SaxoResult
    {
        public Data data { get; set; }
        public string status { get; set; }
    }

    public class Type
    {
        public string value { get; set; }
        public string type { get; set; }
        public bool pushable { get; set; }
    }

    public class Underlying
    {
        public string value { get; set; }
        public string type { get; set; }
        public bool pushable { get; set; }
    }

    public class Value
    {
        public string value { get; set; }
        public long timestamp { get; set; }
        public int size { get; set; }
        public int sin { get; set; }
        public int fieldId { get; set; }
        public int bestMarket { get; set; }
        public string valueRender { get; set; }
        public bool pushable { get; set; }
        public string valueFormat { get; set; }
        public string timestampFormat { get; set; }
        public string sizeFormat { get; set; }
    }


    #endregion
}
