using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using StockAnalyzerSettings.Properties;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class InvestingDataProvider : StockDataProviderBase, IConfigDialog
    {
        private static readonly string FOLDER = @"\daily\Investing";
        private static readonly string ARCHIVE_FOLDER = @"\archive\daily\Investing";

        private static readonly string CONFIG_FILE = "InvestingDownload.cfg";
        private static readonly string CONFIG_FILE_USER = "InvestingDownload.user.cfg";

        public override void InitDictionary(StockDictionary stockDictionary, bool download)
        {
            return;
            // Parse Investing.cfg file// Create data folder if not existing
            if (!Directory.Exists(DataFolder + FOLDER))
            {
                Directory.CreateDirectory(DataFolder + FOLDER);
            }
            if (!Directory.Exists(DataFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ARCHIVE_FOLDER);
            }

            this.needDownload = download;

            // Parse InvestingDownload.cfg file
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
                        StockLog.Write("Investing Daily Entry: " + row[2] + " already in stockDictionary");
                    }
                }
            }
        }

        public override bool SupportsIntradayDownload => false;

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
            return res;
        }

        public string FormatURL(long ticker, DateTime startDate, DateTime endDate)
        {
            var interval = "D";
            var from = (long)((startDate - refDate).TotalSeconds);
            var to = (long)((endDate - refDate).TotalSeconds);
            return $"{Settings.Default.InvestingUrlRoot}/history?symbol={ticker}&resolution={interval}&from={from}&to={to}";
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
                        if (!stockSerie.StockName.StartsWith("CC_"))
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
                    url = FormatURL(stockSerie.Ticker, lastDate.AddMonths(-2), DateTime.Today);
                }
                else
                {
                    url = FormatURL(stockSerie.Ticker, lastDate, DateTime.Today);
                }

                int nbTries = 2;
                while (nbTries > 0)
                {
                    try
                    {
                        var response = InvestingIntradayDataProvider.HttpGetFromInvesting(url);
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
            return false;
        }

        private static bool ParseDailyData(StockSerie stockSerie, string fileName)
        {
            var res = false;
            try
            {
                using var sr = new StreamReader(fileName);
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
            catch (Exception e)
            {
                StockLog.Write("Unable to parse daily data for " + stockSerie.StockName);
                StockLog.Write(e);
            }
            return res;
        }

        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            var configDlg = new InvestingIntradayDataProviderConfigDlg(stockDico, CONFIG_FILE_USER) { StartPosition = FormStartPosition.CenterScreen };
            return configDlg.ShowDialog();
        }

        public override string DisplayName => "Investing";
    }
}
