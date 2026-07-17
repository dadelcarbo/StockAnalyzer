using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders.Yahoo;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace StockAnalyzer.StockData.DataProviders.Yahoo
{
    internal class YahooDataClient : IDataHttpClient
    {
        public string FormatURL(string symbol, DateTime startDate, DateTime endDate, string interval = "1d")
        {
            var startTime = (int)(startDate - refDate).TotalSeconds;
            var endTime = (int)(endDate - refDate).TotalSeconds;

            return $"https://query2.finance.yahoo.com/v8/finance/chart/{symbol}?period1={startTime}&period2={endTime}&interval={interval}&includePrePost=false&events=div%7Csplit%7Cearn&lang=en-US&region=US";
        }

        private HttpClient httpClient = null;
        public string HttpGetFromYahoo(string url, string symbol)
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
                request.Headers.TryAddWithoutValidation("authority", "query1.finance.yahoo.com");
                request.Headers.TryAddWithoutValidation("accept", "*/*");
                request.Headers.TryAddWithoutValidation("accept-language", "en-GB,en;q=0.9,fr;q=0.8");
                request.Headers.TryAddWithoutValidation("cookie", "GUC=AQABBwFjUTNjgUIjOQUK&s=AQAAAHQwhHqy&g=Y0_klA; A1=d=AQABBDrgTGECEJASw6tugP5gurwL3tTZJbsFEgABBwEzUWOBY-Uzb2UB9qMAAAcIOuBMYdTZJbs&S=AQAAAtchReKAiYT-dd18q6pme9o; A3=d=AQABBDrgTGECEJASw6tugP5gurwL3tTZJbsFEgABBwEzUWOBY-Uzb2UB9qMAAAcIOuBMYdTZJbs&S=AQAAAtchReKAiYT-dd18q6pme9o; EuConsent=CPP_3gAPP_3gAAOACBFRClCoAP_AAH_AACiQIjNd_Hf_bX9n-f596ft0eY1f9_r3ruQzDhfNk-8F2L_W_LwX_2E7NB36pq4KmR4ku1LBIQNtHMnUDUmxaokVrzHsak2MpyNKJ7BkknsZe2dYGFtPm5lD-QKZ7_5_d3f52T_9_9v-39z33913v3d93-_12LjdV591H_v9fR_bc_Kdt_5-AAAAAAAAEEEQCTDEvIAuxLHAk0DSqFECMKwkKgFABBQDC0TWADA4KdlYBHqCFgAhNQEYEQIMQUYEAgAEAgCQiACQAsEACAIgEAAIAUICEABAwCCwAsDAIAAQDQMQAoABAkIMjgqOUwICIFogJbKwBKKqY0wgDLLACgERkVEAiAIAEgICAsHEMASAlADDQAYAAgkEIgAwABBIIVABgACCQQA; thamba=2; cmp=t=1666643858&j=1&u=1---&v=56; A1S=d=AQABBDrgTGECEJASw6tugP5gurwL3tTZJbsFEgABBwEzUWOBY-Uzb2UB9qMAAAcIOuBMYdTZJbs&S=AQAAAtchReKAiYT-dd18q6pme9o&j=GDPR; PRF=t^%^3DTSLA^%^252BAAPL^%^252B^%^255EFCHI^%^252BMC.PA^%^252BES^%^253DF^%^252BNQ^%^253DF^%^252BSI^%^253DF^%^252BCC^%^253DF^%^252BADYEN.AS^%^252BAVT.PA^%^252BAC.PA");
                request.Headers.TryAddWithoutValidation("origin", "https://finance.yahoo.com");
                request.Headers.TryAddWithoutValidation("referer", $"https://finance.yahoo.com/quote/{symbol}/chart?p={symbol}");
                request.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                request.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
                request.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
                request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-site");
                request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36");

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
                var url = FormatURL(instrument.Symbol, startDate, DateTime.Today);

                var response = HttpGetFromYahoo(url, instrument.Symbol);
                if (string.IsNullOrEmpty(response))
                    return null;

                var yahooJson = YahooJson.FromJson(response);
                if (!string.IsNullOrEmpty(yahooJson?.chart?.error))
                {
                    StockLog.Write($"Error loading {instrument.DisplayName}: {yahooJson?.chart?.error}");
                }

                int i = 0;
                var priceHint = yahooJson.chart.result[0].meta.priceHint;
                var quote = yahooJson.chart.result[0].indicators.quote[0];
                var newBars = new List<StockDailyValue>();
                foreach (var timestamp in yahooJson.chart.result[0].timestamp)
                {
                    if (quote.open[i] == null || quote.high[i] == null || quote.low[i] == null || quote.close[i] == null)
                    {
                        i++;
                        continue;
                    }
                    var openDate = refDate.AddSeconds(timestamp).Date;
                    long vol = quote.volume[i].HasValue ? quote.volume[i].Value : 0;
                    var dailyValue = new StockDailyValue(
                           (float)Math.Round(quote.open[i].Value, priceHint),
                           (float)Math.Round(quote.high[i].Value, priceHint),
                           (float)Math.Round(quote.low[i].Value, priceHint),
                           (float)Math.Round(quote.close[i].Value, priceHint),
                           vol,
                           openDate);

                    newBars.Add(dailyValue);
                    i++;
                }

                if (newBars.Count() > 0)
                {
                    return newBars.ToArray();
                }
            }
            catch (Exception e)
            {
                StockLog.Write("Unable to parse daily data for " + instrument.DisplayName);
                StockLog.Write(e);
            }
            return null;
        }

        public string FormatUrl(StockInstrument instrument)
        {
            throw new NotImplementedException();
        }
    }
}