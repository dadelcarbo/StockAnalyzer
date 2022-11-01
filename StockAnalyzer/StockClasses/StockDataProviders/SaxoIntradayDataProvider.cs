using Newtonsoft.Json;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockClasses.StockDataProviders.Saxo;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockWeb;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Net.Http;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class SaxoIntradayDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private readonly string ARCHIVE_FOLDER = INTRADAY_ARCHIVE_SUBFOLDER + @"\SaxoIntraday";
        static private readonly string INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\SaxoIntraday";
        static private readonly string CONFIG_FILE = "SaxoIntradayDownload.cfg";
        static private readonly string CONFIG_FILE_USER = "SaxoIntradayDownload.user.cfg";

        public string UserConfigFileName => CONFIG_FILE_USER;

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

            // Parse SaxoIntradayDownload.cfg file
            this.needDownload = download;
            InitFromFile(stockDictionary, download, Path.Combine(Folders.PersonalFolder, CONFIG_FILE));
            InitFromFile(stockDictionary, download, Path.Combine(Folders.PersonalFolder, CONFIG_FILE_USER));
        }

        public override bool SupportsIntradayDownload => true;

        public override bool LoadData(StockSerie stockSerie)
        {
            var archiveFileName = DataFolder + ARCHIVE_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
            if (File.Exists(archiveFileName))
            {
                stockSerie.ReadFromCSVFile(archiveFileName);
            }
            return stockSerie.Count > 0;
        }

        public string FormatIntradayURL(string ticker, string period)
        {
            return $"https://fr-be.structured-products.saxo/page-api/charts/BE/isin/{ticker}/?timespan={period}&type=ohlc&benchmarks=";
        }

        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            return true;
        }
        static SortedDictionary<string, DateTime> DownloadHistory = new SortedDictionary<string, DateTime>();
        public bool DownloadIntradayData5m(StockSerie stockSerie)
        {
            if (stockSerie.Count > 0)
            {
                if (DownloadHistory.ContainsKey(stockSerie.ShortName) && DownloadHistory[stockSerie.ShortName] > DateTime.Now.AddMinutes(-2))
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

                using (var wc = new WebClient())
                {
                    wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    var url = FormatIntradayURL(stockSerie.ISIN, "1D");

                    try
                    {
                        if (DownloadHistory.ContainsKey(stockSerie.ShortName))
                        {
                            DownloadHistory[stockSerie.ShortName] = DateTime.Now;
                        }
                        else
                        {
                            DownloadHistory.Add(stockSerie.ShortName, DateTime.Now);
                        }
                        var jsonData = SaxoIntradayDataProvider.HttpGetFromSaxo(url);
                        var saxoData = JsonConvert.DeserializeObject<SaxoJSon>(jsonData, Converter.Settings);
                        if (saxoData?.series?[0]?.data == null)
                            return false;

                        stockSerie.IsInitialised = false;
                        this.LoadData(stockSerie);
                        DateTime lastDate = DateTime.MinValue;
                        if (stockSerie.Count > 0)
                        {
                            if (stockSerie.Keys.Last().Date == DateTime.Today)
                            {
                                lastDate = stockSerie.Keys.Last();
                                stockSerie.RemoveLast();
                            }
                        }
                        else
                        {
                            lastDate = saxoData.series[0].data.First().x;
                        }
                        var date = lastDate;
                        StockDailyValue newBar = null;
                        foreach (var bar in saxoData.series[0].data.Where(b => b.x > lastDate && b.y > 0).ToList())
                        {
                            if (newBar == null)
                            {
                                newBar = new StockDailyValue(bar.y, bar.y, bar.y, bar.y, 0, date);
                            }
                            else
                            {
                                var minute = (bar.x.Minute / 5) * 5;
                                if (minute == newBar.DATE.Minute)
                                {
                                    newBar.HIGH = Math.Max(newBar.HIGH, bar.y);
                                    newBar.LOW = Math.Min(newBar.LOW, bar.y);
                                    newBar.CLOSE = bar.y;
                                }
                                else
                                {
                                    date = date.AddMinutes(5);
                                    stockSerie.Add(newBar.DATE, newBar);
                                    newBar = new StockDailyValue(newBar.CLOSE, bar.y, bar.y, bar.y, 0, date);
                                }
                            }
                        }
                        if (newBar != null)
                        {
                            stockSerie.Add(date, newBar);
                        }

                        var firstArchiveDate = stockSerie.Keys.Last().AddMonths(-2).AddDays(-lastDate.Day + 1).Date;
                        var archiveFileName = DataFolder + ARCHIVE_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

                        var lastArchiveDate = stockSerie.Keys.Last().Date < DateTime.Today || DateTime.Now.TimeOfDay > new TimeSpan(22, 0, 0) ? stockSerie.Keys.Last() : stockSerie.Keys.Last().Date;

                        stockSerie.SaveToCSVFromDateToDate(archiveFileName, firstArchiveDate, lastArchiveDate);

                        return true;
                    }
                    catch (Exception e)
                    {
                        StockLog.Write(e);
                    }
                }
            }
            return false;
        }
        public override bool DownloadIntradayData(StockSerie stockSerie)
        {
            if (stockSerie.Count > 0)
            {
                if (DownloadHistory.ContainsKey(stockSerie.ShortName) && DownloadHistory[stockSerie.ShortName] > DateTime.Now.AddSeconds(-30))
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

                using (var wc = new WebClient())
                {
                    wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    var url = FormatIntradayURL(stockSerie.ISIN, "1W");

                    try
                    {
                        if (DownloadHistory.ContainsKey(stockSerie.ShortName))
                        {
                            DownloadHistory[stockSerie.ShortName] = DateTime.Now;
                        }
                        else
                        {
                            DownloadHistory.Add(stockSerie.ShortName, DateTime.Now);
                        }
                        var jsonData = SaxoIntradayDataProvider.HttpGetFromSaxo(url);
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
                            var archiveFileName = DataFolder + ARCHIVE_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

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
        private void InitFromFile(StockDictionary stockDictionary, bool download, string fileName)
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
                        if (!stockDictionary.ContainsKey(row[1]))
                        {
                            var stockSerie = new StockSerie(row[1], row[0], StockSerie.Groups.INTRADAY, StockDataProvider.SaxoIntraday, BarDuration.H_1);
                            stockSerie.ISIN = row[0];
                            stockDictionary.Add(row[1], stockSerie);

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
                            Console.WriteLine("Saxo Intraday Entry: " + row[1] + " already in stockDictionary");
                        }
                    }
                }
            }
        }

        static DateTime refDate = new DateTime(1970, 01, 01) + (DateTime.Now - DateTime.UtcNow);

        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            //Process.Start(Path.Combine(Folders.PersonalFolder, CONFIG_FILE_USER));

            var configDlg = new SaxoDataProviderDlg(stockDico, Path.Combine(Folders.PersonalFolder, CONFIG_FILE_USER));
            return configDlg.ShowDialog();

            return DialogResult.OK;
        }

        public string DisplayName => "Saxo Intraday";
    }
}
