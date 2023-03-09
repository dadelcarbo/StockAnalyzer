using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json;
using UltimateChartist.Helpers;

namespace UltimateChartist.DataModels.DataProviders.SaxoTurbo
{
    public class SaxoTurboDataProvider : StockDataProviderBase
    {
        static private readonly string SAXO_ID_FILE = "SaxoUnderlyings.cfg";

        static public string SaxoUnderlyingFile => Path.Combine(Folders.PersonalFolder, SAXO_ID_FILE);

        public override string Name => "SaxoTurbo";
        public override string DisplayName => "Saxo Turbo";
        public override BarDuration[] BarDurations { get; } = { BarDuration.M_1, BarDuration.M_5, BarDuration.H_1, BarDuration.Daily };
        public override BarDuration DefaultBarDuration => BarDuration.Daily;

        public override void InitDictionary()
        {
            InitCacheFolders();

            // Parse SaxoIntradayDownload.cfg file
            InitFromFile(Path.Combine(Folders.PersonalFolder, CONFIG_FILE));
        }

        public override List<StockBar> LoadData(Instrument instrument, BarDuration duration)
        {
            var archiveFileName = Path.Combine(CACHE_FOLDER, duration.ToString(), instrument.Symbol.Replace(':', '_') + "_" + instrument.Name + ".txt");
            if (File.Exists(archiveFileName))
            {
                return StockBar.Load(archiveFileName);
            }
            else
            {
                return DownloadData(instrument, duration);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticker"></param>
        /// <param name="period">1D - 1 minute bars from begining of the current day<br/>
        /// 2D - 5 minutes bars from the last 24 Hours<br/>
        /// 1W - 1 hour bar for 1 week period</param>
        /// <returns></returns>
        public string FormatIntradayURL(string ticker, string period)
        {
            return $"https://fr-be.structured-products.saxo/page-api/charts/BE/isin/{ticker}/?timespan={period}&type=ohlc&benchmarks=";
        }

        static SortedDictionary<string, DateTime> DownloadHistory = new SortedDictionary<string, DateTime>();

        public override List<StockBar> DownloadData(Instrument instrument, BarDuration duration)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return null;

            NotifyProgress("Downloading intraday for " + instrument.Name);

            try
            {
                var url = duration switch
                {
                    BarDuration.M_1 => FormatIntradayURL(instrument.ISIN, "1D"),
                    BarDuration.M_5 => FormatIntradayURL(instrument.ISIN, "2D"),
                    BarDuration.H_1 => FormatIntradayURL(instrument.ISIN, "1W"),
                    BarDuration.Daily => FormatIntradayURL(instrument.ISIN, "6M"),
                    _ => null
                };
                if (url == null)
                    return null;
                var jsonData = HttpGetFromSaxo(url);
                if (string.IsNullOrEmpty(jsonData))
                    return null;
                var saxoData = JsonSerializer.Deserialize<SaxoTurboJSon>(jsonData);
                if (saxoData?.series?[0]?.data == null)
                    return null;

                DateTime lastDate = DateTime.MinValue;
                var bars = new List<StockBar>();
                foreach (var bar in saxoData.series[0].data.Where(b => b.x > lastDate && b.y > 0).ToList())
                {
                    DateTime date = bar.x;
                    if (duration != BarDuration.Daily)
                    {
                        date = bar.x.AddHours(-1);
                    }
                    var newBar = new StockBar(date, bar.y, bar.h, bar.l, bar.c, 0);
                    bars.Add(newBar);
                }
                return bars;
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            return null;
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
                using (var request = new HttpRequestMessage())
                {
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
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            return null;

        }
        private void InitFromFile(string fileName)
        {
            string line;
            if (File.Exists(fileName))
            {
                using (var sr = new StreamReader(fileName, true))
                {
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                        var row = line.Split(',');
                        var instrument = new Instrument()
                        {
                            Name = row[1],
                            Symbol = row[0],
                            ISIN = row[0],
                            Country = row[0].Substring(0, 2),
                            Group = StockGroup.TURBO,
                            DataProvider = this,
                            RealTimeDataProvider = this
                        };
                        Instruments.Add(instrument);
                    }
                }
            }
        }

        static DateTime refDate = new DateTime(1970, 01, 01) + (DateTime.Now - DateTime.UtcNow);

        //public DialogResult ShowDialog(StockDictionary stockDico)
        //{
        //    //Process.Start(Path.Combine(Folders.PersonalFolder, CONFIG_FILE_USER));

        //    var configDlg = new SaxoDataProviderDlg(stockDico, Path.Combine(Folders.PersonalFolder, CONFIG_FILE_USER));
        //    configDlg.ShowDialog();

        //    return DialogResult.OK;
        //}

        public override void OpenInDataProvider(Instrument instrument)
        {
            Process.Start($"https://fr-be.structured-products.saxo/products/{instrument.ISIN}");
        }
    }

    #region JSON PARSING CLASSES
    public class R
    {
        public DateTime low { get; set; }
        public DateTime high { get; set; }
    }

    public class SaxoTurboDatum
    {
        public DateTime x { get; set; }
        public decimal y { get; set; }
        public decimal c { get; set; }
        public decimal h { get; set; }
        public decimal l { get; set; }
        public R r { get; set; }
    }

    public class Series
    {
        public int sin { get; set; }
        public string name { get; set; }
        public string currency { get; set; }
        public List<SaxoTurboDatum> data { get; set; }
    }

    public class Line
    {
        public int sin { get; set; }
        public string id { get; set; }
        public decimal value { get; set; }
    }
    public class SaxoTurboJSon
    {
        //public string timespan { get; set; }
        public List<Series> series { get; set; }
        // public List<Line> lines { get; set; }
    }
    #endregion

}
