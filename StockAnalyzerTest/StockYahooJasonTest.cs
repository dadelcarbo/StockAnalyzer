using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace StockAnalyzerTest
{
    [TestClass]
    public class StockYahooJasonTest
    {
        //[TestMethod]
        //[DeploymentItem(@"YahooJson\EURGBP=X_INT_FX_EUR_GBP_INTRADAY.json")]
        //public void ParseJSon()
        //{
        //    //YahooIntradayDataProvider dp = new YahooIntradayDataProvider();

        //    //using (var fs = File.OpenRead(@"EURGBP=X_INT_FX_EUR_GBP_INTRADAY.json"))
        //    //{
        //    //    string csv = dp.YahooJsonToCSV(fs);
        //    //}
        //}

        //[TestMethod]
        //public void DownloadYahooIntraday()
        //{
        //    string url = "https://l1-query.finance.yahoo.com/v8/finance/chart/SPY?period2=1495228978&period1=1494883378&interval=1m&indicators=quote&includeTimestamps=true&includePrePost=false&events=div%7Csplit%7Cearn&corsDomain=finance.yahoo.com";

        //    using (WebClient wc = new WebClient())
        //    {
        //        wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

        //        var data = wc.DownloadString(url);

        //        YahooIntradayDataProvider dp = new YahooIntradayDataProvider();

        //        string csv = dp.YahooJsonToCSV(new MemoryStream(Encoding.UTF8.GetBytes(data)));
        //        Assert.AreNotEqual(string.Empty, csv);
        //    }
        //}

    }
}
