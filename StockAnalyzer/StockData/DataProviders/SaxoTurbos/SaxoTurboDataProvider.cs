using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData.DataProviders.SaxoTurboDataProvider;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Telerik.Windows.Data;

namespace StockAnalyzer.StockData.DataProviders.SaxoTurbos
{
    public class SaxoTurboDataProvider : DataProviderBase
    {
        public override string DisplayName => "Saxo Turbos";
        public override BarDuration[] SupportedDurations => new BarDuration[] { BarDuration.H_1, BarDuration.H_2, BarDuration.H_3, BarDuration.H_4 };

        public override BarDuration DefaultDuration => BarDuration.H_1;

        public override DataProvider Provider => DataProvider.SaxoTurbo;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticker"></param>
        /// <param name="period">1D - 1 minute bars from begining of the current day<br/>
        /// 2D - 5 minutes bars from the last 24 Hours<br/>
        /// 1W - 1 hour bar for 1 week period</param>
        /// <returns></returns>
        string FormatIntradayURL(string ticker, string period)
        {
            return $"https://fr-be.structured-products.saxo/page-api/charts/BE/isin/{ticker}/?timespan={period}&type=ohlc&benchmarks=";
        }

        public override DataSerie DownloadData(StockInstrument instrument)
        {
            NotifyProgress($"Downloading {instrument.DisplayName}");

            using var wc = new WebClient();
            wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

            DataSerie dataSerie = LoadData(instrument, BarDuration.H_1);
            try
            {
                string url = FormatIntradayURL(instrument.Isin, "1W");
                var jsonData = HttpGetFromSaxo(url);
                if (string.IsNullOrEmpty(jsonData))
                    return dataSerie;

                var saxoData = JsonSerializer.Deserialize<SaxoJSon>(jsonData);
                if (saxoData?.series?[0]?.data == null || saxoData?.series?[0]?.data.Count == 0)
                    return dataSerie;

                DateTime lastDate;
                if (dataSerie?.LastValue != null)
                {
                    if (dataSerie.LastValue.DATE >= saxoData.series[0].data.Last().x)
                    {
                        return dataSerie;
                    }
                    lastDate = dataSerie.LastValue.DATE;
                }
                else
                {
                    lastDate = saxoData.series[0].data.First().x.AddTicks(-1);
                }

                List<StockDailyValue> newBars = new List<StockDailyValue>();
                foreach (var bar in saxoData.series[0].data.Where(b => b.x > lastDate && b.y > 0).ToList())
                {
                    var newBar = new StockDailyValue(bar.y, bar.h, bar.l, bar.c, 0, bar.x.AddHours(1));
                    newBars.Add(newBar);
                }

                if (newBars.Count > 0)
                {
                    var finalBars = dataSerie.Values.Union(newBars.Where(b => b.DATE > dataSerie.LastValue.DATE)).ToArray();

                    // Serialize todays bar only if time is greater that 22:10 pm
                    var isLate = DateTime.Now.TimeOfDay > new TimeSpan(22, 10, 0);
                    var serializeBars = finalBars.Where(b => b.DATE.Date < DateTime.Now.Date || isLate).ToArray();
                    StockBar.Serialize(GetInstrumentFilePath(instrument), serializeBars);

                    dataSerie = new DataSerie(instrument, BarDuration.H_1, finalBars);
                }

                return dataSerie;
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            return dataSerie;
        }

        static private HttpClient httpClient = null;
        private static string HttpGetFromSaxo(string url)
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

        public override void ForceDownloadData(StockInstrument instrument)
        {
            throw new NotImplementedException();
        }

        protected override StockInstrument CreateInstrumentFromConfigLine(string line)
        {
            var row = line.Split(',');

            return new StockInstrument()
            {
                Id = row[1],
                Name = row[2],
                Isin = row[1],
                Symbol = string.Empty,
                Group = Groups.TURBO,
                Provider = DataProvider.SaxoTurbo
            };
        }

        protected override bool NeedDownload(StockInstrument instrument, InstrumentDownloadHistory history)
        {
            if (history.DownloadDate == DateTime.MinValue)
                return true;

            var closeTime = new TimeSpan(22, 00, 0);
            var openTime = new TimeSpan(08, 0, 0);
            var delay = new TimeSpan(0, 0, 5);

            var now = DateTime.Now;
            var isLate = now.TimeOfDay > closeTime;
            var isEarly = now.TimeOfDay < openTime;

            // Check if week-end
            if ((now.DayOfWeek == DayOfWeek.Friday && isLate) ||
                now.DayOfWeek == DayOfWeek.Saturday ||
                now.DayOfWeek == DayOfWeek.Sunday ||
                (now.DayOfWeek == DayOfWeek.Monday && isEarly))
            {
                if ((now.Date - history.LastDate.Date).TotalDays >= 3)
                    return true;
                if (history.LastDate.DayOfWeek == DayOfWeek.Friday && history.LastDate.AddHours(1).TimeOfDay == closeTime)
                {
                    return false;
                }
            }
            else if (isEarly) // 
            {
                if ((history.LastDate.Date - now.Date).TotalDays <= 1)
                {
                    return false;
                }
            }
            else if (isLate)
            {
                if (history.LastDate.AddHours(1) == now.Date.Add(closeTime))
                {
                    return false;
                }
            }
            return now > history.DownloadDate.Add(delay);
        }
    }
}
