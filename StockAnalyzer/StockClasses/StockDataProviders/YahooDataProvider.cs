using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockClasses.StockDataProviders.Yahoo;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class YahooDataProvider : StockDataProviderBase, IConfigDialog
    {
        private static readonly string FOLDER = @"\daily\Yahoo";
        private static readonly string ARCHIVE_FOLDER = @"\archive\daily\Yahoo";

        private static readonly string CONFIG_FILE = "YahooDownload.cfg";
        private static readonly string CONFIG_FILE_USER = "YahooDownload.user.cfg";

        public override void InitDictionary(StockDictionary stockDictionary, bool download)
        {
            // Parse Yahoo.cfg file// Create data folder if not existing
            if (!Directory.Exists(DataFolder + FOLDER))
            {
                Directory.CreateDirectory(DataFolder + FOLDER);
            }
            if (!Directory.Exists(DataFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ARCHIVE_FOLDER);
            }

            this.needDownload = download;

            // Parse YahooDownload.cfg file
            InitFromFile(stockDictionary, download, Path.Combine(Folders.PersonalFolder, CONFIG_FILE));
            InitFromFile(stockDictionary, download, Path.Combine(Folders.PersonalFolder, CONFIG_FILE_USER));
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

                    var row = line.Split(',');
                    var stockName = row[1];
                    if (!stockDictionary.ContainsKey(stockName))
                    {
                        var stockSerie = new StockSerie(stockName, row[0], (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[2]), StockDataProvider.Yahoo, BarDuration.Daily);
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
                        StockLog.Write("Yahoo Daily Entry: " + row[2] + " already in stockDictionary");
                    }
                }
            }
        }

        public override bool SupportsIntradayDownload => true;

        public override bool DownloadIntradayData(StockSerie stockSerie)
        {
            var url = this.FormatQuoteURL(stockSerie.Symbol);
            var response = YahooDataProvider.HttpGetFromYahoo(url, stockSerie.Symbol);
            if (!string.IsNullOrEmpty(response))
            {
                if (response.StartsWith("{"))
                {
                    var fileName = DataFolder + FOLDER + "\\Quote_" + stockSerie.Symbol.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
                    File.WriteAllText(fileName, response);
                    stockSerie.IsInitialised = false;
                    return true;
                }
                StockLog.Write(response);
                return false;
            }
            return true;
        }

        public override bool LoadData(StockSerie stockSerie)
        {
            StockLog.Write("LoadData for " + stockSerie.StockName);
            bool res = false;
            var archiveFileName = DataFolder + ARCHIVE_FOLDER + "\\" + stockSerie.Symbol.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
            if (File.Exists(archiveFileName))
            {
                stockSerie.ReadFromCSVFile(archiveFileName);
                res = true;
            }

            var fileName = DataFolder + FOLDER + "\\" + stockSerie.Symbol.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

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

            fileName = DataFolder + FOLDER + "\\Quote_" + stockSerie.Symbol.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
            if (File.Exists(fileName))
            {
                try
                {
                    var yahooQuoteResult = YahooQuoteResult.FromJson(File.ReadAllText(fileName));
                    if (!string.IsNullOrEmpty(yahooQuoteResult?.quoteResponse?.error))
                    {
                        StockLog.Write($"Error loading {stockSerie.StockName}: {yahooQuoteResult?.quoteResponse?.error}");
                    }
                    var quote = yahooQuoteResult.quoteResponse.result[0];
                    var openDate = refDate.AddSeconds(quote.regularMarketTime).AddMilliseconds(quote.gmtOffSetMilliseconds);
                    if (!stockSerie.ContainsKey(openDate.Date))
                    {
                        long vol = quote.regularMarketVolume;
                        var dailyValue = new StockDailyValue(
                               (float)Math.Round(quote.regularMarketOpen, quote.priceHint),
                               (float)Math.Round(quote.regularMarketDayHigh, quote.priceHint),
                               (float)Math.Round(quote.regularMarketDayLow, quote.priceHint),
                               (float)Math.Round(quote.regularMarketPrice, quote.priceHint),
                               vol,
                               openDate);

                        stockSerie.Add(dailyValue.DATE, dailyValue);
                    }
                }
                catch (Exception ex)
                {
                    StockLog.Write($"Exception loading Quote for {stockSerie.StockName}: {ex.Message}");
                }
                File.Delete(fileName);
            }
            return res;
        }

        public string FormatURL(string symbol, DateTime startDate, DateTime endDate, string interval)
        {
            var startTime = (int)(startDate - refDate).TotalSeconds;
            var endTime = (int)(endDate - refDate).TotalSeconds;

            return $"https://query2.finance.yahoo.com/v8/finance/chart/{symbol}?period1={startTime}&period2={endTime}&interval={interval}&includePrePost=false&events=div%7Csplit%7Cearn&lang=en-US&region=US";
        }

        public string FormatQuoteURL(string symbol)
        {
            return $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={symbol}";
        }

        public override bool ForceDownloadData(StockSerie stockSerie)
        {
            StockLog.Write("ForceDownloadData for " + stockSerie.StockName);
            var archiveFileName = DataFolder + ARCHIVE_FOLDER + "\\" + stockSerie.Symbol.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
            if (File.Exists(archiveFileName))
            {
                File.Delete(archiveFileName);
            }
            var fileName = DataFolder + FOLDER + "\\" + stockSerie.Symbol.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
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

                var fileName = DataFolder + FOLDER + "\\" + stockSerie.Symbol.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

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
                using var wc = new WebClient();
                wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

                var url = string.Empty;
                var lastDate = new DateTime(ARCHIVE_START_YEAR, 1, 1);
                if (stockSerie.Initialise() && stockSerie.Count > 0)
                {
                    lastDate = stockSerie.ValueArray[stockSerie.LastCompleteIndex].DATE.Date;
                    if (lastDate >= DateTime.Today)
                        return false;
                    url = FormatURL(stockSerie.Symbol, new DateTime(lastDate.Year, lastDate.Month, 1), DateTime.Today, "1d");
                }
                else
                {
                    url = FormatURL(stockSerie.Symbol, lastDate, DateTime.Today, "1d");
                }

                int nbTries = 2;
                while (nbTries > 0)
                {
                    try
                    {
                        var response = YahooDataProvider.HttpGetFromYahoo(url, stockSerie.Symbol);
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

        static private HttpClient httpClient = null;
        public static string HttpGetFromYahoo(string url, string symbol)
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
                request.Headers.TryAddWithoutValidation("authority", "query1.finance.yahoo.com");
                request.Headers.TryAddWithoutValidation("accept", "*/*");
                request.Headers.TryAddWithoutValidation("accept-language", "en-GB,en;q=0.9,fr;q=0.8");
                request.Headers.TryAddWithoutValidation("cookie", "GUC=AQABBwFjUTNjgUIjOQUK&s=AQAAAHQwhHqy&g=Y0_klA; A1=d=AQABBDrgTGECEJASw6tugP5gurwL3tTZJbsFEgABBwEzUWOBY-Uzb2UB9qMAAAcIOuBMYdTZJbs&S=AQAAAtchReKAiYT-dd18q6pme9o; A3=d=AQABBDrgTGECEJASw6tugP5gurwL3tTZJbsFEgABBwEzUWOBY-Uzb2UB9qMAAAcIOuBMYdTZJbs&S=AQAAAtchReKAiYT-dd18q6pme9o; EuConsent=CPP_3gAPP_3gAAOACBFRClCoAP_AAH_AACiQIjNd_Hf_bX9n-f596ft0eY1f9_r3ruQzDhfNk-8F2L_W_LwX_2E7NB36pq4KmR4ku1LBIQNtHMnUDUmxaokVrzHsak2MpyNKJ7BkknsZe2dYGFtPm5lD-QKZ7_5_d3f52T_9_9v-39z33913v3d93-_12LjdV591H_v9fR_bc_Kdt_5-AAAAAAAAEEEQCTDEvIAuxLHAk0DSqFECMKwkKgFABBQDC0TWADA4KdlYBHqCFgAhNQEYEQIMQUYEAgAEAgCQiACQAsEACAIgEAAIAUICEABAwCCwAsDAIAAQDQMQAoABAkIMjgqOUwICIFogJbKwBKKqY0wgDLLACgERkVEAiAIAEgICAsHEMASAlADDQAYAAgkEIgAwABBIIVABgACCQQA; thamba=2; cmp=t=1666643858&j=1&u=1---&v=56; A1S=d=AQABBDrgTGECEJASw6tugP5gurwL3tTZJbsFEgABBwEzUWOBY-Uzb2UB9qMAAAcIOuBMYdTZJbs&S=AQAAAtchReKAiYT-dd18q6pme9o&j=GDPR; PRF=t^%^3DTSLA^%^252BAAPL^%^252B^%^255EFCHI^%^252BMC.PA^%^252BES^%^253DF^%^252BNQ^%^253DF^%^252BSI^%^253DF^%^252BCC^%^253DF^%^252BADYEN.AS^%^252BAVT.PA^%^252BAC.PA");
                request.Headers.TryAddWithoutValidation("origin", "https://finance.yahoo.com");
                request.Headers.TryAddWithoutValidation("referer", $"https://finance.yahoo.com/quote/{symbol}/chart?p={symbol}");
                request.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                request.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
                request.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
                request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-site");
                request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36");

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

        private static bool ParseDailyData(StockSerie stockSerie, string fileName)
        {
            var res = false;
            try
            {
                using var sr = new StreamReader(fileName);
                var yahooJson = YahooJson.FromJson(sr.ReadToEnd());
                if (!string.IsNullOrEmpty(yahooJson?.chart?.error))
                {
                    StockLog.Write($"Error loading {stockSerie.StockName}: {yahooJson?.chart?.error}");
                }

                int i = 0;
                var priceHint = yahooJson.chart.result[0].meta.priceHint;
                var quote = yahooJson.chart.result[0].indicators.quote[0];
                foreach (var timestamp in yahooJson.chart.result[0].timestamp)
                {
                    if (quote.open[i] == null || quote.high[i] == null || quote.low[i] == null || quote.close[i] == null)
                    {
                        i++;
                        continue;
                    }
                    var openDate = refDate.AddSeconds(timestamp).Date;
                    if (!stockSerie.ContainsKey(openDate))
                    {
                        long vol = quote.volume[i].HasValue ? quote.volume[i].Value : 0;
                        var dailyValue = new StockDailyValue(
                               (float)Math.Round(quote.open[i].Value, priceHint),
                               (float)Math.Round(quote.high[i].Value, priceHint),
                               (float)Math.Round(quote.low[i].Value, priceHint),
                               (float)Math.Round(quote.close[i].Value, priceHint),
                               vol,
                               openDate);

                        stockSerie.Add(dailyValue.DATE, dailyValue);
                    }
                    i++;
                }
                stockSerie.ClearBarDurationCache();

                res = true;
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

        public override string DisplayName => "Yahoo";

        public override void OpenInDataProvider(StockSerie stockSerie)
        {
            Process.Start($"https://finance.yahoo.com/quote/{stockSerie.Symbol}");
        }
    }
}
