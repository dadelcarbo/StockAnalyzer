using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockClasses.StockDataProviders.CitiFirst;
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
    public class CitifirstDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private readonly string ARCHIVE_FOLDER = INTRADAY_ARCHIVE_SUBFOLDER + @"\Citifirst";
        static private readonly string INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\Citifirst";
        static private readonly string CONFIG_FILE = "CitifirstDownload.cfg";
        static private readonly string CONFIG_FILE_USER = "CitifirstDownload.user.cfg";

        #region HttpClient

        static private HttpClient httpClient = null;
        static public HttpResponseMessage HttpGet(string productUrl)
        {
            try
            {
                if (httpClient == null)
                {
                    var handler = new HttpClientHandler();
                    handler.UseCookies = false;

                    // If you are using .NET Core 3.0+ you can replace `~DecompressionMethods.None` to `DecompressionMethods.All`
                    handler.AutomaticDecompression = ~DecompressionMethods.None;
                    httpClient = new HttpClient(handler);
                }
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), productUrl))
                {
                    request.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                    request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                    request.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
                    request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.71 Safari/537.36 Edg/94.0.992.38");
                    request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "none");
                    request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "navigate");
                    request.Headers.TryAddWithoutValidation("Sec-Fetch-User", "?1");
                    request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "document");
                    request.Headers.TryAddWithoutValidation("Accept-Language", "fr,fr-FR;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
                    request.Headers.TryAddWithoutValidation("Cookie", "_ga=GA1.2.942508426.1591082115; _gid=GA1.2.396338001.1633183129; DisclaimerAccepted=True; noMoreCookieWarning=true");

                    var response = httpClient.SendAsync(request).Result;
                    var productPage = response.Content.ReadAsStringAsync().Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        StockLog.Write("StatusCode: " + response.StatusCode + Environment.NewLine + productPage);
                        return null;
                    }

                    var index = productPage.IndexOf("/Data/Json/Chart?");
                    if (index == -1)
                    {
                        StockLog.Write("DataSource not found !!!");
                        return null;
                    }

                    var aspNetSessionId = string.Empty;
                    var antiXsrfToken = string.Empty;
                    foreach (var cookie in response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value)
                    {
                        var str = cookie.Split(';')[0].Split('=');
                        if (str[0] == "ASP.NET_SessionId")
                        {
                            aspNetSessionId = str[1];
                        }
                        if (str[0] == "__AntiXsrfToken")
                        {
                            antiXsrfToken = str[1];
                        }
                    }

                    var chartDataUrl = productPage.Substring(index);
                    chartDataUrl = "https://fr.citifirst.com" + chartDataUrl.Substring(0, chartDataUrl.IndexOf("}") - 2).Replace("\\u0026", "&");

                    using (var chartRequest = new HttpRequestMessage(new HttpMethod("GET"), chartDataUrl))
                    {
                        chartRequest.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                        chartRequest.Headers.TryAddWithoutValidation("sec-ch-ua", "^^");
                        chartRequest.Headers.TryAddWithoutValidation("Accept", "application/json, text/javascript, */*; q=0.01");
                        chartRequest.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                        chartRequest.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                        chartRequest.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.71 Safari/537.36 Edg/94.0.992.38");
                        chartRequest.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "^^");
                        chartRequest.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
                        chartRequest.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
                        chartRequest.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
                        chartRequest.Headers.TryAddWithoutValidation("Referer", productUrl);
                        chartRequest.Headers.TryAddWithoutValidation("Accept-Language", "fr,fr-FR;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
                        chartRequest.Headers.TryAddWithoutValidation("Cookie", $"_ga=GA1.2.942508426.1591082115; _gid=GA1.2.396338001.1633183129; DisclaimerAccepted=True; noMoreCookieWarning=true; ASP.NET_SessionId={aspNetSessionId}; __AntiXsrfToken={antiXsrfToken}; _gat=1");

                        return httpClient.SendAsync(chartRequest).Result;
                    }
                }
            }
            catch (Exception e)
            {
                StockLog.Write(e);
            }
            return null;
        }

        #endregion

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

            // Parse cfg file
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

            var fileName = DataFolder + INTRADAY_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

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
                var archiveFileName = DataFolder + ARCHIVE_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
                if (File.Exists(archiveFileName))
                    File.Delete(archiveFileName);
                var fileName = DataFolder + INTRADAY_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
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

                var fileName = DataFolder + INTRADAY_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

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
                using (var wc = new WebClient())
                {
                    wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

                    int nbTries = 2;
                    while (nbTries > 0)
                    {
                        try
                        {
                            var response = HttpGet(stockSerie.Url);
                            var content = response.Content.ReadAsStringAsync().Result;
                            if (response.IsSuccessStatusCode)
                            {
                                if (content.StartsWith("{"))
                                {
                                    File.WriteAllText(fileName, content);
                                    stockSerie.IsInitialised = false;
                                    return true;
                                }
                                StockLog.Write(content);
                                return false;
                            }
                            StockLog.Write(content);
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
                        if (!stockDictionary.ContainsKey(row[0]))
                        {
                            var fields = row[1].Split('/');

                            var stockSerie = new StockSerie(row[0], row[0],
                                StockSerie.Groups.INTRADAY,
                                StockDataProvider.Citifirst, BarDuration.M_10);
                            stockSerie.ProductType = fields[5];
                            stockSerie.Underlying = fields[6];
                            stockSerie.ISIN = fields[7];
                            stockSerie.Url = row[1];

                            var dailySerie = stockDictionary.Values.FirstOrDefault(s => !string.IsNullOrEmpty(s.ISIN) && s.ShortName == stockSerie.ShortName);
                            if (dailySerie != null)
                            {
                                stockSerie.ISIN = dailySerie.ISIN;
                            }
                            stockDictionary.Add(row[0], stockSerie);
                            if (download && this.needDownload)
                            {
                                this.needDownload = this.DownloadDailyData(stockSerie);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Investing Intraday Entry: " + row[2] + " already in stockDictionary");
                        }
                    }
                }
            }
        }

        static DateTime refDate = new DateTime(1970, 01, 01);
        private static bool ParseIntradayData(StockSerie stockSerie, string fileName)
        {
            var res = false;
            try
            {
                using (var sr = new StreamReader(fileName))
                {
                    var citifirstJson = CitiFirstSeries.FromJson(sr.ReadToEnd());
                    if (citifirstJson?.series == null || citifirstJson.series.Count == 0)
                        return false;
                    StockDailyValue previousValue = null;
                    foreach (var data in citifirstJson.series.First().data.Where(d => d.y != null))
                    {
                        var openDate = refDate.AddSeconds(data.x / 1000);
                        var value = data.y.Value;
                        if (!stockSerie.ContainsKey(openDate))
                        {
                            var dailyValue = new StockDailyValue(value, value, value, value,
                                   0,
                                   openDate);
                            stockSerie.Add(dailyValue.DATE, dailyValue);
                        }
                        //for (var i = 0; i < citifirstJson.data.Count; i++)
                        //{
                        //    if (citifirstJson.O[i] == 0 && citifirstJson.H[i] == 0 && citifirstJson.L[i] == 0 && citifirstJson.C[i] == 0)
                        //        continue;

                        //    var openDate = refDate.AddSeconds(citifirstJson.T[i]);
                        //    if (!stockSerie.ContainsKey(openDate))
                        //    {
                        //        var volString = citifirstJson.V[i];
                        //        long vol = 0;
                        //        long.TryParse(citifirstJson.V[i], out vol);
                        //        var dailyValue = new StockDailyValue(
                        //               citifirstJson.O[i],
                        //               citifirstJson.H[i],
                        //               citifirstJson.L[i],
                        //               citifirstJson.C[i],
                        //               vol,
                        //               openDate);
                        //        #region Add Missing 5 Minutes bars
                        //        if (previousValue != null && dailyValue.DATE.Day == previousValue.DATE.Day)
                        //        {
                        //            var date = previousValue.DATE.Add(minute5);
                        //            while (date < openDate && !stockSerie.ContainsKey(date))
                        //            {
                        //                // Create missing bars
                        //                var missingValue = new StockDailyValue(
                        //                       previousValue.CLOSE,
                        //                       previousValue.CLOSE,
                        //                       previousValue.CLOSE,
                        //                       previousValue.CLOSE,
                        //                       0,
                        //                       date);
                        //                stockSerie.Add(date, missingValue);
                        //                date = date.Add(minute5);
                        //            }
                        //        }
                        //        #endregion
                        //        stockSerie.Add(dailyValue.DATE, dailyValue);
                        //        previousValue = dailyValue;
                        //    }
                        //}
                        //stockSerie.ClearBarDurationCache();

                        res = true;
                    }
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
            Process.Start(Path.Combine(Folders.PersonalFolder, CONFIG_FILE_USER));
            return DialogResult.OK;
        }

        public string DisplayName => "Citifirst";
    }
}
