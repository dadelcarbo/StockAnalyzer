using Newtonsoft.Json;
using StockAnalyzer.StockWeb;
using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockDataProviders.Yahoo
{
    public class Chart
    {
        public List<Result> result { get; set; }
        public string error { get; set; }
    }

    public class CurrentTradingPeriod
    {
        public Pre pre { get; set; }
        public Regular regular { get; set; }
        public Post post { get; set; }
    }

    public class Indicators
    {
        public List<Quote> quote { get; set; }
    }

    public class Meta
    {
        public string currency { get; set; }
        public string symbol { get; set; }
        public string exchangeName { get; set; }
        public string instrumentType { get; set; }
        public int firstTradeDate { get; set; }
        public int regularMarketTime { get; set; }
        public int gmtoffset { get; set; }
        public string timezone { get; set; }
        public string exchangeTimezoneName { get; set; }
        public double regularMarketPrice { get; set; }
        public double chartPreviousClose { get; set; }
        public double previousClose { get; set; }
        public int scale { get; set; }
        public int priceHint { get; set; }
        public CurrentTradingPeriod currentTradingPeriod { get; set; }
        // public TradingPeriods tradingPeriods { get; set; }
        public string dataGranularity { get; set; }
        public string range { get; set; }
        public List<string> validRanges { get; set; }
    }

    public class Post
    {
        public string timezone { get; set; }
        public int start { get; set; }
        public int end { get; set; }
        public int gmtoffset { get; set; }
    }

    public class Pre
    {
        public string timezone { get; set; }
        public int start { get; set; }
        public int end { get; set; }
        public int gmtoffset { get; set; }
    }

    public class Quote
    {
        public List<long?> volume { get; set; }
        public List<double?> high { get; set; }
        public List<double?> close { get; set; }
        public List<double?> open { get; set; }
        public List<double?> low { get; set; }
    }

    public class Regular
    {
        public string timezone { get; set; }
        public int start { get; set; }
        public int end { get; set; }
        public int gmtoffset { get; set; }
    }

    public class Result
    {
        public Meta meta { get; set; }
        public List<int> timestamp { get; set; }
        public Indicators indicators { get; set; }
    }

    public class YahooJson
    {
        public Chart chart { get; set; }
        public static YahooJson FromJson(string json) => JsonConvert.DeserializeObject<YahooJson>(json, Converter.Settings);
    }

    //public class TradingPeriods
    //{
    //    public List<List<>> pre { get; set; }
    //    public List<List<>> post { get; set; }
    //    public List<List<>> regular { get; set; }

    //}


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class QuoteResult
    {
        public string exchange { get; set; }
        public string shortname { get; set; }
        public string quoteType { get; set; }
        public string symbol { get; set; }
        public string index { get; set; }
        public double score { get; set; }
        public string typeDisp { get; set; }
        public string longname { get; set; }
        public string exchDisp { get; set; }
        public string sector { get; set; }
        public string industry { get; set; }
        public bool dispSecIndFlag { get; set; }
        public bool isYahooFinance { get; set; }
        public string name { get; set; }
        public string permalink { get; set; }
    }

    public class ResearchReport
    {
        public string reportHeadline { get; set; }
        public string author { get; set; }
        public object reportDate { get; set; }
        public string id { get; set; }
        public string provider { get; set; }
    }

    public class YahooSearchResult
    {
        public List<object> explains { get; set; }
        public int count { get; set; }
        public List<QuoteResult> quotes { get; set; }

        public static YahooSearchResult FromJson(string json) => JsonConvert.DeserializeObject<YahooSearchResult>(json, Converter.Settings);
    }


    public class YahooQuoteResult
    {
        public QuoteResponse quoteResponse { get; set; }
        public static YahooQuoteResult FromJson(string json) => JsonConvert.DeserializeObject<YahooQuoteResult>(json, Converter.Settings);
    }

    public class QuoteResponse
    {
        public QuoteDetails[] result { get; set; }
        public string error { get; set; }
    }

    public class QuoteDetails
    {
        public string language { get; set; }
        public string region { get; set; }
        public string quoteType { get; set; }
        public string typeDisp { get; set; }
        public string quoteSourceName { get; set; }
        public bool triggerable { get; set; }
        public string customPriceAlertConfidence { get; set; }
        public string marketState { get; set; }
        public string messageBoardId { get; set; }
        public string market { get; set; }
        public float regularMarketChangePercent { get; set; }
        public string currency { get; set; }
        public int gmtOffSetMilliseconds { get; set; }
        public string exchange { get; set; }
        public string exchangeTimezoneShortName { get; set; }
        public string exchangeTimezoneName { get; set; }
        public bool esgPopulated { get; set; }
        public float regularMarketPrice { get; set; }
        public string shortName { get; set; }
        public string longName { get; set; }
        public long firstTradeDateMilliseconds { get; set; }
        public int priceHint { get; set; }
        public float postMarketChangePercent { get; set; }
        public int postMarketTime { get; set; }
        public float postMarketPrice { get; set; }
        public float postMarketChange { get; set; }
        public float regularMarketChange { get; set; }
        public int regularMarketTime { get; set; }
        public float regularMarketDayHigh { get; set; }
        public string regularMarketDayRange { get; set; }
        public float regularMarketDayLow { get; set; }
        public long regularMarketVolume { get; set; }
        public float regularMarketPreviousClose { get; set; }
        public float bid { get; set; }
        public float ask { get; set; }
        public int bidSize { get; set; }
        public int askSize { get; set; }
        public string fullExchangeName { get; set; }
        public string financialCurrency { get; set; }
        public float regularMarketOpen { get; set; }
        public long averageDailyVolume3Month { get; set; }
        public long averageDailyVolume10Day { get; set; }
        public float fiftyTwoWeekLowChange { get; set; }
        public float fiftyTwoWeekLowChangePercent { get; set; }
        public string fiftyTwoWeekRange { get; set; }
        public float fiftyTwoWeekHighChange { get; set; }
        public float fiftyTwoWeekHighChangePercent { get; set; }
        public float fiftyTwoWeekLow { get; set; }
        public float fiftyTwoWeekHigh { get; set; }
        public int dividendDate { get; set; }
        public int earningsTimestamp { get; set; }
        public int earningsTimestampStart { get; set; }
        public int earningsTimestampEnd { get; set; }
        public float trailingAnnualDividendRate { get; set; }
        public float trailingPE { get; set; }
        public float trailingAnnualDividendYield { get; set; }
        public float epsTrailingTwelveMonths { get; set; }
        public float epsForward { get; set; }
        public float epsCurrentYear { get; set; }
        public float priceEpsCurrentYear { get; set; }
        public long sharesOutstanding { get; set; }
        public float bookValue { get; set; }
        public float fiftyDayAverage { get; set; }
        public float fiftyDayAverageChange { get; set; }
        public float fiftyDayAverageChangePercent { get; set; }
        public float twoHundredDayAverage { get; set; }
        public float twoHundredDayAverageChange { get; set; }
        public float twoHundredDayAverageChangePercent { get; set; }
        public long marketCap { get; set; }
        public float forwardPE { get; set; }
        public float priceToBook { get; set; }
        public int sourceInterval { get; set; }
        public int exchangeDataDelayedBy { get; set; }
        public string averageAnalystRating { get; set; }
        public bool tradeable { get; set; }
        public bool cryptoTradeable { get; set; }
        public string symbol { get; set; }
    }


}
