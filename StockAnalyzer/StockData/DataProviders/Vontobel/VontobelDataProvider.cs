using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders.Vontobel;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Telerik.Windows.Data;

namespace StockAnalyzer.StockData.DataProviders.Vontobel
{
    public class VontobelDataProvider : DataProviderBase
    {
        public override string DisplayName => "Vontobel";
        public override BarDuration[] SupportedDurations => new BarDuration[] { BarDuration.H_1, BarDuration.H_2, BarDuration.H_3, BarDuration.H_4 };

        public override BarDuration DefaultDuration => BarDuration.H_1;

        public override DataProvider Provider => DataProvider.Vontobel;

        /// <summary>
        /// Period is one of:<br/>
        /// 0 - 1D 5 Minutes Bars<br/>
        /// 1 - 2D 5 Minutes Bars<br/>
        /// 2 - 1W 5 Minutes Bars<br/>
        /// 3 - 1M 1 Hour Bars
        /// </summary>
        /// <param name="Isin"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public string FormatIntradayURL(string Isin, int period)
        {
            return $"https://markets.vontobel.com/api/v1/charts/products/{Isin}/detail/{period}?c=fr-fr";
        }

        public override DataSerie DownloadData(StockInstrument instrument)
        {
            NotifyProgress($"Downloading {instrument.DisplayName}");

            using var wc = new WebClient();
            wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

            DataSerie dataSerie = LoadData(instrument, BarDuration.H_1);
            try
            {
                string url = FormatIntradayURL(instrument.Isin, 3);

                var jsonData = HttpGetFromVontobel(url);
                if (string.IsNullOrEmpty(jsonData))
                    return dataSerie;

                var vontobelData = JsonSerializer.Deserialize<VontobelJSon>(jsonData);
                if (vontobelData == null || !vontobelData.isSuccess || vontobelData.payload == null)
                {
                    return dataSerie;
                }
                DateTime lastDate;
                if (dataSerie?.LastValue != null)
                {
                    if (dataSerie.LastValue.DATE >= refDate.AddMilliseconds(vontobelData.payload.series[0].points.Max(p => p.timestamp)).ToLocalTime())
                    {
                        return dataSerie;
                    }
                    lastDate = dataSerie.LastValue.DATE;
                }
                else
                {
                    lastDate = refDate.AddMilliseconds(vontobelData.payload.series[0].points.Min(p => p.timestamp)).ToLocalTime();
                }

                var newBars = vontobelData.payload.series[0].points.Reverse().Select(bar => new StockDailyValue(bar.bid, bar.bid, bar.bid, bar.bid, 0, refDate.AddMilliseconds(bar.timestamp).ToLocalTime())).ToArray();
                if (newBars.Count() > 0)
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
        private static string HttpGetFromVontobel(string url)
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
                Id = row[0],
                Name = row[1],
                Isin = row[0],
                Symbol = row[2],
                Group = Groups.TURBO,
                Provider = DataProvider.Vontobel
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
