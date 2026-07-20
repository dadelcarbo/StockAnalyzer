using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace StockAnalyzer.StockData.DataProviders.Yahoo
{
    internal class YahooDataClient : IDataHttpClient
    {
        public YahooDataClient()
        {
            Instance = this; // Pseudo singleton
        }
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

        public static YahooDataClient Instance { get; private set; }

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



        public YahooSearchResult SearchFromYahoo(string search)
        {
            try
            {
                if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    var url = $"https://query1.finance.yahoo.com/v1/finance/search?q={search}&lang=en-US&region=US&quotesCount=6&newsCount=3&listsCount=2&enableFuzzyQuery=false&quotesQueryId=tss_match_phrase_query&multiQuoteQueryId=multi_quote_single_token_query&newsQueryId=news_cie_vespa&enableCb=true&enableNavLinks=true&enableEnhancedTrivialQuery=true&enableResearchReports=true&enableCulturalAssets=true&enableLogoUrl=true&recommendCount=5";

                    if (httpClient == null)
                    {
                        var handler = new HttpClientHandler();
                        handler.AutomaticDecompression = ~DecompressionMethods.None;

                        httpClient = new HttpClient(handler);
                    }
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                    {
                        request.Headers.TryAddWithoutValidation("accept", "*/*");
                        request.Headers.TryAddWithoutValidation("accept-language", "fr,fr-FR;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
                        request.Headers.TryAddWithoutValidation("cookie", "axids=gam=y-5J2nTa1E2uKm8p1bbIqpxhNS3sTM5.Ex~A&dv360=eS1tNmVfdElaRTJ1RnJDM2c5ME4xZDlwdHl3SWpDVHlFbX5B&ydsp=y-RCkPhNpE2uJyZDkAH.iHpHvzehVTcIj_~A&tbla=y-pSAoiWFE2uJyQltidu8WhcDOgc5wSCuA~A; tbla_id=54179619-0b64-44db-8b66-35bf83addca6-tuctcfb6c39; GUC=AQABCAFm00Jm_kIZFgP4&s=AQAAAAi6SdKt&g=ZtH2iw; A1=d=AQABBLfmAWYCEJRSJ0er9K5iY9-55MkC5CcFEgABCAFC02b-ZuUzb2UB9qMAAAcIs-YBZmr6HEI&S=AQAAAuzFUXctpl8HLY6VzAlcdsE; A3=d=AQABBLfmAWYCEJRSJ0er9K5iY9-55MkC5CcFEgABCAFC02b-ZuUzb2UB9qMAAAcIs-YBZmr6HEI&S=AQAAAuzFUXctpl8HLY6VzAlcdsE; A1S=d=AQABBLfmAWYCEJRSJ0er9K5iY9-55MkC5CcFEgABCAFC02b-ZuUzb2UB9qMAAAcIs-YBZmr6HEI&S=AQAAAuzFUXctpl8HLY6VzAlcdsE; EuConsent=CP8B3EAP8B3EAAOACBFRBFFoAP_gAEPgACiQJhNB9G7WTXFneXp2YPskOYUX0VBJ4MAwBgCBAcABzBIUIAwGVmAzJEyIICACGAIAIGJBIABtGAhAQEAAYIAFAABIAEEAABAAIGAAACAAAABACAAAAAAAAAAQgEAXMBQgmAZEAFoIQUhAhgAgAQAAAAAEAIgBAgQAEAAAQAAICAAIACgAAgAAAAAAAAAEAFAIEQAAAAECAotkfQTBADINSogCLAkJCIQMIIEAIgoCACgQAAAAECAAAAmCAoQBgEqMBEAIAQAAAAAAAAQEACAAACABCAAIAAgQAAAAAQAAAAACAAAEAAAAAAAAAAAAAAAAAAAAAAAAAMQAhBAACAACAAgoAAAABAAAAAAAAAARAAAAAAAAAAAAAAAAARAAAAAAAAAAAAAAAAAAAQAAAAAAAABAAILAAA; PRF=t%3D1EX.SG%252BEXM.BR%252BMTRK.AS%252BLCOR.MI%252BWT%252BJUVE.MI%252BFCT.MI%252BBUD%252BNKLA%252BBMPS.MI%252BRIOT%252BMSFT%252BNVDA%252BAAPL%252BRIVN%26qke-neo%3Dfalse%26qct-neo%3Dbar; cmp=t=1725211700&j=1&u=1---&v=40");
                        request.Headers.TryAddWithoutValidation("origin", "https://finance.yahoo.com");
                        request.Headers.TryAddWithoutValidation("priority", "u=1, i");
                        request.Headers.TryAddWithoutValidation("referer", "https://finance.yahoo.com");
                        request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Chromium\";v=\"128\", \"Not;A=Brand\";v=\"24\", \"Microsoft Edge\";v=\"128\"");
                        request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                        request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                        request.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
                        request.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
                        request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-site");
                        request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36 Edg/128.0.0.0");

                        var response = httpClient.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            return YahooSearchResult.FromJson(response.Content.ReadAsStringAsync().Result);
                        }
                        else
                        {
                            StockLog.Write("StatusCode: " + response.StatusCode + Environment.NewLine + response);
                        }
                    }
                }
            }
            catch
            {
            }
            return null;
        }

    }
}