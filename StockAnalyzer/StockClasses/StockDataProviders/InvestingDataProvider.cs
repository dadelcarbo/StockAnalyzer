using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class InvestingDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private string FOLDER = @"\data\daily\Investing";
        static private string ARCHIVE_FOLDER = @"\data\archive\daily\Investing";

        static private string CONFIG_FILE = @"\InvestingDownload.cfg";
        static private string CONFIG_FILE_USER = @"\InvestingDownload.user.cfg";
        public string UserConfigFileName { get { return CONFIG_FILE_USER; } }

        public override void InitDictionary(StockDictionary stockDictionary, bool download)
        {
            // Parse Investing.cfg file// Create data folder if not existing
            if (!Directory.Exists(RootFolder + FOLDER))
            {
                Directory.CreateDirectory(RootFolder + FOLDER);
            }
            if (!Directory.Exists(RootFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(RootFolder + ARCHIVE_FOLDER);
            }

            this.needDownload = download;

            // Parse InvestingDownload.cfg file
            InitFromFile(stockDictionary, download, RootFolder + CONFIG_FILE);
            InitFromFile(stockDictionary, download, RootFolder + CONFIG_FILE_USER);
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
                            var stockSerie = new StockSerie(row[2], row[1], (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[3]), StockDataProvider.Investing, BarDuration.Daily);
                            stockSerie.Ticker = long.Parse(row[0]);

                            stockDictionary.Add(row[2], stockSerie);
                            if (download && this.needDownload)
                            {
                                this.needDownload = this.DownloadDailyData(stockSerie);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Investing Daily Entry: " + row[2] + " already in stockDictionary");
                        }
                    }
                }
            }
        }

        public override bool SupportsIntradayDownload => false;

        public override bool LoadData(StockSerie stockSerie)
        {
            bool res = false;
            var archiveFileName = RootFolder + ARCHIVE_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
            if (File.Exists(archiveFileName))
            {
                stockSerie.ReadFromCSVFile(archiveFileName);
                res = true;
            }

            var fileName = RootFolder + FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

            if (File.Exists(fileName))
            {
                if (ParseDailyData(stockSerie, fileName))
                {
                    stockSerie.Values.Last().IsComplete = false;
                    var lastDate = stockSerie.Keys.Last();

                    var previousMonth = lastDate.AddMonths(-2);
                    var firstArchiveDate = new DateTime(previousMonth.Year, previousMonth.Month, 1); // Archive two month back

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

        static DateTime refDate = new DateTime(1970, 01, 01) + (DateTime.Now - DateTime.UtcNow);
        public string FormatURL(long ticker, DateTime startDate, DateTime endDate)
        {
            var interval = "D";
            var from = (long)((startDate - refDate).TotalSeconds);
            var to = (long)((endDate - refDate).TotalSeconds);
            return $"http://tvc4.forexpros.com/0f8a29a810801b55700d8d096869fe1f/1567000256/1/1/8/history?symbol={ticker}&resolution={interval}&from={from}&to={to}";
        }

        static bool first = true;
        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NotifyProgress("Downloading daily data for " + stockSerie.StockName);

                var fileName = RootFolder + FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

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
                        if (File.GetLastWriteTime(fileName) > DateTime.Now.AddMinutes(-2))
                            return false;
                    }
                }
                using (var wc = new WebClient())
                {
                    wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

                    var url = string.Empty;
                    if (stockSerie.Initialise() && stockSerie.Count > 0)
                    {
                        var startDate = stockSerie.ValueArray[stockSerie.LastCompleteIndex].DATE.Date.AddDays(-7);
                        if (startDate > DateTime.Today) return true;

                        url = FormatURL(stockSerie.Ticker, startDate, DateTime.Now);
                    }
                    else
                    {
                        url = FormatURL(stockSerie.Ticker, DateTime.Today.AddYears(-5), DateTime.Today);
                    }

                    int nbTries = 3;
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
                                else
                                {
                                    return false;
                                }
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

        private static bool ParseDailyData(StockSerie stockSerie, string fileName)
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

                        var openDate = refDate.AddSeconds(barchartJson.T[i]).Date;
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
                StockLog.Write("Unable to parse daily data for " + stockSerie.StockName);
                StockLog.Write(e);
            }
            return res;
        }

        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            var configDlg = new InvestingIntradayDataProviderConfigDlg(stockDico, this.UserConfigFileName);
            return configDlg.ShowDialog();
        }

        public string DisplayName => "Investing";
    }
}
