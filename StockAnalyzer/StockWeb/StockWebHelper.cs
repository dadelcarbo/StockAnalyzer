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
                var handler = new HttpClientHandler();

                // If you are using .NET Core 3.0+ you can replace `~DecompressionMethods.None` to `DecompressionMethods.All`
                handler.AutomaticDecompression = ~DecompressionMethods.None;
                handler.ServerCertificateCustomValidationCallback = (requestMessage, certificate, chain, policyErrors) => true;
                httpClient = new HttpClient(handler);
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


                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    request.Headers.TryAddWithoutValidation("Accept-Language", "en-GB,en;q=0.9,fr;q=0.8");
                    request.Headers.TryAddWithoutValidation("Cache-Control", "max-age=0");
                    request.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                    request.Headers.TryAddWithoutValidation("If-Modified-Since", "Sat, 15 Oct 2022 01:48:54 GMT");
                    request.Headers.TryAddWithoutValidation("If-None-Match", "^^");
                    request.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
                    request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36");

                    var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
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
                var response = InvestingIntradayDataProvider.HttpGetFromInvesting(url);
                if (!string.IsNullOrEmpty(response))
                {
                    if (string.IsNullOrWhiteSpace(response) || response == "[]") return new List<StockDetails>(); ;

                    return StockDetails.FromJson(response);
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