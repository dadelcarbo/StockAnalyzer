﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class YahooDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private string INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\Yahoo";
        static private string DAILY_FOLDER = DAILY_SUBFOLDER + @"\Yahoo";
        static private string ARCHIVE_FOLDER = DAILY_ARCHIVE_SUBFOLDER + @"\Yahoo";
        static private string CONFIG_FILE = @"\YahooDownload.cfg";
        static private string CONFIG_FILE_USER = @"\YahooDownload.user.cfg";

        public string UserConfigFileName { get { return CONFIG_FILE_USER; } }

        public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
        {
            return;
            // Parse yahoo.cfg file// Create data folder if not existing
            if (!Directory.Exists(rootFolder + DAILY_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + DAILY_FOLDER);
            }
            if (!Directory.Exists(rootFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + ARCHIVE_FOLDER);
            }
            if (!Directory.Exists(rootFolder + INTRADAY_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + INTRADAY_FOLDER);
            }
            else
            {
                foreach (string file in Directory.GetFiles(rootFolder + INTRADAY_FOLDER))
                {
                    if (File.GetLastWriteTime(file).Date != DateTime.Today)
                    {
                        File.Delete(file);
                    }
                }
            }

            // Parse YahooDownload.cfg file
            this.needDownload = download;
            InitFromFile(rootFolder, stockDictionary, download, rootFolder + CONFIG_FILE);
            InitFromFile(rootFolder, stockDictionary, download, rootFolder + CONFIG_FILE_USER);
        }
        public override bool SupportsIntradayDownload
        {
            get { return true; }
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

            fileName = rootFolder + DAILY_FOLDER + "\\" + stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv";
            res |= ParseCSVFile(stockSerie, fileName);

            ParseIntradayData(stockSerie, rootFolder + INTRADAY_FOLDER, stockSerie.ShortName + ".csv");

            return res;
        }
        public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                bool isUpTodate = false;
                stockSerie.Initialise();
                if (stockSerie.Count > 0)
                {
                    // This serie already exist, download just the missing data.
                    DateTime lastDate = stockSerie.Keys.Last();

                    isUpTodate = (lastDate >= DateTime.Today) ||
                        (lastDate.DayOfWeek == DayOfWeek.Friday && (DateTime.Now - lastDate).Days <= 3 && (DateTime.Today.DayOfWeek == DayOfWeek.Monday && DateTime.UtcNow.Hour < 23)) ||
                        (lastDate >= DateTime.Today.AddDays(-1) && DateTime.UtcNow.Hour < 23);

                    NotifyProgress("Downloading " + stockSerie.StockGroup.ToString() + " - " + stockSerie.StockName);
                    if (!isUpTodate)
                    {
                        NotifyProgress("Downloading " + stockSerie.StockGroup.ToString() + " - " + stockSerie.StockName);
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
                        this.DownloadFileFromProvider(rootFolder + DAILY_FOLDER, fileName, startDate, DateTime.Today, stockSerie.ShortName);

                        if (stockSerie.StockName == "SP500") // Check if something new has been downloaded using SP500 as the reference for all downloads
                        {
                            this.ParseCSVFile(stockSerie, rootFolder + DAILY_FOLDER + "\\" + fileName);
                            if (lastDate == stockSerie.Keys.Last())
                            {
                                this.needDownload = false;
                            }
                        }
                    }
                    else
                    {
                        if (stockSerie.StockName == "SP500")
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
                        if (!this.DownloadFileFromProvider(rootFolder + ARCHIVE_FOLDER, stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + "_" + i.ToString() + ".csv", new DateTime(i, 1, 1), new DateTime(i, 12, 31), stockSerie.ShortName))
                        {
                            break;
                        }
                    }
                    this.DownloadFileFromProvider(rootFolder + DAILY_FOLDER, stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv", lastDate, DateTime.Today, stockSerie.ShortName);
                }
            }
            return true;
        }
        public override bool DownloadIntradayData(string rootFolder, StockSerie stockSerie)
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NotifyProgress("Downloading intraday for " + stockSerie.StockName);

                if (!stockSerie.Initialise())
                {
                    return false;
                }

                if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday
                        || DateTime.Today.DayOfWeek == DayOfWeek.Sunday
                        || stockSerie.Keys.Last() == DateTime.Today)
                {
                    return false;
                }

                string folder = rootFolder + INTRADAY_FOLDER;
                string fileName = stockSerie.ShortName + ".csv";
                if (File.Exists(folder + "\\" + fileName))
                {
                    if (File.GetLastWriteTime(folder + "\\" + fileName) > DateTime.Now.AddMinutes(-5))
                        return false;
                }
                using (WebClient wc = new WebClient())
                {
                    wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    string url = "http://download.finance.yahoo.com/d/quotes.csv?s=" + stockSerie.ShortName + "&f=d1t1oml1v&e=.csv";
                    wc.DownloadFile(url, folder + "\\" + fileName);
                    stockSerie.IsInitialised = false;
                }
            }
            return true;
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
                        if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }
                        string[] row = line.Split(',');
                        StockSerie stockSerie = new StockSerie(row[1], row[0], (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[2]), StockDataProvider.Yahoo);

                        if (!stockDictionary.ContainsKey(row[1]))
                        {
                            stockDictionary.Add(row[1], stockSerie);
                        }
                        else
                        {
                            StockLog.Write("Yahoo Entry: " + row[1] + " already in stockDictionary");
                        }
                        if (download && this.needDownload)
                        {
                            this.DownloadDailyData(rootFolder, stockSerie);
                        }
                    }
                }
            }
        }

        private static void ParseIntradayData(StockSerie stockSerie, string folder, string fileName)
        {
            try
            {
                if (File.Exists(folder + "\\" + fileName))
                {
                    using (StreamReader sr = new StreamReader(folder + "\\" + fileName))
                    {
                        string line = sr.ReadLine();
                        string[] row = line.Replace("\"", "").Replace(" - ", ",").Split(',');
                        if (row.Length == 7)
                        {
                            try
                            {
                                StockDailyValue dailyValue = new StockDailyValue(stockSerie.StockName,
                                    float.Parse(row[2], usCulture),
                                    float.Parse(row[4], usCulture),
                                    float.Parse(row[3], usCulture),
                                    float.Parse(row[5], usCulture),
                                    long.Parse(row[6]),
                                    DateTime.Parse(row[0] + " " + row[1], usCulture));

                                StockDailyValue lastValue = stockSerie.Values.Last();


                                if (lastValue.DATE.Date == dailyValue.DATE.Date)
                                {
                                    if (lastValue.DATE.Hour == 0 && lastValue.DATE.Minute == 0) return;

                                    stockSerie.Remove(lastValue.DATE);
                                }
                                stockSerie.Add(dailyValue.DATE, dailyValue);
                                stockSerie.ClearBarDurationCache();
                            }
                            catch (System.Exception e)
                            {
                                StockLog.Write("Unable to parse intraday data for " + stockSerie.StockName);
                                StockLog.Write(line);
                                StockLog.Write(e);
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                StockLog.Write("Unable to parse intraday data for " + stockSerie.StockName);
                StockLog.Write(e);
            }
        }

        public bool DownloadFileFromProvider(string destFolder, string fileName, DateTime startDate, DateTime endDate, string stockName)
        {
            string url = @"http://ichart.finance.yahoo.com/table.csv?s=$NAME&a=$START_MONTH&b=$START_DAY&c=$START_YEAR&d=$END_MONTH&e=$END_DAY&f=$END_YEAR&g=d&ignore=.csv";

            // Build URL
            url = url.Replace("$NAME", stockName);
            url = url.Replace("$START_DAY", startDate.Day.ToString());
            url = url.Replace("$START_MONTH", (startDate.Month - 1).ToString());
            url = url.Replace("$START_YEAR", startDate.Year.ToString());
            url = url.Replace("$END_DAY", endDate.Day.ToString());
            url = url.Replace("$END_MONTH", (endDate.Month - 1).ToString());
            url = url.Replace("$END_YEAR", endDate.Year.ToString());

            DateTime refDate = new DateTime(1970,1,1);

            url = "https://query1.finance.yahoo.com/v7/finance/download/$NAME?period1=$START_DATE&period2=$END_DATE&interval=1d&events=history&crumb=sb83SE0p84J";
            string startPeriod = (startDate - refDate).TotalSeconds.ToString();
            string endPeriod = (endDate - refDate).TotalSeconds.ToString();
            url = url.Replace("$START_DATE", startPeriod);
            url = url.Replace("$END_DATE", endPeriod);
            url = url.Replace("$NAME", stockName);

            url = "https://query1.finance.yahoo.com/v7/finance/download/AC.PA?period1=1492673834&period2=1495265834&interval=1d&events=history&crumb=sb83SE0p84J";
            int nbTries = 2;
            while (nbTries > 0)
            {
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                       // wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
                        wc.DownloadString("https://fr.finance.yahoo.com/quote/%5EFCHI/history?p=%5EFCHI");
                        wc.DownloadFile(url, destFolder + "\\" + fileName);
                        //StockLog.Write("Download succeeded: " + stockName);
                        return true;
                    }
                }
                catch (SystemException)
                {
                    //StockLog.Write(e);
                    nbTries--;
                }
            }
            if (nbTries <= 0)
            {
                StockLog.Write("Failed downloading from yes: " + stockName);
                return false;
            }
            return true;
        }

        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            YahooDataProviderConfigDlg configDlg = new YahooDataProviderConfigDlg(stockDico);
            return configDlg.ShowDialog();
        }

        public string DisplayName
        {
            get { return "Yahoo!"; }
        }
    }
}
