using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace StockAnalyzer.StockData.DataProviders.Cnn
{
    internal class CnnDataClient : IDataHttpClient
    {
        protected static readonly DateTime refDate = new DateTime(1970, 01, 01);
        public StockDailyValue[] GetData(StockInstrument instrument, DateTime startDate)
        {
            try
            {
                var json = HttpGet($"https://production.dataviz.cnn.io/index/{instrument.Id.ToLower()}/graphdata");
                if (string.IsNullOrEmpty(json))
                    return null;

                var data = JsonSerializer.Deserialize<FearAndGreedData>(json.Replace("0.0,", "0,"));
                if (data?.fear_and_greed_historical?.data == null || data.fear_and_greed_historical.data.Length == 0)
                    return null;

                var refMS = (startDate - refDate).Ticks / (TimeSpan.TicksPerMillisecond);

                var newBars = data.fear_and_greed_historical.data
                    .Where(d => d.x > refMS)
                    .Select(d => new StockDailyValue(d.y, d.y, d.y, d.y, 0, refDate.AddMilliseconds(d.x).Date));

                if (newBars.Any())
                {
                    // Remove duplicate entries that share the same DATE (keep the first occurrence)
                    var newBarArray = newBars.GroupBy(b => b.DATE).Select(g => g.First()).ToArray();

                    var lastBar = newBarArray[newBarArray.Length - 1];
                    if (lastBar.DATE == DateTime.Today && MarketHours.MarketHoursTable[instrument.Market].IsOpened)
                    {
                        lastBar.IsComplete = false;
                    }

                    return newBarArray;
                }
            }
            catch (Exception e)
            {
                StockLog.Write("Unable to parse daily data for " + instrument.DisplayName);
                StockLog.Write(e);
            }
            return null;
        }


        private HttpClient httpClient = null;
        private string HttpGet(string url)
        {
            try
            {
                if (httpClient == null)
                {
                    var handler = new HttpClientHandler();
                    handler.AutomaticDecompression = ~DecompressionMethods.None;
                    httpClient = new HttpClient(handler);

                    httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                    httpClient.DefaultRequestHeaders.Add("Pragma", "no-cache");
                }
                using var request = new HttpRequestMessage(new HttpMethod("GET"), "https://production.dataviz.cnn.io/index/fearandgreed/graphdata");
                request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                request.Headers.TryAddWithoutValidation("accept-language", "fr,fr-FR;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
                request.Headers.TryAddWithoutValidation("if-none-match", "W/9218406070119492977");
                request.Headers.TryAddWithoutValidation("priority", "u=0, i");
                request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Microsoft Edge\";v=\"125\", \"Chromium\";v=\"125\", \"Not.A/Brand\";v=\"24\"");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                request.Headers.TryAddWithoutValidation("sec-fetch-site", "none");
                request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
                request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0");

                var task = httpClient.SendAsync(request);
                var response = task.Result;

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
    }
}
