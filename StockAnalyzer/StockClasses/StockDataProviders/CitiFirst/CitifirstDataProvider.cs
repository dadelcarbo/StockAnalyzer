using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders.CitiFirst
{
    public class CitifirstDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private readonly string ARCHIVE_FOLDER = INTRADAY_ARCHIVE_SUBFOLDER + @"\Citifirst";
        static private readonly string INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\Citifirst";
        static private readonly string CONFIG_FILE = "CitifirstDownload.cfg";
        static private readonly string CONFIG_FILE_USER = "CitifirstDownload.user.cfg";

        #region HttpClient

        static private HttpClient httpClient = null;
        static public HttpResponseMessage HttpGet(string isin)
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
                // Retrieves 15 minutes data.
                var url = $"https://fr.citifirst.com/citi/v1/theq/api/Charts/fr-FR/GetProduct?period=Month&timeZone=CET&symbol={isin}&pointInterval=900&timeFrom=28800&timeTo=79200&series=Bid";
                //  var url = $"https://fr.citifirst.com/citi/v1/theq/api/Charts/fr-FR/GetProduct?period=Week&timeZone=CET&symbol={isin}&pointInterval=60&timeFrom=28800&timeTo=79200&series=Bid";
                return httpClient.GetAsync(url).Result;
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
            needDownload = download;
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
                DownloadIntradayData(stockSerie);
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
                       || DateTime.Today.DayOfWeek == DayOfWeek.Sunday && lastWriteTime.Date >= DateTime.Today.AddDays(-1)
                       || DateTime.Today.DayOfWeek == DayOfWeek.Saturday && lastWriteTime.Date >= DateTime.Today)
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
                            var response = HttpGet(stockSerie.ISIN);
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
                            var stockSerie = new StockSerie(row[0], row[1],
                                StockSerie.Groups.TURBO,
                                StockDataProvider.Citifirst, BarDuration.M_15);
                            stockSerie.ISIN = row[2];
                            stockSerie.Url = row[3];

                            var dailySerie = stockDictionary.Values.FirstOrDefault(s => !string.IsNullOrEmpty(s.ISIN) && s.Symbol == stockSerie.Symbol);
                            if (dailySerie != null)
                            {
                                stockSerie.ISIN = dailySerie.ISIN;
                            }
                            stockDictionary.Add(row[0], stockSerie);
                            if (download && needDownload)
                            {
                                needDownload = DownloadDailyData(stockSerie);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Investing Intraday Entry: " + row[0] + " already in stockDictionary");
                        }
                    }
                }
            }
        }

        static readonly DateTime refDate = new DateTime(1970, 01, 01);
        private static bool ParseIntradayData(StockSerie stockSerie, string fileName)
        {
            var res = false;
            try
            {
                using (var sr = new StreamReader(fileName))
                {
                    var citifirstJson = CitiFirstSeries.FromJson(sr.ReadToEnd());
                    if (citifirstJson?.Bid == null || citifirstJson.Bid.Length == 0)
                        return false;
                    StockDailyValue previousValue = null;
                    foreach (var data in citifirstJson.Bid)
                    {
                        var openDate = refDate.AddSeconds(data.x / 1000);
                        var value = data.y;
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

        public override string DisplayName => "Citifirst";

        public override void OpenInDataProvider(StockSerie stockSerie)
        {
            if (stockSerie.Url != null)
            {
                Process.Start(stockSerie.Url);
            }
        }
    }
}
