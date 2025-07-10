using System.Collections.Generic;
using System.Diagnostics;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog
{
    #region Underlying

    public class SaxoUnderlyings
    {
        public List<SearchUnderlying> entries { get; set; }
    }

    #endregion
    #region Product


    public class BidAsk
    {
        public ValueTuple valueTuple { get; set; }
        public string type { get; set; }
        public string valueRender { get; set; }
        public string valueFormat { get; set; }
        public string timestampFormat { get; set; }
        public string sizeFormat { get; set; }
        public bool pushable { get; set; }
        public string subscriptionString { get; set; }

        public override string ToString()
        {
            return valueTuple == null ? "null" : valueTuple.ToString();
        }
    }

    public class ValueTuple
    {
        public double value { get; set; }
        //public float timestamp { get; set; }
        //public float size { get; set; }
        //public float delayMinutes { get; set; }

        public override string ToString()
        {
            return value.ToString();
        }
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

    public class Leverage
    {
        public string type { get; set; }
        public double value { get; set; }
        public int precisionMin { get; set; }
        public int precisionMax { get; set; }
        public string currency { get; set; }
        public bool pushable { get; set; }
        //public PushableMetadata pushableMetadata { get; set; }
    }

    [DebuggerDisplay("{value}")]
    public class StringValue
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
        public StringValue name { get; set; }
        public BidAsk ask { get; set; }
        public StringValue underlying { get; set; }
        public StringValue type { get; set; }
        public BidAsk bid { get; set; }
        public StringValue isin { get; set; }


        public override string ToString()
        {
            return name == null ? "null" : name.ToString();
        }
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


    ////////////////////////////////
    ///


    public class UnderlyingRoot
    {
        public Datum data { get; set; }
        public string status { get; set; }
    }

    public class Datum
    {
        public Filters filters { get; set; }
    }

    public class Filters
    {
        public Firstlevel firstLevel { get; set; }
    }

    public class Firstlevel
    {
        public Underlyings underlying { get; set; }
    }

    public class Underlyings
    {
        public string label { get; set; }
        public string type { get; set; }
        public int order { get; set; }
        public Dictionary<string, SearchUnderlying> list { get; set; }
        public bool showOptionAll { get; set; }
        public bool showActionButtons { get; set; }
    }

    public class SearchUnderlying
    {
        public string label { get; set; }
        public long value { get; set; }
    }

}
