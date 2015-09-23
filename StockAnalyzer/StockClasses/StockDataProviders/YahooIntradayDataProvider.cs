using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class YahooIntradayDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private string ARCHIVE_FOLDER = INTRADAY_ARCHIVE_SUBFOLDER + @"\YahooIntraday";
        static private string INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\YahooIntraday";
        static private string CONFIG_FILE = @"\YahooIntradayDownload.cfg";
        static private string CONFIG_FILE_USER = @"\YahooIntradayDownload.user.cfg";

        public string UserConfigFileName { get {return CONFIG_FILE_USER;}}

        public override bool LoadIntradayDurationArchiveData(string rootFolder, StockSerie serie, StockSerie.StockBarDuration duration)
        {
           string durationFileName = rootFolder + ARCHIVE_FOLDER + "\\" + duration + "\\" + serie.ShortName.Replace(':', '_') + "_" + serie.StockName + "_" + serie.StockGroup.ToString() + ".txt";
           if (File.Exists(durationFileName))
           {
              serie.ReadFromCSVFile(durationFileName, duration);
           }
           else
           {
              return false;
           }
           return true;
        }

        public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
        {
            // Parse yahoo.cfg file// Create data folder if not existing
            if (!Directory.Exists(rootFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + ARCHIVE_FOLDER);
            }
            foreach (StockSerie.StockBarDuration duration in cacheDurations)
            {
               string durationFileName = rootFolder + ARCHIVE_FOLDER + "\\" + duration;
               if (!Directory.Exists(durationFileName))
               {
                  Directory.CreateDirectory(durationFileName);
               }
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
            string archiveFileName = rootFolder + ARCHIVE_FOLDER + "\\" + stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
            if (File.Exists(archiveFileName))
            {
                stockSerie.ReadFromCSVFile(archiveFileName);
            }

            string fileName = rootFolder + INTRADAY_FOLDER + "\\" + stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

            if (File.Exists(fileName))
            {
                if (ParseIntradayData(stockSerie, fileName))
                {
                   stockSerie.Values.Last().IsComplete = false;
                   var lastDate = stockSerie.Keys.Last();

                   DateTime firstArchiveDate = lastDate.AddMonths(-2).AddDays(-lastDate.Day + 1).Date;

                   stockSerie.SaveToCSVFromDateToDate(archiveFileName, firstArchiveDate,
                      lastDate.AddDays(-5).Date);

                   // Archive other time frames
                   string durationFileName;
                   StockSerie.StockBarDuration previousDuration = stockSerie.BarDuration;
                   foreach (StockSerie.StockBarDuration duration in cacheDurations)
                   {
                      durationFileName = rootFolder + ARCHIVE_FOLDER + "\\" + duration + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

                      if (File.Exists(durationFileName) &&
                          File.GetLastWriteTime(durationFileName).Date == DateTime.Today.Date) break; // Only cache once a day.
                      stockSerie.BarDuration = duration;
                      stockSerie.SaveToCSVFromDateToDate(durationFileName, stockSerie.Keys.First(), lastDate.AddDays(30).Date);
                   }

                   // Set back to previous duration.
                   stockSerie.BarDuration = previousDuration;
                }
                else { return false; }
            }
            return true;
        }
        public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
        {
            return true;
        }
        public override bool DownloadIntradayData(string rootFolder, StockSerie stockSerie)
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NotifyProgress("Downloading intraday for" + stockSerie.StockGroup.ToString());
                
                string fileName = rootFolder + INTRADAY_FOLDER + "\\" + stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
                if (File.Exists(fileName))
                {
                    if (File.GetLastWriteTime(fileName) > DateTime.Now.AddMinutes(-2))
                        return false;
                }
                using (WebClient wc = new WebClient())
                {
                    try
                    {
                        wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
                        string url = "http://chartapi.finance.yahoo.com/instrument/1.0/" + stockSerie.ShortName +
                                     "/chartdata;type=quote;range=50d/csv";
                        wc.DownloadFile(url, fileName);
                        stockSerie.IsInitialised = false;
                    }
                    catch
                    {
                    }
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
                        if (!line.StartsWith("#"))
                        {
                            string[] row = line.Split(',');
                            string stockName = row[1];
                            StockSerie stockSerie = new StockSerie(stockName, row[0], (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[2]), StockDataProvider.YahooIntraday);

                            if (!stockDictionary.ContainsKey(stockName))
                            {
                                stockDictionary.Add(stockName, stockSerie);
                            }
                            else
                            {
                                StockLog.Write("Yahoo Entry: " + stockName + " already in stockDictionary");
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

        private static bool ParseIntradayData(StockSerie stockSerie, string fileName)
        {
            bool res = false;
            try
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    string line;
     
                    while ( (line = sr.ReadLine())!=null)
                    {
                        if (line.StartsWith("timezone")) break;
                    }
                    if (sr.EndOfStream) return false;
                    string[] row = line.Split(new char[] { ':' });
                    string timeZone = row[1];
                    double gmtoffset = timeZone == "CEST" ? -32400 : 0;

                    while ( (line = sr.ReadLine())!=null)
                    {
                        if (line.StartsWith("gmtoffset")) break;
                    }
                    if (sr.EndOfStream) return false;


                    row = line.Split(new char[] { ':' });
                    gmtoffset += double.Parse(row[1]);

                    
                    DateTime now = DateTime.Now;
                    DateTime utcNow = now.ToUniversalTime();
                    gmtoffset -= (now - utcNow).TotalSeconds;

                    while (!(line = sr.ReadLine()).StartsWith("range"));

                    // First Range, read second offest for date calculation.
                    row = line.Split(new char[] { ',', ':' });
                    DateTime startDate = new DateTime(int.Parse(row[1].Substring(0, 4)), int.Parse(row[1].Substring(4, 2)), int.Parse(row[1].Substring(6, 2)));
                    
                    //  startDate = startDate.AddHours(9);
                    long startTimeStamp = long.Parse(row[2]);

                    while (!(line = sr.ReadLine()).StartsWith("volume")) ;

                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();

                        row = line.Split(new char[] { ',' });

                        DateTime openDate = startDate;
                        openDate = startDate.AddSeconds(long.Parse(row[0]) - startTimeStamp);
                        openDate = startDate.AddSeconds(long.Parse(row[0]) - startTimeStamp - gmtoffset);
                        if (!stockSerie.ContainsKey(openDate))
                        {
                            StockDailyValue dailyValue = new StockDailyValue(stockSerie.StockName,
                                   float.Parse(row[4], usCulture),
                                   float.Parse(row[2], usCulture),
                                   float.Parse(row[3], usCulture),
                                   float.Parse(row[1], usCulture),
                                   long.Parse(row[5]),
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
                StockLog.Write("Unable to parse intraday data for " + stockSerie.StockName);
                StockLog.Write(e);
            }
            return res;
        }

        public bool DownloadFileFromYahoo(string destFolder, string fileName, DateTime startDate, DateTime endDate, string stockName)
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

            int nbTries = 2;
            while (nbTries > 0)
            {
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
                        wc.DownloadFile(url, destFolder + "\\" + fileName);
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
            YahooIntradayDataProviderConfigDlg configDlg = new YahooIntradayDataProviderConfigDlg(stockDico);
            return configDlg.ShowDialog();
        }

        public string DisplayName
        {
            get { return "Yahoo! Intraday"; }
        }
    }
}
