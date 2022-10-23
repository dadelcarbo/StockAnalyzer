using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockClasses.StockDataProviders.Yahoo;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using StockAnalyzerSettings.Properties;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class YahooIntradayDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private string FOLDER = @"\intraday\YahooIntraday";
        static private string ARCHIVE_FOLDER = @"\archive\intraday\YahooIntraday";

        static private string CONFIG_FILE = "YahooIntradayDownload.cfg";
        static private string CONFIG_FILE_USER = "YahooIntradayDownload.user.cfg";

        public override void InitDictionary(StockDictionary stockDictionary, bool download)
        {
            // Parse YahooIntraday.cfg file// Create data folder if not existing
            if (!Directory.Exists(DataFolder + FOLDER))
            {
                Directory.CreateDirectory(DataFolder + FOLDER);
            }
            if (!Directory.Exists(DataFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ARCHIVE_FOLDER);
            }

            this.needDownload = download;

            // Parse YahooIntradayDownload.cfg file
            InitFromFile(stockDictionary, download, Path.Combine(Folders.PersonalFolder, CONFIG_FILE));
            InitFromFile(stockDictionary, download, Path.Combine(Folders.PersonalFolder, CONFIG_FILE_USER));
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
                        var stockName = row[1];
                        if (!stockDictionary.ContainsKey(stockName))
                        {
                            var stockSerie = new StockSerie(stockName, row[0], (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[2]), StockDataProvider.YahooIntraday, BarDuration.M_5);
                            stockDictionary.Add(stockName, stockSerie);

                            if (RefSerie == null && download) // Check if provider is up to date by checking the reference serie
                            {
                                RefSerie = stockSerie;
                                // Check if download needed.
                                stockSerie.Initialise();
                                DateTime refDate = DateTime.MinValue;
                                if (stockSerie.Count > 0)
                                {
                                    refDate = stockSerie.Keys.Last();
                                }
                                this.DownloadDailyData(stockSerie);
                                stockSerie.Initialise();
                                needDownload = refDate < stockSerie.Keys.Last();
                            }
                            else
                            {
                                if (download && this.needDownload)
                                {
                                    this.DownloadDailyData(stockSerie);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("YahooIntraday Daily Entry: " + row[2] + " already in stockDictionary");
                        }
                    }
                }
            }
        }

        public override bool SupportsIntradayDownload => false;

        public override bool LoadData(StockSerie stockSerie)
        {
            StockLog.Write("LoadData for " + stockSerie.StockName);
            bool res = false;
            var archiveFileName = DataFolder + ARCHIVE_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
            if (File.Exists(archiveFileName))
            {
                stockSerie.ReadFromCSVFile(archiveFileName);
                res = true;
            }

            var fileName = DataFolder + FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

            if (File.Exists(fileName))
            {
                if (ParseDailyData(stockSerie, fileName))
                {
                    var lastDate = stockSerie.Keys.Last();

                    stockSerie.SaveToCSVFromDateToDate(archiveFileName, stockSerie.Keys.First(), lastDate);
                    File.Delete(fileName);
                }
                else
                {
                    return false;
                }
                res = true;
            }
            return res;
        }

        static DateTime refDate = new DateTime(1970, 01, 01);
        public string FormatURL(string ticker, DateTime startDate, DateTime endDate, string interval)
        {
            var startTime = (int)(startDate - refDate).TotalSeconds;
            var endTime = (int)(endDate - refDate).TotalSeconds;

            return $"https://query1.finance.yahoo.com/v8/finance/chart/{ticker}?symbol={ticker}&period1={startTime}&period2={endTime}&useYfid=true&interval={interval}&includePrePost=false&events=div%7Csplit%7Cearn&lang=en-US&region=US&crumb=8hqjAI5r.C2&corsDomain=finance.yahoo.com";
        }

        public override bool ForceDownloadData(StockSerie stockSerie)
        {
            StockLog.Write("ForceDownloadData for " + stockSerie.StockName);
            var archiveFileName = DataFolder + ARCHIVE_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
            if (File.Exists(archiveFileName))
            {
                File.Delete(archiveFileName);
            }
            var fileName = DataFolder + FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            stockSerie.IsInitialised = false;
            first = true;
            return this.DownloadDailyData(stockSerie);
        }

        static bool first = true;
        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            StockLog.Write("DownloadDailyData for " + stockSerie.StockName);
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NotifyProgress("Downloading daily data for " + stockSerie.StockName);

                var fileName = DataFolder + FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

                if (File.Exists(fileName))
                {
                    var lastWriteTime = File.GetLastWriteTime(fileName);
                    if (first && (lastWriteTime > DateTime.Now.AddHours(-2)
                       || (DateTime.Today.DayOfWeek == DayOfWeek.Sunday && lastWriteTime.Date >= DateTime.Today.AddDays(-1))
                       || (DateTime.Today.DayOfWeek == DayOfWeek.Saturday && lastWriteTime.Date >= DateTime.Today)))
                    {
                        if (stockSerie != RefSerie)
                        {
                            first = false;
                            return false;
                        }
                    }
                    else
                    {
                        if (File.GetLastWriteTime(fileName) > DateTime.Now.AddMinutes(-2))
                            return false;
                    }
                }
                using (var wc = new WebClient())
                {
                    wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

                    var url = string.Empty;
                    var lastDate = new DateTime(ARCHIVE_START_YEAR, 1, 1);
                    if (stockSerie.Initialise() && stockSerie.Count > 0)
                    {
                        lastDate = stockSerie.ValueArray[stockSerie.LastCompleteIndex].DATE.Date;
                        if (lastDate >= DateTime.Today)
                            return false;
                        url = FormatURL(stockSerie.ShortName, lastDate.AddDays(-2), DateTime.Today, "5m");
                    }
                    else
                    {
                        url = FormatURL(stockSerie.ShortName, DateTime.Today.AddDays(-10), DateTime.Now, "5m");
                    }

                    int nbTries = 2;
                    while (nbTries > 0)
                    {
                        try
                        {
                            var response = YahooDataProvider.HttpGetFromYahoo(url);
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
                            StockLog.Write(response);
                            nbTries--;
                        }
                        catch (Exception ex)
                        {
                            nbTries--;
                            StockLog.Write(ex);
                        }
                    }
                }
            }
            return false;
        }

        static private HttpClient httpClient = null;
        private static string HttpGetFromYahoo(string url)
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
                    //request.Headers.TryAddWithoutValidation("authority", "tvc6.investing.com");
                    //request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    //request.Headers.TryAddWithoutValidation("accept-language", "fr,fr-FR;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
                    //request.Headers.TryAddWithoutValidation("cache-control", "max-age=0");
                    //request.Headers.TryAddWithoutValidation("cookie", "adBlockerNewUserDomains=1469044276; __gads=ID=8a3c5ca9165802f1:T=1569259598:S=ALNI_Masu3Qed7ImFzeIFflGdJZ2VpYuZA; __cf_bm=s20GsXRnJzDalMulh714_CjCTXney_GW2fL2RkTEN8I-1666118184-0-AV/AKIibsbx7T3UZAQRCO6MTMEVZRTTbympe+QVYiODm2TuF1VQDUFZ7Y8IeMfXuKGHPuPdkYySlYtcArEwXg/I=");
                    //request.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                    //request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                    //request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                    //request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                    //request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                    //request.Headers.TryAddWithoutValidation("sec-fetch-site", "none");
                    //request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
                    //request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                    //request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36 Edg/106.0.1370.47");

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

        private static bool ParseDailyData(StockSerie stockSerie, string fileName)
        {
            var res = false;
            try
            {
                using (var sr = new StreamReader(fileName))
                {
                    var yahooJson = YahooJson.FromJson(sr.ReadToEnd());
                    if (!string.IsNullOrEmpty(yahooJson?.chart?.error))
                    {
                        StockLog.Write($"Error loading {stockSerie.StockName}: {yahooJson?.chart?.error}");
                    }

                    int i = 0;
                    var priceInt = yahooJson.chart.result[0].meta.priceHint;
                    var quote = yahooJson.chart.result[0].indicators.quote[0];
                    foreach (var timestamp in yahooJson.chart.result[0].timestamp)
                    {
                        if (quote.open[i] == null || quote.high[i] == null || quote.low[i] == null || quote.close[i] == null)
                        {
                            i++;
                            continue;
                        }
                        var openDate = refDate.AddSeconds(timestamp).ToLocalTime();
                        if (!stockSerie.ContainsKey(openDate))
                        {
                            long vol = quote.volume[i].HasValue ? quote.volume[i].Value : 0;
                            var dailyValue = new StockDailyValue(
                                   (float)Math.Round(quote.open[i].Value, priceInt),
                                   (float)Math.Round(quote.high[i].Value, priceInt),
                                   (float)Math.Round(quote.low[i].Value, priceInt),
                                   (float)Math.Round(quote.close[i].Value, priceInt),
                                   vol,
                                   openDate);

                            stockSerie.Add(dailyValue.DATE, dailyValue);
                        }
                        i++;
                    }
                    stockSerie.ClearBarDurationCache();

                    res = true;
                }
            }
            catch (Exception e)
            {
                StockLog.Write("Unable to parse daily data for " + stockSerie.StockName);
                StockLog.Write(e);
            }
            return res;
        }

        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            var configDlg = new YahooDataProviderConfigDlg(stockDico, Path.Combine(Folders.PersonalFolder, CONFIG_FILE_USER));
            return configDlg.ShowDialog();
        }

        public string DisplayName => "Yahoo Intraday";
    }
}
