using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class TradingPeriod
    {
        public string timezone { get; set; }
        public int end { get; set; }
        public int start { get; set; }
        public int gmtoffset { get; set; }
    }

    public class CurrentTradingPeriod
    {
        public TradingPeriod pre { get; set; }
        public TradingPeriod regular { get; set; }
        public TradingPeriod post { get; set; }
    }

    public class TradingPeriods
    {
        public List<List<int>> pre { get; set; }
        public List<List<int>> post { get; set; }
        public List<List<int>> regular { get; set; }
    }

    public class Meta
    {
        public string currency { get; set; }
        public string symbol { get; set; }
        public string exchangeName { get; set; }
        public string instrumentType { get; set; }
        public int firstTradeDate { get; set; }
        public int gmtoffset { get; set; }
        public string timezone { get; set; }
        public string exchangeTimezoneName { get; set; }
        public double previousClose { get; set; }
        public int scale { get; set; }
        public CurrentTradingPeriod currentTradingPeriod { get; set; }
        //public TradingPeriods tradingPeriods { get; set; }
        public string dataGranularity { get; set; }
        public List<string> validRanges { get; set; }
    }

    public class Quote
    {
        public float?[] low { get; set; }
        public float?[] high { get; set; }
        public float?[] close { get; set; }
        public float?[] open { get; set; }
        public long?[] volume { get; set; }
    }

    public class Indicators
    {
        public List<Quote> quote { get; set; }
    }

    public class Result
    {
        public Meta meta { get; set; }
        public List<long> timestamp { get; set; }
        public Indicators indicators { get; set; }
    }

    public class Chart
    {
        public List<Result> result { get; set; }
        //public object error { get; set; }
    }

    public class RootObject
    {
        public Chart chart { get; set; }
    }
}