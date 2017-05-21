using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockClasses.StockDataProviders;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace StockAnalyzerTest
{
    [TestClass]
    public class StockYahooJasonTest
    {
        [TestMethod]
        [DeploymentItem(@"YahooJson\EURGBP=X_INT_FX_EUR_GBP_INTRADAY.json")]
        public void ParseJSon()
        {
            YahooIntradayDataProvider dp = new YahooIntradayDataProvider();

            using (var fs = File.OpenRead(@"EURGBP=X_INT_FX_EUR_GBP_INTRADAY.json"))
            {
                string csv = dp.YahooJsonToCSV(fs);
            }
        }

        [TestMethod]
        public void DownloadYahooIntraday()
        {
            string url = "https://l1-query.finance.yahoo.com/v8/finance/chart/SPY?period2=1495228978&period1=1494883378&interval=1m&indicators=quote&includeTimestamps=true&includePrePost=false&events=div%7Csplit%7Cearn&corsDomain=finance.yahoo.com";

            using (WebClient wc = new WebClient())
            {
                wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

                var data = wc.DownloadString(url);

                YahooIntradayDataProvider dp = new YahooIntradayDataProvider();

                string csv = dp.YahooJsonToCSV(new MemoryStream(Encoding.UTF8.GetBytes(data)));
                Assert.AreNotEqual(string.Empty, csv);
            }
        }

        [TestMethod]
        public void DownloadYahooDailyTest()
        {
            string url = "https://fr.finance.yahoo.com/quote/%5EFCHI/history?p=%5EFCHI";

            InitCookies(url);

            DumpCookies();



            //"ucs=eup=1&pnid=&pnct=";
            //"B=0fk65f1bovnn1&b=3&s=to";
            //"PRF=%3Dundefined%26t%3D%255EFCHI%252BAC.PA%252BPPLT%252BCL%253DF%252BSPY%252BEURGBP%253DX%252B%255EGSPC%252BEWG%252BSPY170421P00230500%252BSPY170419C00230500%252BAAPL170407C00144000%252BAAPL%252BAAPL170421C00145000%252BAAPL170421C00155000%252BAAPL170407C00110000";

        }
        private static void DumpCookies()
        {
            foreach (var cookie in Cookies)
            {
                Console.WriteLine(cookie.ToString());
            }
        }
        private static List<Cookie> Cookies { get; set; }

        private static void InitCookies(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.CookieContainer = new CookieContainer();
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "Get";
            req.AllowAutoRedirect = true;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.Headers.Add("Accept-Language", "fr,fr-fr;q=0.8,en-us;q=0.5,en;q=0.3");
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.63 Safari/537.36";
            req.Referer = url;
            req.KeepAlive = true;

            bool success = false;
            int tries = 3;
            while (!success && tries > 0)
            {
                tries--;
                try
                {
                    HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                    {
                        // Get the stream containing content returned by the server.
                        using (Stream dataStream = response.GetResponseStream())
                        {
                            using (StreamReader reader = new StreamReader(dataStream))
                            {
                                Cookies = new List<Cookie>();
                                foreach (var cookie in response.Cookies)
                                {
                                    Cookies.Add(cookie as Cookie);
                                }
                            }
                            string url2 = "https://query1.finance.yahoo.com/v7/finance/download/%5EFCHI?period1=1492717796&period2=1495309796&interval=1d&events=history&crumb=sb83SE0p84J";
                            HttpWebRequest req2 = (HttpWebRequest)WebRequest.Create(url2);

                            req2.CookieContainer = new CookieContainer();
                            req2.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                            req2.Headers.Add("accept-encoding","gzip, deflate, sdch, br");
                            req2.Headers.Add("accept-language","en-US,en;q=0.8,fr;q=0.6");
                            req2.Headers.Add("upgrade-insecure-requests","1");
                            req2.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.63 Safari/537.36";
                            req2.Referer = url;
                            req2.Headers.Add("accept-language", "en-US,en;q=0.8,fr;q=0.6");
                            foreach (Cookie cookie in Cookies)
                            {
                                req2.CookieContainer.Add(cookie);
                            }

                            var response2 = req2.GetResponse();
                        }
                    }
                }
                catch (Exception) { }
            }
        }

    }
}
