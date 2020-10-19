using System;
using System.IO;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockWeb;

namespace StockAnalyzerTest
{
    [TestClass]
    public class StockWebTest
    {
        [TestMethod]
        public void YahooDividendDownload()
        {
            var startDate = new DateTime(2000, 1, 1);
            var endDate = DateTime.Today;

            DateTime refDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var startSecond = (startDate - refDate).TotalSeconds;
            var endSecond = (startDate - refDate).TotalSeconds;

            var url = "https://query1.finance.yahoo.com/v7/finance/download/FP.PA?period1=946857600&period2=1588377600&interval=1mo&events=div";
            //var url = "https://query1.finance.yahoo.com/v7/finance/download/SEL.PA?period1=1339977600&period2=1588377600&interval=1d&events=div";
            var webHelper = new StockWebHelper();

            var fileName = "dividend.csv";
            Assert.IsTrue(webHelper.DownloadFile(".", fileName, url));

            Assert.IsTrue(File.Exists(fileName));
        }
        [TestMethod]
        public void SocGenDownload()
        {
            string ticker = "802022";
            var url = $"https://sgbourse.fr/EmcWebApi/api/Prices/Intraday?productId={ticker}";

            // allows for validation of SSL conversations
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls12
                | SecurityProtocolType.Ssl3;

            // You must change the URL to point to your Web server.
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            //req.Headers.Add("Upgrade-Insecure-Requests", "1");

            WebResponse respon = req.GetResponse();
            Stream res = respon.GetResponseStream();

            StreamReader reader = new StreamReader(res);

            // Read the content.
            var data = reader.ReadToEnd();

            Assert.IsNotNull(data);
        }
    }
}
