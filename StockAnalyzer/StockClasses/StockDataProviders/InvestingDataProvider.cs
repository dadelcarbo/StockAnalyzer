using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class InvestingDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private string FOLDER = @"\data\daily\Investing";
        static private string ARCHIVE_FOLDER = @"\data\archive\daily\Investing";

        static private string CONFIG_FILE = @"\InvestingDownload.cfg";
        static private string CONFIG_FILE_USER = @"\InvestingDownload.user.cfg";
        public string UserConfigFileName { get { return CONFIG_FILE_USER; } }

        public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
        {
            // Parse Investing.cfg file// Create data folder if not existing
            if (!Directory.Exists(rootFolder + FOLDER))
            {
                Directory.CreateDirectory(rootFolder + FOLDER);
            }
            if (!Directory.Exists(rootFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + ARCHIVE_FOLDER);
            }

            // Parse InvestingDownload.cfg file
            InitFromFile(rootFolder, stockDictionary, download, rootFolder + CONFIG_FILE);
            InitFromFile(rootFolder, stockDictionary, download, rootFolder + CONFIG_FILE_USER);
        }
        private void InitFromFile(string rootFolder, StockDictionary stockDictionary, bool download, string fileName)
        {
            string line;
            if (File.Exists(fileName))
            {
                using (var sr = new StreamReader(fileName, true))
                {
                    sr.ReadLine(); // Skip first line
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                        var row = line.Split(',');
                        if (!stockDictionary.ContainsKey(row[2]))
                        {
                            var stockSerie = new StockSerie(row[2], row[1],
                                (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[3]),
                                StockDataProvider.Investing);
                            stockSerie.Ticker = long.Parse(row[0]);

                            stockDictionary.Add(row[2], stockSerie);
                            if (download && this.needDownload)
                            {
                                this.DownloadDailyData(rootFolder, stockSerie);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Investing Daily Entry: " + row[2] + " already in stockDictionary");
                        }
                    }
                }
            }
        }

        public override bool SupportsIntradayDownload => false;

        public override bool LoadData(string rootFolder, StockSerie stockSerie)
        {
            var archiveFileName = rootFolder + ARCHIVE_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
            if (File.Exists(archiveFileName))
            {
                stockSerie.ReadFromCSVFile(archiveFileName);
            }

            var fileName = rootFolder + FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

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
            }
            return true;
        }

        static DateTime refDate = new DateTime(1970, 01, 01) + (DateTime.Now - DateTime.UtcNow);
        public string FormatURL(long ticker, DateTime startDate, DateTime endDate)
        {
            var interval = "D";
            var from = (long)((startDate - refDate).TotalSeconds);
            var to = (long)((endDate - refDate).TotalSeconds);

            return $"https://tvc6.forexpros.com/594533c045d911db442ef05f2db3f33d/1536084049/1/1/8/history?symbol={ticker}&resolution={interval}&from={from}&to={to}";
        }

        public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NotifyProgress("Downloading daily data for " + stockSerie.StockName);

                var fileName = rootFolder + FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

                if (File.Exists(fileName))
                {
                    if (File.GetLastWriteTime(fileName) > DateTime.Now.AddMinutes(-2))
                        return false;
                }
                using (var wc = new WebClient())
                {
                    wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

                    var url = string.Empty;
                    if (stockSerie.Initialise() && stockSerie.Count > 0)
                    {
                        var startDate = stockSerie.ValueArray[stockSerie.LastCompleteIndex].DATE.Date.AddDays(-7);
                        if (startDate > DateTime.Today) return true;

                        url = FormatURL(stockSerie.Ticker, startDate, DateTime.Today);
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
                            wc.DownloadFile(url, fileName);
                            stockSerie.IsInitialised = false;
                            return true;
                        }
                        catch (Exception e)
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
                            var dailyValue = new StockDailyValue(stockSerie.StockName,
                                   barchartJson.O[i],
                                   barchartJson.H[i],
                                   barchartJson.L[i],
                                   barchartJson.C[i],
                                   long.Parse(barchartJson.V[i]),
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
            //var configDlg = new InvestingIntradayDataProviderConfigDlg(stockDico);
            //return configDlg.ShowDialog();
            throw new NotImplementedException();
        }

        public string DisplayName => "Investing";
    }
}
