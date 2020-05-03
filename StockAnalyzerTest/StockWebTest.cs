using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockAnalyzer.StockClasses;
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
    }
}
