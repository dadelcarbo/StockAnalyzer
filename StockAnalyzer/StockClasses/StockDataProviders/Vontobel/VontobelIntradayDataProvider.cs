﻿using Newtonsoft.Json;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
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

namespace StockAnalyzer.StockClasses.StockDataProviders.Vontobel
{
    public class VontobelIntradayDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private readonly string ARCHIVE_FOLDER = INTRADAY_ARCHIVE_SUBFOLDER + @"\VontobelIntraday";
        static private readonly string INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\VontobelIntraday";
        static private readonly string CONFIG_FILE = "VontobelIntradayDownload.cfg";
        static private readonly string VONTOBEL_ID_FILE = "VontobelUnderlyings.cfg";

        static public string VontobelUnderlyingFile => Path.Combine(Folders.PersonalFolder, VONTOBEL_ID_FILE);

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


            // Parse VontobelIntradayDownload.cfg file
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
            return $"https://markets.vontobel.com/api/v1/charts/products/{ticker}/detail/{period}/0?c=fr-fr&it=1";
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
                    string url = FormatIntradayURL(stockSerie.ISIN, "2");
                    if (DownloadHistory.ContainsKey(stockSerie.Symbol))
                    {
                        DownloadHistory[stockSerie.Symbol] = DateTime.Now;
                    }
                    else
                    {
                        DownloadHistory.Add(stockSerie.Symbol, DateTime.Now);
                    }
                    var jsonData = VontobelIntradayDataProvider.HttpGetFromVontobel(url);
                    if (jsonData == null)
                        return false;
                    var vontobelData = JsonConvert.DeserializeObject<VontobelJSon>(jsonData, Converter.Settings);
                    if (!vontobelData.isSuccess || vontobelData.payload == null)
                    {
                        MessageBox.Show(vontobelData.errorCode, $"Failed loading data from Vontobel for {stockSerie.StockName}");
                        return false;
                    }
                    stockSerie.IsInitialised = false;
                    this.LoadData(stockSerie);
                    DateTime lastDate = DateTime.MinValue;
                    if (stockSerie.Count > 0)
                    {
                        lastDate = stockSerie.Keys.Last();
                    }
                    else
                    {
                        lastDate = refDate.AddMilliseconds(vontobelData.payload.series[0].points.Min(p => p.timestamp)).ToLocalTime();
                    }
                    int nbNewBars = 0;
                    List<StockDailyValue> newBars = new List<StockDailyValue>();
                    foreach (var bar in vontobelData.payload.series[0].points.Reverse())
                    {
                        var newBar = new StockDailyValue(bar.bid, bar.bid, bar.bid, bar.bid, 0, refDate.AddMilliseconds(bar.timestamp).ToLocalTime());
                        newBars.Add(newBar);
                        //stockSerie.Add(newBar.DATE, newBar);
                        nbNewBars++;
                    }

                    if (nbNewBars > 0)
                    {
                        newBars = FixMinuteBars(newBars, 5);
                        foreach (var bar in newBars)
                        {
                            stockSerie.Add(bar.DATE, bar);
                        }

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
                        var stockSerie = new StockSerie(row[1], row[2], row[0], StockSerie.Groups.TURBO, StockDataProvider.VontobelIntraday, BarDuration.H_1);
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
                        StockLog.Write("Vontobel Intraday Entry: " + row[1] + " already in stockDictionary");
                    }
                }
            }
        }

        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            var path = Path.Combine(Folders.PersonalFolder, CONFIG_FILE);
            Process.Start(path);

            return DialogResult.OK;
        }

        public override string DisplayName => "Vontobel Intraday";

        public override void OpenInDataProvider(StockSerie stockSerie)
        {
            Process.Start($"https://markets.vontobel.com/fr-fr/produits/leverage/leverage-short/{stockSerie.ISIN}");
        }

        public override void ApplyTrimBefore(StockSerie stockSerie, DateTime date)
        {
            if (!stockSerie.Initialise())
                return;

            var archiveFileName = DataFolder + ARCHIVE_FOLDER + "\\" + stockSerie.Symbol.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
            stockSerie.SaveToCSVFromDateToDate(archiveFileName, date, stockSerie.LastValue.DATE);

            stockSerie.IsInitialised = false;
        }

        public override bool RemoveEntry(StockSerie stockSerie)
        {
            string fileName = Path.Combine(Folders.PersonalFolder, CONFIG_FILE);
            if (!File.Exists(fileName))
                return false;

            var lines = File.ReadAllLines(fileName).Where(l => !l.Contains(stockSerie.ISIN));
            File.WriteAllLines(fileName, lines);

            return true;
        }
    }
}
