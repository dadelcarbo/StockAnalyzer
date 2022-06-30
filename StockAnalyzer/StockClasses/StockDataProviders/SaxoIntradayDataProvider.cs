using Newtonsoft.Json;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockWeb;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

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

        public string FormatIntradayURL(string ticker)
        {
            return $"https://fr-be.structured-products.saxo/page-api/instrument-service/charts/BE/isin/{ticker}/?timespan=1D&type=line&benchmarks=";
        }

        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            return DownloadIntradayData(stockSerie);
        }
        static SortedDictionary<long, DateTime> DownloadHistory = new SortedDictionary<long, DateTime>();
        public override bool DownloadIntradayData(StockSerie stockSerie)
        {
            if (stockSerie.Count > 0)
            {
                if (DownloadHistory.ContainsKey(stockSerie.Ticker) && DownloadHistory[stockSerie.Ticker] > DateTime.Now.AddMinutes(-2))
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
                    var url = FormatIntradayURL(stockSerie.ISIN);

                    try
                    {
                        if (DownloadHistory.ContainsKey(stockSerie.Ticker))
                        {
                            DownloadHistory[stockSerie.Ticker] = DateTime.Now;
                        }
                        else
                        {
                            DownloadHistory.Add(stockSerie.Ticker, DateTime.Now);
                        }
                        var jsonData = StockWebHelper.DownloadData(url);
                        var saxoData = JsonConvert.DeserializeObject<SaxoJSon>(jsonData, Converter.Settings);
                        if (saxoData?.series?[0]?.data == null)
                            return false;

                        stockSerie.IsInitialised = false;
                        this.LoadData(stockSerie);
                        DateTime lastDate = DateTime.Today.AddHours(8);
                        if (stockSerie.Count > 0)
                        {
                            if (stockSerie.Keys.Last().Date == DateTime.Today)
                            {
                                lastDate = stockSerie.Keys.Last();
                                stockSerie.RemoveLast();
                            }
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
                        if (!stockDictionary.ContainsKey(row[2]))
                        {
                            var stockSerie = new StockSerie(row[2], row[1], StockSerie.Groups.INTRADAY, StockDataProvider.SaxoIntraday, BarDuration.M_5);
                            stockSerie.ISIN = row[0];

                            stockDictionary.Add(row[2], stockSerie);
                            if (download && this.needDownload)
                            {
                                this.needDownload = this.DownloadDailyData(stockSerie);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Saxo Intraday Entry: " + row[2] + " already in stockDictionary");
                        }
                    }
                }
            }
        }

        static DateTime refDate = new DateTime(1970, 01, 01) + (DateTime.Now - DateTime.UtcNow);
        private static bool ParseIntradayData(StockSerie stockSerie, string fileName)
        {
            var res = false;
            try
            {
                using (var sr = new StreamReader(fileName))
                {
                    var barchartJson = BarChartJSon.FromJson(sr.ReadToEnd());

                    for (var i = 0; i < barchartJson.C.Length; i++)
                    {
                        if (barchartJson.O[i] == 0 && barchartJson.H[i] == 0 && barchartJson.L[i] == 0 && barchartJson.C[i] == 0)
                            continue;

                        var openDate = refDate.AddSeconds(barchartJson.T[i]);
                        if (!stockSerie.ContainsKey(openDate))
                        {
                            var volString = barchartJson.V[i];
                            long vol = 0;
                            long.TryParse(barchartJson.V[i], out vol);
                            var dailyValue = new StockDailyValue(
                                   barchartJson.O[i],
                                   barchartJson.H[i],
                                   barchartJson.L[i],
                                   barchartJson.C[i],
                                   vol,
                                   openDate);

                            stockSerie.Add(dailyValue.DATE, dailyValue);
                        }
                    }
                    stockSerie.ClearBarDurationCache();

                    res = true;
                }
            }
            catch (Exception e)
            {
                StockLog.Write("Unable to parse intraday data for " + stockSerie.StockName);
                StockLog.Write(e);
            }
            return res;
        }

        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            throw new NotImplementedException();
            //var configDlg = new SaxoIntradayDataProviderConfigDlg(stockDico, this.UserConfigFileName);
            //return configDlg.ShowDialog();
        }

        public string DisplayName => "Saxo Intraday";
    }
}
