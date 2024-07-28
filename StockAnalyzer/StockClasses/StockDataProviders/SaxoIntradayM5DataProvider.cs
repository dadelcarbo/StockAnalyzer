using Newtonsoft.Json;
using StockAnalyzer.StockClasses.StockDataProviders.Saxo;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockWeb;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class SaxoIntradayM5DataProvider : StockDataProviderBase, IConfigDialog
    {
        static private readonly string ARCHIVE_FOLDER = INTRADAY_ARCHIVE_SUBFOLDER + @"\SaxoIntradayM5";
        static private readonly string INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\SaxoIntradayM5";
        static private readonly string CONFIG_FILE = "SaxoIntradayM5Download.cfg";

        public string UserConfigFileName => CONFIG_FILE;

        public override void InitDictionary(StockDictionary stockDictionary, bool download)
        {
            // Create data folder if not existing
            if (!Directory.Exists(DataFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ARCHIVE_FOLDER);
            }
            if (!Directory.Exists(DataFolder + INTRADAY_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + INTRADAY_FOLDER);
            }

            // Parse SaxoIntradayM5Download.cfg file
            this.needDownload = download;
            InitFromFile(stockDictionary, download, Path.Combine(Folders.PersonalFolder, CONFIG_FILE));
        }

        public override bool SupportsIntradayDownload => true;

        public override bool LoadData(StockSerie stockSerie)
        {
            var archiveFileName = DataFolder + ARCHIVE_FOLDER + "\\" + stockSerie.Symbol.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
            if (File.Exists(archiveFileName))
            {
                stockSerie.ReadFromCSVFile(archiveFileName);
            }
            return stockSerie.Count > 0;
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

        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            return true;
        }
        static readonly SortedDictionary<string, DateTime> DownloadHistory = new SortedDictionary<string, DateTime>();
        public override bool DownloadIntradayData(StockSerie stockSerie)
        {
            if (stockSerie.Count > 0)
            {
                if (DownloadHistory.ContainsKey(stockSerie.Symbol) && DownloadHistory[stockSerie.Symbol] > DateTime.Now.AddMinutes(-1))
                {
                    return false;  // Do not download more than every 2 minutes.
                }
                var lastDate = stockSerie.Keys.Last();
                if (lastDate.Date == DateTime.Today && lastDate.TimeOfDay == new TimeSpan(21, 55, 00))
                {
                    return false;
                }
            }

            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NotifyProgress("Downloading intraday for " + stockSerie.StockName);

                using var wc = new WebClient();
                wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

                try
                {
                    if (DownloadHistory.ContainsKey(stockSerie.Symbol))
                    {
                        DownloadHistory[stockSerie.Symbol] = DateTime.Now;
                    }
                    else
                    {
                        DownloadHistory.Add(stockSerie.Symbol, DateTime.Now);
                    }
                    var url = FormatIntradayURL(stockSerie.ISIN, "2D");
                    var jsonData = SaxoIntradayM5DataProvider.HttpGetFromSaxo(url);
                    var saxoData = JsonConvert.DeserializeObject<SaxoJSon>(jsonData, Converter.Settings);
                    if (saxoData?.series?[0]?.data == null)
                        return false;

                    stockSerie.IsInitialised = false;
                    this.LoadData(stockSerie);
                    DateTime lastDate = DateTime.MinValue;
                    if (stockSerie.Count > 0)
                    {
                        lastDate = stockSerie.Keys.Last();
                        if (stockSerie.Keys.Last().Date == DateTime.Today)
                        {
                            stockSerie.RemoveLast();
                        }
                    }
                    else
                    {
                        List<StockDailyValue> m5Bar = DownloadDataM5FromH1(stockSerie);
                        if (m5Bar != null)
                        {
                            var end = saxoData.series[0]?.data.Count > 0 ? saxoData.series[0].data[0].r.low : DateTime.MaxValue;

                            foreach (var bar in m5Bar.Where(b => b.DATE < end))
                            {
                                stockSerie.Add(bar.DATE, bar);
                            }
                            lastDate = stockSerie.Keys.Last();
                        }
                        else
                        {
                            lastDate = saxoData.series[0].data.First().x;
                        }
                    }
                    DateTime date = DateTime.Today.AddHours(8);
                    bool update = false;
                    foreach (var bar in saxoData.series[0].data.Where(b => b.r.low > lastDate && b.y > 0).ToList())
                    {
                        var newBar = new StockDailyValue(bar.y, bar.h, bar.l, bar.c, 0, bar.r.low);
                        stockSerie.Add(newBar.DATE, newBar);

                        update = true;
                    }

                    if (update)
                    {
                        var firstArchiveDate = stockSerie.Keys.Last().AddMonths(-2).AddDays(-lastDate.Day + 1).Date;
                        var archiveFileName = DataFolder + ARCHIVE_FOLDER + "\\" + stockSerie.Symbol.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

                        var lastArchiveDate = stockSerie.Keys.Last().Date < DateTime.Today || DateTime.Now.TimeOfDay > new TimeSpan(22, 0, 0) ? stockSerie.Keys.Last() : stockSerie.Keys.Last().Date;

                        stockSerie.SaveToCSVFromDateToDate(archiveFileName, firstArchiveDate, lastArchiveDate);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    StockLog.Write(e);
                }
            }
            return false;
        }

        private List<StockDailyValue> DownloadDataM5FromH1(StockSerie stockSerie)
        {
            try
            {
                var url = FormatIntradayURL(stockSerie.ISIN, "1W");
                var jsonData = SaxoIntradayM5DataProvider.HttpGetFromSaxo(url);
                var saxoData = JsonConvert.DeserializeObject<SaxoJSon>(jsonData, Converter.Settings);
                if (saxoData?.series?[0]?.data == null)
                    return null;

                return FixMinuteBars(saxoData.series[0].data.Where(bar => bar.x < DateTime.Today).Select(bar => new StockDailyValue(bar.y, bar.y, bar.y, bar.y, 0, bar.x)), 5);
            }
            catch (Exception e)
            {
                StockLog.Write(e);
            }
            return null;
        }

        public bool DownloadIntradayDataH1(StockSerie stockSerie)
        {
            //if (stockSerie.Uic != 0)
            //{
            //    var chartService = new ChartService();
            //    var dailyValues = chartService.GetData(stockSerie.Uic, BarDuration.M_5);
            //    if (dailyValues != null)
            //    {
            //        stockSerie.IsInitialised = false;
            //        foreach (var newBar in dailyValues)
            //        {
            //            stockSerie.Add(newBar.DATE, newBar);
            //        }
            //        stockSerie.Initialise();
            //        return true;
            //    }
            //}

            if (stockSerie.Count > 0)
            {
                if (DownloadHistory.ContainsKey(stockSerie.Symbol) && DownloadHistory[stockSerie.Symbol] > DateTime.Now.AddSeconds(-30))
                {
                    return false;  // Do not download more than every xx seconds
                }
                var lastDate = stockSerie.Keys.Last();
                if (lastDate.Date == DateTime.Today && lastDate.TimeOfDay == new TimeSpan(21, 55, 00))
                {
                    return false;
                }
            }

            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NotifyProgress("Downloading intraday for " + stockSerie.StockName);

                using var wc = new WebClient();
                wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

                try
                {
                    string url = FormatIntradayURL(stockSerie.ISIN, "1W");
                    if (DownloadHistory.ContainsKey(stockSerie.Symbol))
                    {
                        DownloadHistory[stockSerie.Symbol] = DateTime.Now;
                    }
                    else
                    {
                        DownloadHistory.Add(stockSerie.Symbol, DateTime.Now);
                    }
                    var jsonData = SaxoIntradayM5DataProvider.HttpGetFromSaxo(url);
                    if (string.IsNullOrEmpty(jsonData))
                        return false;
                    var saxoData = JsonConvert.DeserializeObject<SaxoJSon>(jsonData, Converter.Settings);
                    if (saxoData?.series?[0]?.data == null)
                        return false;

                    stockSerie.IsInitialised = false;
                    this.LoadData(stockSerie);
                    DateTime lastDate = DateTime.MinValue;
                    if (stockSerie.Count > 0)
                    {
                        lastDate = stockSerie.Keys.Last();
                    }
                    else
                    {
                        lastDate = saxoData.series[0].data.First().x.AddTicks(-1);



                    }
                    int nbNewBars = 0;
                    foreach (var bar in saxoData.series[0].data.Where(b => b.x > lastDate && b.y > 0).ToList())
                    {
                        var newBar = new StockDailyValue(bar.y, bar.h, bar.l, bar.c, 0, bar.x.AddHours(-1));
                        stockSerie.Add(newBar.DATE, newBar);
                        nbNewBars++;
                    }

                    if (nbNewBars > 0)
                    {
                        var firstArchiveDate = stockSerie.Keys.Last().AddMonths(-2).AddDays(-lastDate.Day + 1).Date;
                        var archiveFileName = DataFolder + ARCHIVE_FOLDER + "\\" + stockSerie.Symbol.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

                        var lastArchiveDate = stockSerie.Keys.Last().Date < DateTime.Today || DateTime.Now.TimeOfDay > new TimeSpan(22, 0, 0) ? stockSerie.Keys.Last() : stockSerie.Keys.Last().Date;

                        stockSerie.SaveToCSVFromDateToDate(archiveFileName, firstArchiveDate, lastArchiveDate);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    StockLog.Write(e);
                }
            }
            return false;
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
        private void InitFromFile(StockDictionary stockDictionary, bool download, string fileName)
        {
            string line;
            if (File.Exists(fileName))
            {
                using var sr = new StreamReader(fileName, true);
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                    if (line.StartsWith("$")) break;

                    var row = line.Split(',');
                    if (!stockDictionary.ContainsKey(row[1]))
                    {
                        var stockSerie = new StockSerie(row[1], row[0], StockSerie.Groups.TURBO, StockDataProvider.SaxoIntraday_M5, BarDuration.M_5);
                        stockSerie.ISIN = row[0];
                        stockDictionary.Add(row[1], stockSerie);
                        if (row.Length == 3)
                        {
                            stockSerie.Uic = long.Parse(row[2]);
                        }

                        if (RefSerie == null && download) // Check if provider is up to date by checking the reference serie
                        {
                            // Check if download needed.
                            stockSerie.Initialise();
                            DateTime refDate = DateTime.MinValue;
                            if (stockSerie.Count > 0)
                            {
                                refDate = stockSerie.Keys.Last();
                            }
                            this.DownloadIntradayData(stockSerie);
                            if (stockSerie.Initialise())
                            {
                                needDownload = refDate < stockSerie.Keys.Last();
                                RefSerie = stockSerie;
                            }
                        }
                        else
                        {
                            if (download && this.needDownload)
                            {
                                this.DownloadIntradayData(stockSerie);
                            }
                        }
                    }
                    else
                    {
                        StockLog.Write("Saxo Intraday Entry: " + row[1] + " already in stockDictionary");
                    }
                }
            }
        }

        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            //Process.Start(Path.Combine(Folders.PersonalFolder, CONFIG_FILE_USER));

            //var configDlg = new SaxoDataProviderDlg(stockDico, Path.Combine(Folders.PersonalFolder, CONFIG_FILE_USER)) { StartPosition = FormStartPosition.CenterScreen };
            //configDlg.ShowDialog();

            return DialogResult.OK;
        }

        public override string DisplayName => "Saxo Turbos 5Min";

        public override void OpenInDataProvider(StockSerie stockSerie)
        {
            Process.Start($"https://fr-be.structured-products.saxo/products/{stockSerie.ISIN}");
        }

        public override bool RemoveEntry(StockSerie stockSerie)
        {
            SaxoConfigEntry.RemoveEntry(stockSerie.ISIN, Path.Combine(Folders.PersonalFolder, CONFIG_FILE));

            return true;
        }
    }
}
