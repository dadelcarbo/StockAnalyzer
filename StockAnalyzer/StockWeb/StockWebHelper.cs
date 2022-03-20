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
        private bool DownloadFile(string destFile, string url)
        {
            if (File.GetLastWriteTime(destFile) > DateTime.Now.AddHours(-6))
            {
                return true;
            }
            bool success = true;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();

            if (responseFromServer.Length != 0)
            {
                // Save content to file
                StreamWriter writer = new StreamWriter(destFile);
                writer.Write(responseFromServer);
                writer.Close();
            }
            else
            {
                success = false;
            }

            // Cleanup the streams
            reader.Close();
            dataStream.Close();
            // Close the reponse
            response.Close();

            return success;
        }
        #endregion
        #region Investing.com
        private static string urlTemplate = $"{StockDataProviderBase.URL_PREFIX_INVESTING}/search?limit=30&query=%SEARCHTEXT%&type=&exchange=%EXCHANGE%";
        
        private static HttpClient httpClient;

        public IEnumerable<StockDetails> GetInvestingStockDetails(string searchText, string exchange = "")
        {
            string url = urlTemplate.Replace("%SEARCHTEXT%", searchText);
            url = url.Replace("%EXCHANGE%", exchange);

            try
            {
                // Request information
                if (httpClient == null) httpClient = new HttpClient();

                var result = httpClient.GetStringAsync(url).GetAwaiter().GetResult();

                // Parse response
                if (string.IsNullOrWhiteSpace(result) || result == "[]") return new List<StockDetails>(); ;

                return StockDetails.FromJson(result);

            }
            catch (Exception ex)
            {
                throw new StockWebHelperException($"Error getting stock details for {searchText}", ex);
            }
        }
        #endregion
    }
}