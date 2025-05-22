using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Windows.Forms;
using System.Linq;
using System.Diagnostics;
using System.Text.Json;

namespace StockAnalyzer.StockClasses.StockDataProviders.CNN
{
    public class CnnDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private readonly string ARCHIVE_FOLDER = DAILY_ARCHIVE_SUBFOLDER + @"\Cnn";
        static private readonly string DAILY_FOLDER = DAILY_SUBFOLDER + @"\Cnn";
        private static readonly string WEB_CACHE_FOLDER = DAILY_FOLDER + @"\WebCache";

        public override void InitDictionary(StockDictionary stockDictionary, bool download)
        {
            // Create data folder if not existing
            if (!Directory.Exists(DataFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ARCHIVE_FOLDER);
            }
            if (!Directory.Exists(DataFolder + DAILY_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + DAILY_FOLDER);
            }
            if (!Directory.Exists(DataFolder + WEB_CACHE_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + WEB_CACHE_FOLDER);
            }

            // Parse CnnDownload.cfg file
            var longName = "FEAR_GREED";
            if (!stockDictionary.ContainsKey(longName))
            {
                stockDictionary.Add(longName, new StockSerie(longName, "FnG", StockSerie.Groups.BREADTH, StockDataProvider.CNN, BarDuration.Daily));
            }
            needDownload = false;
        }

        public override bool SupportsIntradayDownload => false;

        public override bool LoadData(StockSerie stockSerie)
        {
            // Read archive first
            string fileName = stockSerie.Symbol + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv";
            string fullFileName = DataFolder + ARCHIVE_FOLDER + "\\" + fileName;
            bool res = ParseCSVFile(stockSerie, fullFileName);

            fullFileName = DataFolder + DAILY_FOLDER + "\\" + fileName;
            res = ParseCSVFile(stockSerie, fullFileName) || res;
            if (stockSerie.Count > 0)
            {
                stockSerie.PreInitialise();
            }

            DownloadDailyData(stockSerie);
            return res;
        }

        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            var cac40 = StockDictionary.Instance["CAC40"];
            cac40.Initialise();
            if (stockSerie.Count == 0 || cac40.Keys.Last() > stockSerie.LastValue.DATE)
            {
                var json = HttpGet("https://production.dataviz.cnn.io/index/fearandgreed/graphdata");
                if (string.IsNullOrEmpty(json))
                    return false;

                try
                {
                    var data = JsonSerializer.Deserialize<FearAndGreedData>(json.Replace("0.0,", "0,"));
                    if (data?.fear_and_greed_historical?.data == null || data.fear_and_greed_historical.data.Length == 0)
                        return false;

                    var refMS = stockSerie.LastValue == null ? 0 : (stockSerie.LastValue.DATE - refDate).Ticks / (TimeSpan.TicksPerMillisecond);

                    foreach (var value in data.fear_and_greed_historical.data.Where(d => d.x > refMS).Select(d => new StockDailyValue(d.y, d.y, d.y, d.y, 0, refDate.AddSeconds(d.x / 1000).Date)))
                    {
                        if (!stockSerie.ContainsKey(value.DATE))
                            stockSerie.Add(value.DATE, value);
                    }

                    string fileName = stockSerie.Symbol + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv";
                    string archiveFileName = DataFolder + ARCHIVE_FOLDER + "\\" + fileName;
                    var lastDate = stockSerie.Keys.Last().Date == DateTime.Today ? stockSerie.Keys.Last().AddDays(-1): stockSerie.Keys.Last();
                    stockSerie.SaveToCSVFromDateToDate(archiveFileName, stockSerie.Keys.First(), lastDate);

                    return true;
                }
                catch (Exception ex)
                {
                    StockLog.Write(ex);
                }
            }
            return false;
        }

        static private HttpClient httpClient = null;
        private static string HttpGet(string url)
        {
            try
            {
                var cacheFilePath = Path.Combine(DataFolder + WEB_CACHE_FOLDER, "fearGreed.json");
                if (File.Exists(cacheFilePath) && File.GetCreationTime(cacheFilePath).Date == DateTime.Today)
                {
                    return File.ReadAllText(cacheFilePath);
                }

                if (httpClient == null)
                {
                    var handler = new HttpClientHandler();
                    handler.AutomaticDecompression = ~DecompressionMethods.None;
                    httpClient = new HttpClient(handler);
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://production.dataviz.cnn.io/index/fearandgreed/graphdata"))
                    {
                        request.Headers.TryAddWithoutValidation("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                        request.Headers.TryAddWithoutValidation("accept-language", "fr,fr-FR;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
                        request.Headers.TryAddWithoutValidation("cache-control", "max-age=0");
                        request.Headers.TryAddWithoutValidation("if-none-match", "W/9218406070119492977");
                        request.Headers.TryAddWithoutValidation("priority", "u=0, i");
                        request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Microsoft Edge\";v=\"125\", \"Chromium\";v=\"125\", \"Not.A/Brand\";v=\"24\"");
                        request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                        request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                        request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                        request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                        request.Headers.TryAddWithoutValidation("sec-fetch-site", "none");
                        request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");
                        request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                        request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0");

                        var task = httpClient.SendAsync(request);
                        var response = task.Result;

                        if (response.IsSuccessStatusCode)
                        {
                            var res = response.Content.ReadAsStringAsync().Result;
                            File.WriteAllText(cacheFilePath, res);
                            return res;
                        }
                        else
                        {
                            StockLog.Write("StatusCode: " + response.StatusCode + Environment.NewLine + response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            return null;

        }

        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            return DialogResult.OK;
        }

        public override string DisplayName => "Cnn";

        public override void OpenInDataProvider(StockSerie stockSerie)
        {
            Process.Start("https://edition.cnn.com/markets/fear-and-greed");
        }
    }
}
