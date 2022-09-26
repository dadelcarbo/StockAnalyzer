using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockWeb
{
    public class StockWebHelperException : Exception
    {
        public StockWebHelperException(string msg, Exception e)
            : base(msg, e)
        {
        }
    }
    public class StockWebHelper
    {
        public delegate void DownloadingStockEventHandler(string text);

        public StockWebHelper()
        {
        }
        #region Download Entry Points
        public bool DownloadFile(string destFolder, string fileName, string url)
        {
            try
            {
                if (!System.IO.Directory.Exists(destFolder))
                {
                    System.IO.Directory.CreateDirectory(destFolder);
                }

                if (!DownloadFile(destFolder + @"\" + fileName, url))
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                StockLog.Write(e);
                return false;
            }
            return true;
        }
        public string DownloadHtml(string url, Encoding encoding)
        {
            if (encoding == null) encoding = Encoding.GetEncoding("ISO-8859-15");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream, encoding);
            // Read the content.
            return reader.ReadToEnd();
        }
        public static string DownloadData(string url)
        {
            // allows for validation of SSL conversations
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls12
                | SecurityProtocolType.Ssl3;

            // You must change the URL to point to your Web server.
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            StreamReader reader = new StreamReader(req.GetResponse().GetResponseStream());
            return reader.ReadToEnd();
        }

        static HttpClient httpClient;
        static void InitWebClient()
        {
            if (httpClient == null)
            {
                httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            }
        }
        public static bool DownloadFile(string destFile, string url)
        {
            bool success = true;
            try
            {
                if (File.GetLastWriteTime(destFile) > DateTime.Now.AddHours(-6))
                {
                    return true;
                }
                InitWebClient();
                var response = httpClient.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    // Save content to file
                    var writer = new FileStream(destFile, FileMode.Create);
                    response.Content.CopyToAsync(writer).Wait();
                    writer.Close();
                }
                else
                {
                    success = false;
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
                success = false;
            }
            return success;
        }
        #endregion
        #region Investing.com

        public IEnumerable<StockDetails> GetInvestingStockDetails(string searchText, string exchange = "")
        {
            string urlTemplate = $"{StockDataProviderBase.URL_PREFIX_INVESTING}/search?limit=30&query=%SEARCHTEXT%&type=&exchange=%EXCHANGE%";
            string url = urlTemplate.Replace("%SEARCHTEXT%", searchText);
            url = url.Replace("%EXCHANGE%", exchange);

            try
            {
                var response =  InvestingIntradayDataProvider.HttpGet(url);
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    // Parse response
                    if (string.IsNullOrWhiteSpace(result) || result == "[]") return new List<StockDetails>(); ;

                    return StockDetails.FromJson(result);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new StockWebHelperException($"Error getting stock details for {searchText}", ex);
            }
        }
        #endregion
    }
}