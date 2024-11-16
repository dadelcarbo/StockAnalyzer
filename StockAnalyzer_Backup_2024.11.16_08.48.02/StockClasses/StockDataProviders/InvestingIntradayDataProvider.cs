using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class InvestingIntradayDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private readonly string ARCHIVE_FOLDER = INTRADAY_ARCHIVE_SUBFOLDER + @"\InvestingIntraday";
        static private readonly string INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\InvestingIntraday";
        static private readonly string CONFIG_FILE = "InvestingIntradayDownload.cfg";
        static private readonly string CONFIG_FILE_USER = "InvestingIntradayDownload.user.cfg";

        #region HttpClient

        static private HttpClient httpClient = null;
        static public string HttpGetFromInvesting2(string url)
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
                request.Headers.TryAddWithoutValidation("authority", "tvc6.investing.com");
                request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                request.Headers.TryAddWithoutValidation("accept-language", "fr,fr-FR;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
                request.Headers.TryAddWithoutValidation("cache-control", "max-age=0");
                request.Headers.TryAddWithoutValidation("cookie", "adBlockerNewUserDomains=1469044276; __gads=ID=8a3c5ca9165802f1:T=1569259598:S=ALNI_Masu3Qed7ImFzeIFflGdJZ2VpYuZA; __cf_bm=s20GsXRnJzDalMulh714_CjCTXney_GW2fL2RkTEN8I-1666118184-0-AV/AKIibsbx7T3UZAQRCO6MTMEVZRTTbympe+QVYiODm2TuF1VQDUFZ7Y8IeMfXuKGHPuPdkYySlYtcArEwXg/I=");
                request.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                request.Headers.TryAddWithoutValidation("sec-fetch-site", "none");
                request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
                request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36 Edg/106.0.1370.47");

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

        static private WebBrowser wb;
        static public string HttpGetFromInvesting(string url)
        {
            try
            {
                return null;
                if (wb == null)
                {
                    wb = new WebBrowser();
                    wb.ScriptErrorsSuppressed = true;
                    wb.Navigated += Wb_Navigated;
                }
                if (downloads.ContainsKey(url))
                {
                    string res = downloads[url];
                    downloads.Remove(url);
                    return res;
                }
                StockLog.Write("DL-HttpGetFromInvesting: " + Thread.CurrentThread.ManagedThreadId);
                Task.Run(() => DownloadData(url));

                int tries = 100;
                while (!downloads.ContainsKey(url) && --tries > 0)
                {
                    Task.Delay(100).Wait();
                }
                return null;
                //return downloads[url];
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            return null;
        }

        private static void DownloadData(object url)
        {
            while (wb.IsBusy)
            {
                StockLog.Write("DL-Wait busy web browser");
                Thread.Sleep(100);
            }
            StockLog.Write("DL-DownloadData: " + Thread.CurrentThread.ManagedThreadId);
            wb.Navigate(url.ToString());
        }

        static readonly SortedDictionary<string, string> downloads = new SortedDictionary<string, string>();
        private static void Wb_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            StockLog.Write("DL-Wb_Navigated: " + Thread.CurrentThread.ManagedThreadId);
            StockLog.Write(wb.DocumentText);
            downloads.Add(e.Url.ToString(), wb.DocumentText);
        }

        #endregion

        public override void InitDictionary(StockDictionary stockDictionary, bool download)
        {
            return;
            // Create data folder if not existing
            if (!Directory.Exists(DataFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ARCHIVE_FOLDER);
            }

            if (!Directory.Exists(DataFolder + INTRADAY_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + INTRADAY_FOLDER);
            }

            // Parse cfg file
            this.needDownload = download;
            InitFromFile(stockDictionary, download, Path.Combine(Folders.PersonalFolder, CONFIG_FILE));
            InitFromFile(stockDictionary, download, Path.Combine(Folders.PersonalFolder, CONFIG_FILE_USER));
        }

        public override bool SupportsIntradayDownload => true;

        public override bool LoadData(StockSerie stockSerie)
        {
            var archiveFileName = DataFolder + ARCHIVE_FOLDER + "\\" + stockSerie.Symbol.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
            if (File.Exists(archiveFileName))
            {
                stockSerie.ReadFromCSVFile(archiveFileName);
            }

            var fileName = DataFolder + INTRADAY_FOLDER + "\\" + stockSerie.Symbol.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

            if (File.Exists(fileName))
            {
                if (ParseIntradayData(stockSerie, fileName))
                {
                    stockSerie.Values.Last().IsComplete = false;
                    var lastDate = stockSerie.Keys.Last();

                    var firstArchiveDate = lastDate.AddMonths(-6).AddDays(-lastDate.Day + 1).Date;

                    stockSerie.SaveToCSVFromDateToDate(archiveFileName, firstArchiveDate, lastDate.AddDays(-5).Date);
                }
                else
                {
                    return false;
                }
            }
            return stockSerie.Count > 0;
        }

        public string FormatIntradayURL(long ticker, DateTime startDate)
        {
            var interval = 5;
            var from = (long)((startDate - refDate).TotalSeconds);
            var to = (long)((DateTime.Now - refDate).TotalSeconds);

            return $"{URL_PREFIX_INVESTING}/history?symbol={ticker}&resolution={interval}&from={from}&to={to}";
        }

        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            return true;
        }
        public override bool ForceDownloadData(StockSerie stockSerie)
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NotifyProgress("ForceDownload intraday for " + stockSerie.StockName);

                // Cleanup old files
                var archiveFileName = DataFolder + ARCHIVE_FOLDER + "\\" + stockSerie.Symbol.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
                if (File.Exists(archiveFileName))
                    File.Delete(archiveFileName);
                var fileName = DataFolder + INTRADAY_FOLDER + "\\" + stockSerie.Symbol.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
                if (File.Exists(fileName))
                    File.Delete(fileName);

                stockSerie.IsInitialised = false;
                this.DownloadIntradayData(stockSerie);
            }
            return true;
        }
        static bool first = true;
        public override bool DownloadIntradayData(StockSerie stockSerie)
        {
            StockLog.Write("DownloadIntradayData for " + stockSerie.StockName);
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NotifyProgress("Downloading intraday for " + stockSerie.StockName);

                var fileName = DataFolder + INTRADAY_FOLDER + "\\" + stockSerie.Symbol.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

                if (File.Exists(fileName))
                {
                    var lastWriteTime = File.GetLastWriteTime(fileName);
                    if (first && lastWriteTime > DateTime.Now.AddHours(-2)
                       || (DateTime.Today.DayOfWeek == DayOfWeek.Sunday && lastWriteTime.Date >= DateTime.Today.AddDays(-1))
                       || (DateTime.Today.DayOfWeek == DayOfWeek.Saturday && lastWriteTime.Date >= DateTime.Today))
                    {
                        if (!stockSerie.StockName.Contains("CC_"))
                        {
                            first = false;
                            return false;
                        }
                    }
                    else
                    {
                        var writeDate = File.GetLastWriteTime(fileName);
                        if (writeDate > DateTime.Now.AddSeconds(-59))
                        {
                            return false;
                        }
                    }
                }
                using var wc = new WebClient();
                wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

                var url = string.Empty;
                if (stockSerie.Initialise())
                {
                    url = FormatIntradayURL(stockSerie.Ticker, stockSerie.ValueArray[stockSerie.LastCompleteIndex].DATE.Date.AddDays(-7));
                }
                else
                {
                    url = FormatIntradayURL(stockSerie.Ticker, DateTime.Today.AddDays(-80));
                }

                int nbTries = 2;
                while (nbTries > 0)
                {
                    try
                    {
                        var response = HttpGetFromInvesting(url);
                        if (!string.IsNullOrEmpty(response))
                        {
                            if (response.StartsWith("{"))
                            {
                                File.WriteAllText(fileName, response);
                                stockSerie.IsInitialised = false;
                                return true;
                            }
                            StockLog.Write(response);
                            return false;
                        }
                        nbTries--;
                    }
                    catch (Exception ex)
                    {
                        nbTries--;
                        StockLog.Write(ex);
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
                using var sr = new StreamReader(fileName, true);
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                    if (line.StartsWith("$")) break;

                    var row = line.Split(',');
                    if (!stockDictionary.ContainsKey(row[2]))
                    {
                        var stockSerie = new StockSerie(row[2], row[1],
                            (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[3]),
                            StockDataProvider.InvestingIntraday, BarDuration.M_5);
                        stockSerie.Ticker = long.Parse(row[0]);

                        //var dailySerie = stockDictionary.Values.FirstOrDefault(s => !string.IsNullOrEmpty(s.ISIN) && s.ShortName == stockSerie.ShortName);
                        //if (dailySerie != null)
                        //{
                        //    stockSerie.ISIN = dailySerie.ISIN;
                        //}
                        stockDictionary.Add(row[2], stockSerie);
                        if (download && this.needDownload)
                        {
                            this.needDownload = this.DownloadDailyData(stockSerie);
                        }
                    }
                    else
                    {
                        StockLog.Write("Investing Intraday Entry: " + row[2] + " already in stockDictionary");
                    }
                }
            }
        }

        private static bool ParseIntradayData(StockSerie stockSerie, string fileName)
        {
            var res = false;
            try
            {
                using var sr = new StreamReader(fileName);
                var barchartJson = BarChartJSon.FromJson(sr.ReadToEnd());
                if (barchartJson == null || barchartJson.C == null)
                    return false;
                StockDailyValue previousValue = null;
                var minute5 = new TimeSpan(0, 5, 0);
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
                        #region Add Missing 5 Minutes bars
                        if (previousValue != null && dailyValue.DATE.Day == previousValue.DATE.Day)
                        {
                            var date = previousValue.DATE.Add(minute5);
                            while (date < openDate && !stockSerie.ContainsKey(date))
                            {
                                // Create missing bars
                                var missingValue = new StockDailyValue(
                                       previousValue.CLOSE,
                                       previousValue.CLOSE,
                                       previousValue.CLOSE,
                                       previousValue.CLOSE,
                                       0,
                                       date);
                                stockSerie.Add(date, missingValue);
                                date = date.Add(minute5);
                            }
                        }
                        #endregion
                        stockSerie.Add(dailyValue.DATE, dailyValue);
                        previousValue = dailyValue;
                    }
                }
                stockSerie.ClearBarDurationCache();

                res = true;
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
            var configDlg = new InvestingIntradayDataProviderConfigDlg(stockDico, CONFIG_FILE_USER) { StartPosition = FormStartPosition.CenterScreen };
            return configDlg.ShowDialog();
        }

        public override string DisplayName => "Investing Intraday";
    }
}
