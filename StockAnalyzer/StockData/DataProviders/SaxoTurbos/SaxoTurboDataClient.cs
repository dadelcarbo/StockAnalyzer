using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace StockAnalyzer.StockData.DataProviders.SaxoTurbos
{
    public class SaxoTurboDataClient : IDataHttpClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticker"></param>
        /// <param name="period">1D - 1 minute bars from begining of the current day<br/>
        /// 2D - 5 minutes bars from the last 24 Hours<br/>
        /// 1W - 1 hour bar for 1 week period</param>
        /// <returns></returns>
        public string FormatUrl(StockInstrument instrument)
        {
            string period = "1W";
            return $"https://fr-be.structured-products.saxo/page-api/charts/BE/isin/{instrument.Isin}/?timespan={period}&type=ohlc&benchmarks=";
        }

        private HttpClient httpClient = null;
        protected string HttpGetContentAsString(StockInstrument instrument)
        {
            try
            {
                if (httpClient == null)
                {
                    var handler = new HttpClientHandler();
                    handler.AutomaticDecompression = ~DecompressionMethods.None;

                    httpClient = new HttpClient(handler);
                }

                var url = FormatUrl(instrument);

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

        public StockDailyValue[] GetData(StockInstrument instrument, DateTime startDate)
        {
            try
            {
                var jsonData = HttpGetContentAsString(instrument);
                if (jsonData == null)
                    return null;

                var saxoData = JsonSerializer.Deserialize<SaxoJSon>(jsonData);
                if (saxoData?.series?[0]?.data == null || saxoData?.series?[0]?.data.Count == 0)
                    return null;

                var bars = saxoData.series[0].data
                    .Where(b => b.x > startDate && b.y > 0)
                    .Select(bar => new StockDailyValue(bar.y, bar.h, bar.l, bar.c, 0, bar.x.AddHours(1)))
                    .ToArray();

                if (MarketHours.MarketHoursTable[instrument.Market].IsOpened)
                {
                    foreach (var bar in bars.Where(b => b.DATE.Date == DateTime.Today && b.DATE.Hour == DateTime.Now.Hour))
                    {
                        bar.IsComplete = false;
                    }
                }

                return bars;
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            return null;
        }
    }
}
