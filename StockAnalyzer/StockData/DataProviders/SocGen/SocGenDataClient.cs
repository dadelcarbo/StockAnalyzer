using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace StockAnalyzer.StockData.DataProviders.SocGen
{
    internal class SocGenDataClient : IDataHttpClient
    {
        /// <summary>
        /// Return 1Week data of tick based data. Need to evaluate 1 hour bars
        /// </summary>
        /// <param name="instrument"></param>
        /// <returns></returns>
        public string FormatUrl(StockInstrument instrument)
        {
            return $"https://sgbourse.fr/EmcWebApi/api/Prices/Intraday?productId={instrument.Ticker}";
        }

        public StockDailyValue[] GetData(StockInstrument instrument, DateTime startDate)
        {
            try
            {
                string url = FormatUrl(instrument);

                var jsonData = HttpGetFromSocGen(url);
                if (jsonData == null)
                    return null;

                var data = JsonSerializer.Deserialize<SocGenDatum[]>(jsonData);

                var tickBars = data?.Where(b => b.Date > startDate);
                if (tickBars == null || !tickBars.Any())
                    return null;

                return GenerateHourBarFromTickBar(tickBars)?.ToArray();

            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            return null;
        }

        private IEnumerable<StockDailyValue> GenerateHourBarFromTickBar(IEnumerable<SocGenDatum> values)
        {
            List<StockDailyValue> bars = new List<StockDailyValue>();
            StockDailyValue newValue = null;
            DateTime closeDate = DateTime.Now;
            foreach (var tickBar in values)
            {
                var tickDate = tickBar.Date.ToLocalTime();
                var tickValue = tickBar.Ask == 0 ? tickBar.Bid : tickBar.Ask;
                if (newValue == null)
                {
                    // New bar
                    var openDate = new DateTime(tickDate.Year, tickDate.Month, tickDate.Day, tickDate.Hour, 0, 0);
                    closeDate = openDate.AddHours(1);

                    newValue = new StockDailyValue(tickValue, tickValue, tickValue, tickValue, 0, openDate)
                    {
                        IsComplete = false
                    };
                    bars.Add(newValue);
                }
                else if (tickDate.Date != newValue.DATE.Date || tickDate >= closeDate)
                {
                    // Force bar end at the end of a day
                    newValue.IsComplete = true;

                    // New bar
                    var openDate = new DateTime(tickDate.Year, tickDate.Month, tickDate.Day, tickDate.Hour, 0, 0);
                    closeDate = openDate.AddHours(1);

                    newValue = new StockDailyValue(tickValue, tickValue, tickValue, tickValue, 0, openDate)
                    {
                        IsComplete = false
                    };
                    bars.Add(newValue);
                }
                else
                {
                    // Need to extend current bar
                    newValue.HIGH = Math.Max(newValue.HIGH, tickValue);
                    newValue.LOW = Math.Min(newValue.LOW, tickValue);
                    newValue.CLOSE = tickValue;
                }
            }
            return bars.Count > 0 ? bars : null;
        }

        private HttpClient httpClient = null;
        private string HttpGetFromSocGen(string url)
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
    }
}