using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace StockAnalyzer.StockData.DataProviders.Vontobel
{
    internal class VontobelDataClient : IDataHttpClient
    {
        public string FormatUrl(StockInstrument instrument)
        {
            /// Period specification
            /// 0 - 1D 5 Minutes Bars<br/>
            /// 1 - 2D 5 Minutes Bars<br/>
            /// 2 - 1W 5 Minutes Bars<br/>
            /// 3 - 1M 1 Hour Bars<br/>
            int period = 3;
            return $"https://markets.vontobel.com/api/v1/charts/products/{instrument.Isin}/detail/{period}?c=fr-fr&it=1";

            // https://markets.vontobel.com/api/v1/charts/products/DE000VK7VM44/detail/3?c=fr-fr
        }


        private HttpClient httpClient = null;
        private string HttpGetFromVontobel(string url)
        {
            try
            {
                if (httpClient == null)
                {
                    var handler = new HttpClientHandler();
                    handler.AutomaticDecompression = ~DecompressionMethods.None;

                    httpClient = new HttpClient(handler);
                }
                using var request = new HttpRequestMessage();
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(url);
                var response = httpClient.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    StockLog.Write("StatusCode: " + response.StatusCode + Environment.NewLine + response);
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            return null;

        }


        protected static readonly DateTime refDate = new DateTime(1970, 01, 01);
        public StockDailyValue[] GetData(StockInstrument instrument, DateTime startDate)
        {
            try
            {
                var url = FormatUrl(instrument);

                var jsonData = HttpGetFromVontobel(url);
                if (jsonData == null)
                    return null;

                var vontobelData = JsonSerializer.Deserialize<VontobelJSon>(jsonData);

                if (vontobelData == null || !vontobelData.isSuccess || vontobelData.payload == null)
                    return null;

                var startDateUnix = (startDate - refDate).TotalMilliseconds;
                var newBars = vontobelData.payload.series[0].points.Reverse().
                    Where(bar => bar.timestamp > startDateUnix).
                    Select(bar => new StockDailyValue(bar.bid, bar.bid, bar.bid, bar.bid, 0, refDate.AddMilliseconds(bar.timestamp).ToLocalTime()))
                    .ToArray();
                if (newBars.Count() > 0)
                {
                    return newBars;
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            return null;
        }
    }
}