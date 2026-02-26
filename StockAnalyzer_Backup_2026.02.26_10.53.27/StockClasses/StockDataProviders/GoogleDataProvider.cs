using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class GoogleDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private string FOLDER = @"\data\daily\Google";
        static private string ARCHIVE_FOLDER = @"\data\archive\daily\Google";

        static private string CONFIG_FILE = @"\GoogleDownload.cfg";
        static private string CONFIG_FILE_USER = @"\GoogleDownload.user.cfg";
        public string UserConfigFileName { get { return CONFIG_FILE_USER; } }


        public override bool SupportsIntradayDownload
        {
            get { return false; }
        }
        public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
        {
            return; // Google API not running anymore.
            // Parse Google.cfg file// Create data folder if not existing
            if (!Directory.Exists(rootFolder + FOLDER))
            {
                Directory.CreateDirectory(rootFolder + FOLDER);
            }
            if (!Directory.Exists(rootFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + ARCHIVE_FOLDER);
            }

            // Parse GoogleDownload.cfg file
            InitFromFile(rootFolder, stockDictionary, download, rootFolder + CONFIG_FILE);
            InitFromFile(rootFolder, stockDictionary, download, rootFolder + CONFIG_FILE_USER);
        }

        private void InitFromFile(string rootFolder, StockDictionary stockDictionary, bool download, string fileName)
        {
            string line;
            if (File.Exists(fileName))
            {
                using (StreamReader sr = new StreamReader(fileName, true))
                {
                    sr.ReadLine(); // Skip first line
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                        {
                            string[] row = line.Split(',');

                            string shortName = row[0].Contains(':') ? row[0].Split(':')[1] : row[0];
                            StockSerie stockSerie = new StockSerie(row[1], shortName, (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[2]), StockDataProvider.Google);

                            stockSerie.Exchange = row[0].Contains(':') ? row[0].Split(':')[0]:null;

                            if (!stockDictionary.ContainsKey(row[1]))
                            {
                                stockDictionary.Add(row[1], stockSerie);
                            }
                            else
                            {
                                StockLog.Write("Google Entry: " + row[1] + " already in stockDictionary");
                            }
                            if (download && this.needDownload)
                            {
                                this.DownloadDailyData(rootFolder, stockSerie);
                            }
                        }
                    }
                }
            }
        }
        public override bool LoadData(string rootFolder, StockSerie stockSerie)
        {
            // Read archive first
            bool res = false;
            string fileName = stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + "_*.csv";
            string[] files = System.IO.Directory.GetFiles(rootFolder + ARCHIVE_FOLDER, fileName);
            foreach (string archiveFileName in files)
            {
                res |= ParseCSVFile(stockSerie, archiveFileName);
            }

            fileName = rootFolder + FOLDER + "\\" + stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv";
            return ParseCSVFile(stockSerie, fileName) || res;
        }

        public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                string shortName = stockSerie.ShortName;
                if (stockSerie.BelongsToGroup(StockSerie.Groups.FTSE100))
                {
                    shortName = "LON:" + shortName;
                }
                else if (stockSerie.BelongsToGroup(StockSerie.Groups.DAX30))
                {
                    shortName = "ETR:" + shortName;
                }
                else if (stockSerie.Exchange != null)
                {
                    shortName = stockSerie.Exchange + ":" + shortName;
                }

                bool isUpTodate = false;
                stockSerie.Initialise();
                if (stockSerie.Count > 0)
                {
                    // This serie already exist, download just the missing data.
                    DateTime lastDate = stockSerie.Keys.Last();

                    isUpTodate = (lastDate >= DateTime.Today) ||
                        (lastDate.DayOfWeek == DayOfWeek.Friday && (DateTime.Now - lastDate).Days <= 2 && (DateTime.Today.DayOfWeek == DayOfWeek.Monday && DateTime.UtcNow.Hour < 20)) ||
                        (lastDate >= DateTime.Today.AddDays(-1) && DateTime.UtcNow.Hour < 20);

                    NotifyProgress("Downloading " + stockSerie.StockGroup.ToString() + " - " + stockSerie.StockName);
                    if (!isUpTodate)
                    {
                        for (int year = lastDate.Year; year < DateTime.Today.Year; year++)
                        {
                            // Happy new year !!! it's time to archive old data...
                            if (
                               !File.Exists(rootFolder + ARCHIVE_FOLDER + "\\" + stockSerie.ShortName + "_" +
                                            stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + "_" +
                                            year.ToString() + ".csv"))
                            {
                                this.DownloadFileFromProvider(rootFolder + ARCHIVE_FOLDER,
                                   stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() +
                                   "_" + year.ToString() + ".csv", new DateTime(year, 1, 1), new DateTime(year, 12, 31),
                                   stockSerie.ShortName);
                            }
                        }
                        DateTime startDate = new DateTime(DateTime.Today.Year, 01, 01);
                        string fileName = stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv";
                        this.DownloadFileFromProvider(rootFolder + FOLDER, fileName, startDate, DateTime.Today, shortName);

                        if (stockSerie.StockName == "ADIDAS") // Check if something new has been downloaded using ANGLO AMERICAN as the reference for all downloads
                        {
                            this.ParseCSVFile(stockSerie, rootFolder + FOLDER + "\\" + fileName);
                            if (lastDate == stockSerie.Keys.Last())
                            {
                                this.needDownload = false;
                            }
                        }
                    }
                    else
                    {
                        if (stockSerie.StockName == "ADIDAS")
                        {
                            this.needDownload = false;
                        }
                    }
                    stockSerie.IsInitialised = isUpTodate;
                }
                else
                {
                    NotifyProgress("Creating archive for " + stockSerie.StockName + " - " + stockSerie.StockGroup.ToString());
                    DateTime lastDate = new DateTime(DateTime.Today.Year, 01, 01);
                    for (int i = lastDate.Year - 1; i > ARCHIVE_START_YEAR; i--)
                    {
                        if (!this.DownloadFileFromProvider(rootFolder + ARCHIVE_FOLDER, stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + "_" + i.ToString() + ".csv", new DateTime(i, 1, 1), new DateTime(i, 12, 31), shortName))
                        {
                            break;
                        }
                    }
                    this.DownloadFileFromProvider(rootFolder + FOLDER, stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv", lastDate, DateTime.Today, shortName);
                }
            }
            return true;
        }

        public bool DownloadFileFromProvider(string destFolder, string fileName, DateTime startDate, DateTime endDate, string stockName)
        {
            // https://finance.google.com/finance/historical?q=AMPE&startdate=Jan+1%2C+2017&enddate=Dec+11%2C+2017&histperiod=daily&output=csv

            string url = @"https://finance.google.com/finance/historical?q=$NAME&startdate=$START_MONTH+$START_DAY%2C+$START_YEAR&enddate=$END_MONTH+$END_DAY%2C+$END_YEAR&histperiod=daily&output=csv";

            // Build URL
            url = url.Replace("$NAME", stockName);
            url = url.Replace("$START_DAY", startDate.Day.ToString());
            url = url.Replace("$START_MONTH", startDate.ToString("MMM", usCulture));
            url = url.Replace("$START_YEAR", startDate.Year.ToString());
            url = url.Replace("$END_DAY", endDate.Day.ToString());
            url = url.Replace("$END_MONTH", endDate.ToString("MMM", usCulture));
            url = url.Replace("$END_YEAR", endDate.Year.ToString());

            int nbTries = 2;
            while (nbTries > 0)
            {
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

                        string fileContent = wc.DownloadString(url);

                        if (fileContent.Length <= 40 || fileContent.StartsWith("<!DOCTYPE", StringComparison.CurrentCultureIgnoreCase))
                        {
                            return false;
                        }
                        else
                        {
                            // Save content to file
                            using (StreamWriter writer = new StreamWriter(destFolder + "\\" + fileName))
                            {
                                writer.Write(fileContent);
                                //StockLog.Write("Download succeeded: " + destFolder + "\\" + fileName);
                            }
                        }

                        StockLog.Write("Download succeeded: " + stockName);
                        return true;
                    }
                }
                catch (SystemException e)
                {
                    StockLog.Write(e);
                    nbTries--;
                }
            }
            if (nbTries <= 0)
            {
                return false;
            }
            return true;
        }


        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            GoogleDataProviderConfigDlg configDlg = new GoogleDataProviderConfigDlg(stockDico);
            return configDlg.ShowDialog();
        }

        public string DisplayName
        {
            get { return "Google"; }
        }
    }
}
