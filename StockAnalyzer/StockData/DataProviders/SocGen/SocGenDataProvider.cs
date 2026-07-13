using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Telerik.Windows.Data;

namespace StockAnalyzer.StockData.DataProviders.SocGen
{
    public class SocGenDataProvider : DataProviderBase
    {
        public override string DisplayName => "Soc. Gen.";
        public override BarDuration[] SupportedDurations => new BarDuration[] { BarDuration.H_1, BarDuration.H_2, BarDuration.H_3, BarDuration.H_4 };

        public override BarDuration DefaultDuration => BarDuration.H_1;

        public override DataProvider Provider => DataProvider.SocGen;

        /// <summary>
        /// Return 1Week data of tick based data. Need to evaluate 1 hour bars
        /// </summary>
        /// <param name="ticker"></param>
        /// <returns></returns>
        public string FormatIntradayURL(long ticker)
        {
            return $"https://sgbourse.fr/EmcWebApi/api/Prices/Intraday?productId={ticker}";
            //return $"https://sgbourse.fr/EmcWebApi/api/Prices/Intraday/Asset?assetId={ticker}";
        }

        public override DataSerie DownloadData(StockInstrument instrument)
        {
            NotifyProgress($"Downloading {instrument.DisplayName}");

            using var wc = new WebClient();
            wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

            DataSerie dataSerie = LoadData(instrument, BarDuration.H_1);
            try
            {
                string url = FormatIntradayURL(instrument.Ticker);
                var jsonData = HttpGetFromSocGen(url);
                if (string.IsNullOrEmpty(jsonData))
                    return dataSerie;

                var data = JsonSerializer.Deserialize<Datum[]>(jsonData);
                if (data == null || data.Length == 0)
                    return dataSerie;

                DateTime lastDate;
                if (dataSerie?.LastValue != null)
                {
                    if (dataSerie.LastValue.DATE >= data.Last().Date)
                    {
                        return dataSerie;
                    }
                    lastDate = dataSerie.LastValue.DATE;
                }
                else
                {
                    lastDate = data.First().Date.AddTicks(-1);
                }

                var newBars = data.Where(b => b.Date > lastDate).Select(bar => new StockDailyValue(bar.Bid, bar.Bid, bar.Bid, bar.Bid, 0, bar.Date)).ToArray();

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
        private static string HttpGetFromSocGen(string url)
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
                Name = row[3],
                Isin = row[0],
                Symbol = row[1],
                Ticker = long.Parse(row[2]),
                Group = Groups.TURBO,
                Provider = DataProvider.SocGen,
                Market = Market.TURBO
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
