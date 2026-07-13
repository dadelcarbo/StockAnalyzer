using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using System;
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

                var newBars = saxoData.series[0].data.Where(b => b.x > lastDate && b.y > 0).Select(bar => new StockDailyValue(bar.y, bar.h, bar.l, bar.c, 0, bar.x.AddHours(1))).ToArray();

                if (newBars.Length > 0)
                {
                    var finalBars = dataSerie == null ? newBars : dataSerie.Values.Union(newBars.Where(b => b.DATE > dataSerie.LastValue.DATE)).ToArray();

                    // Serialize todays bar only if time is greater that 22:10 pm
                    var isLate = DateTime.Now.TimeOfDay > new TimeSpan(22, 10, 0);
                    var serializeBars = finalBars.Where(b => b.DATE.Date < DateTime.Now.Date || isLate).ToArray();
                    StockBar.Serialize(GetInstrumentFilePath(instrument), serializeBars);

                    dataSerie = new DataSerie(instrument, DefaultDuration, finalBars);

                    instrument.SetDataSerie(DefaultDuration, dataSerie);

                    var history = GetDownloadHistory(instrument);
                    history.LastDate = dataSerie.LastValue.DATE;
                    history.DownloadDate = DateTime.Now;
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

        TimeSpan closeTime = new TimeSpan(22, 00, 0);
        TimeSpan openTime = new TimeSpan(08, 0, 0);
        TimeSpan shortDelay = new TimeSpan(0, 1, 0);
        TimeSpan longDelay = new TimeSpan(2, 0, 0);

        public override bool NeedDownload(StockInstrument instrument)
        {
            var history = GetDownloadHistory(instrument);
            if (history.LastDate == DateTime.MinValue)
                return true;
            if (history.DownloadDate.Add(longDelay) > DateTime.Now)
                return false;

            var now = DateTime.Now;
            var isLate = now.TimeOfDay > closeTime;
            var isEarly = now.TimeOfDay < openTime;

            if ((now.DayOfWeek == DayOfWeek.Friday && isLate) || // Check if week-end
                now.DayOfWeek == DayOfWeek.Saturday ||
                now.DayOfWeek == DayOfWeek.Sunday ||
                (now.DayOfWeek == DayOfWeek.Monday && isEarly))
            {
                if ((now.Date - history.LastDate.Date).TotalDays > 3)
                    return true;
                if (history.LastDate.DayOfWeek == DayOfWeek.Friday && history.LastDate.AddHours(1).TimeOfDay == closeTime)
                {
                    return false;
                }
            }
            else if (isEarly) // 
            {
                return history.LastDate.AddHours(1) != now.Date.AddDays(-1).Add(closeTime);
            }
            else if (isLate)
            {
                return history.LastDate.AddHours(1) != now.Date.Add(closeTime);
            }
            return true;
        }

        protected override bool UpdateIntradayDataSpecific(StockInstrument instrument)
        {
            if (GetDownloadHistory(instrument).DownloadDate.Add(shortDelay) > DateTime.Now)
                return false;

            return DownloadData(instrument) != null;
        }
    }
}
