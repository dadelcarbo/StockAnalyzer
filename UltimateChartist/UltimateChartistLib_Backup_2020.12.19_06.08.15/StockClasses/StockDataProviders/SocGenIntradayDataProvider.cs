using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockWeb;
using StockAnalyzerApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class SocGenIntradayDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private readonly string ARCHIVE_FOLDER = INTRADAY_ARCHIVE_SUBFOLDER + @"\SocGenIntraday";
        static private readonly string INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\SocGenIntraday";
        static private readonly string CONFIG_FILE = @"\SocGenIntradayDownload.cfg";
        static private readonly string CONFIG_FILE_USER = @"\SocGenIntradayDownload.user.cfg";

        public string UserConfigFileName => CONFIG_FILE_USER;

        public override bool LoadIntradayDurationArchiveData(string rootFolder, StockSerie serie, StockBarDuration duration)
        {
            StockLog.Write("LoadIntradayDurationArchiveData Name:" + serie.StockName + " duration:" + duration);
            var durationFileName = rootFolder + ARCHIVE_FOLDER + "\\" + duration + "\\" + serie.ShortName.Replace(':', '_') + "_" + serie.StockName + "_" + serie.StockGroup.ToString() + ".txt";
            if (File.Exists(durationFileName))
            {
                var values = serie.GetValues(duration);
                if (values == null)
                    StockLog.Write("LoadIntradayDurationArchiveData Cache File Found, current size is: 0");
                else StockLog.Write("LoadIntradayDurationArchiveData Cache File Found, current size is: " + values.Count);
                serie.ReadFromCSVFile(durationFileName, duration);


                StockLog.Write("LoadIntradayDurationArchiveData New serie size is: " + serie.GetValues(duration).Count);
                if (serie.GetValues(duration).Count > 0)
                {
                    StockLog.Write("LoadIntradayDurationArchiveData First bar: " + serie.GetValues(duration).First().ToString());
                    StockLog.Write("LoadIntradayDurationArchiveData Last bar: " + serie.GetValues(duration).Last().ToString());
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
        {
            // Create data folder if not existing
            if (!Directory.Exists(rootFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + ARCHIVE_FOLDER);
            }
            if (!Directory.Exists(rootFolder + INTRADAY_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + INTRADAY_FOLDER);
            }

            // Parse SocGenIntradayDownload.cfg file
            this.needDownload = download;
            InitFromFile(rootFolder, stockDictionary, download, rootFolder + CONFIG_FILE);
            InitFromFile(rootFolder, stockDictionary, download, rootFolder + CONFIG_FILE_USER);
        }

        public override bool SupportsIntradayDownload => true;

        public override bool LoadData(string rootFolder, StockSerie stockSerie)
        {
            var archiveFileName = rootFolder + ARCHIVE_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
            if (File.Exists(archiveFileName))
            {
                stockSerie.ReadFromCSVFile(archiveFileName);
            }
            return stockSerie.Count > 0;
        }

        public string FormatIntradayURL(long ticker)
        {
            //return $"https://bourse.societegenerale.fr/product-detail?productId={ticker}";
            return $"https://sgbourse.fr/EmcWebApi/api/Prices/Intraday?productId={ticker}";
        }

        public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
        {
            return true;
        }
        static SortedDictionary<long, DateTime> DownloadHistory = new SortedDictionary<long, DateTime>();
        public override bool DownloadIntradayData(string rootFolder, StockSerie stockSerie)
        {
            if (stockSerie.Count > 0 && DownloadHistory.ContainsKey(stockSerie.Ticker) && DownloadHistory[stockSerie.Ticker] > DateTime.Now.AddMinutes(-2))
            {
                return false;  // Do not download more than every 2 minutes.
            }

            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NotifyProgress("Downloading intraday for " + stockSerie.StockName);

                using (var wc = new WebClient())
                {
                    wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    var url = FormatIntradayURL(stockSerie.Ticker);

                    try
                    {
                        var entries = StockWebHelper.DownloadData(url).Replace("[", "").Replace("]", "").Replace("},{", "|").Replace("{", "").Replace("}", "").Split('|');
                        if (DownloadHistory.ContainsKey(stockSerie.Ticker))
                        {
                            DownloadHistory[stockSerie.Ticker] = DateTime.Now;
                        }
                        else
                        {
                            DownloadHistory.Add(stockSerie.Ticker, DateTime.Now);
                        }
                        stockSerie.IsInitialised = false;
                        this.LoadData(rootFolder, stockSerie);
                        DateTime lastDate = stockSerie.Count > 0 ? stockSerie.Keys.Last().Date.AddDays(1) : DateTime.MinValue;
                        var values = new Dictionary<DateTime, float>();
                        DateTime date;
                        float value;
                        foreach (var entry in entries)
                        {
                            var fields = entry.Replace("\"", "").Split(',');
                            var dateText = fields.First(e => e.StartsWith("Date")).Replace("Date:", "");
                            date = DateTime.Parse(dateText, Global.FrenchCulture);
                            if (date > lastDate)
                            {
                                float ask = float.Parse(fields.First(e => e.StartsWith("Ask")).Split(':')[1]);
                                float bid = float.Parse(fields.First(e => e.StartsWith("Bid")).Split(':')[1]);
                                value = ask == 0 ? bid : ask;

                                values.Add(date, value);
                            }
                        }
                        if (values.Count == 0)
                            return true;
                        // Make 5 minutes bar
                        date = values.First().Key;
                        value = values.First().Value;
                        var minute = (date.Minute / 5) * 5;
                        StockDailyValue newBar = new StockDailyValue(value, value, value, value, 0, new DateTime(date.Year, date.Month, date.Day, date.Hour, minute, 0));

                        foreach (var data in values.Skip(1))
                        {
                            date = data.Key;
                            value = data.Value;
                            minute = (date.Minute / 5) * 5;

                            if (minute == newBar.DATE.Minute)
                            {
                                newBar.HIGH = Math.Max(newBar.HIGH, value);
                                newBar.LOW = Math.Min(newBar.LOW, value);
                                newBar.CLOSE = value;
                            }
                            else
                            {
                                stockSerie.Add(newBar.DATE, newBar);
                                var newDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, minute, 0);
                                newBar = new StockDailyValue(newBar.CLOSE, value, value, value, 0, newDate);
                            }
                        }
                        if (newBar != null)
                        {
                            newBar.IsComplete = false;
                            stockSerie.Add(newBar.DATE, newBar);
                        }

                        var firstArchiveDate = stockSerie.Keys.Last().AddMonths(-2).AddDays(-lastDate.Day + 1).Date;
                        var archiveFileName = rootFolder + ARCHIVE_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

                        stockSerie.SaveToCSVFromDateToDate(archiveFileName, firstArchiveDate, stockSerie.Keys.Last().Date);

                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return false;
        }
        private void InitFromFile(string rootFolder, StockDictionary stockDictionary, bool download, string fileName)
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
                            var stockSerie = new StockSerie(row[2], row[1], StockSerie.Groups.INTRADAY, StockDataProvider.SocGenIntraday);
                            stockSerie.Ticker = long.Parse(row[0]);

                            stockDictionary.Add(row[2], stockSerie);
                            if (download && this.needDownload)
                            {
                                this.needDownload = this.DownloadDailyData(rootFolder, stockSerie);
                            }
                        }
                        else
                        {
                            Console.WriteLine("SocGen Intraday Entry: " + row[2] + " already in stockDictionary");
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
            catch (System.Exception e)
            {
                StockLog.Write("Unable to parse intraday data for " + stockSerie.StockName);
                StockLog.Write(e);
            }
            return res;
        }

        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            throw new NotImplementedException();
            //var configDlg = new SocGenIntradayDataProviderConfigDlg(stockDico, this.UserConfigFileName);
            //return configDlg.ShowDialog();
        }

        public string DisplayName => "SocGen Intraday";
    }
}
