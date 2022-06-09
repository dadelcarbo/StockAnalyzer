using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class InvestingIntradayDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private readonly string ARCHIVE_FOLDER = INTRADAY_ARCHIVE_SUBFOLDER + @"\InvestingIntraday";
        static private readonly string INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\InvestingIntraday";
        static private readonly string CONFIG_FILE = "InvestingIntradayDownload.cfg";
        static private readonly string CONFIG_FILE_USER = "InvestingIntradayDownload.user.cfg";

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
                            HttpClient client = new HttpClient();
                            var response = client.GetAsync(url).Result;
                            if (response.IsSuccessStatusCode)
                            {
                                var content = response.Content.ReadAsStringAsync().Result;
                                if (content.StartsWith("{"))
                                {
                                    File.WriteAllText(fileName, content);
                                    stockSerie.IsInitialised = false;
                                    return true;
                                }
                                return false;
                            }
                            nbTries--;
                        }
                        catch (Exception)
                        {
                            nbTries--;
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
                        if (!stockDictionary.ContainsKey(row[2]))
                        {
                            var stockSerie = new StockSerie(row[2], row[1],
                                (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[3]),
                                StockDataProvider.InvestingIntraday, BarDuration.M_5);
                            stockSerie.Ticker = long.Parse(row[0]);

                            var dailySerie = stockDictionary.Values.FirstOrDefault(s => !string.IsNullOrEmpty(s.ISIN) && s.ShortName == stockSerie.ShortName);
                            if (dailySerie != null)
                            {
                                stockSerie.ISIN = dailySerie.ISIN;
                            }
                            stockDictionary.Add(row[2], stockSerie);
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

        static DateTime refDate = new DateTime(1970, 01, 01) + (DateTime.Now - DateTime.UtcNow);
        private static bool ParseIntradayData(StockSerie stockSerie, string fileName)
        {
            var res = false;
            try
            {
                using (var sr = new StreamReader(fileName))
                {
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
            var configDlg = new InvestingIntradayDataProviderConfigDlg(stockDico, CONFIG_FILE_USER);
            return configDlg.ShowDialog();
        }

        public string DisplayName => "Investing Intraday";
    }
}
