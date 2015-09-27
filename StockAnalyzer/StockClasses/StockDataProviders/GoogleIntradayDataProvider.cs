using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
   public class GoogleIntradayDataProvider : StockDataProviderBase, IConfigDialog
   {
      static private string ARCHIVE_FOLDER = INTRADAY_ARCHIVE_SUBFOLDER + @"\GoogleIntraday";
      static private string INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\GoogleIntraday";
      static private string CONFIG_FILE = @"\GoogleIntradayDownload.cfg";
      static private string CONFIG_FILE_USER = @"\GoogleIntradayDownload.user.cfg";

      public string UserConfigFileName { get { return CONFIG_FILE_USER; } }

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
         // Create data folder if not existing
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

         // Parse GoogleDownload.cfg file
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
         string archiveFileName = rootFolder + ARCHIVE_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
         if (File.Exists(archiveFileName))
         {
            stockSerie.ReadFromCSVFile(archiveFileName);
         }

         string fileName = rootFolder + INTRADAY_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

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
                  stockSerie.SaveToCSVFromDateToDate(durationFileName, stockSerie.Keys.First(), lastDate.AddDays(-5).Date);
               }

               // Set back to previous duration.
               stockSerie.BarDuration = previousDuration;
            }
            else
            {
               return false;
            }
         }
         return true;
      }
      public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
      {
         return false;
      }
      public override bool DownloadIntradayData(string rootFolder, StockSerie stockSerie)
      {
         if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
         {
            NotifyProgress("Downloading intraday for" + stockSerie.StockGroup.ToString());

            string fileName = rootFolder + INTRADAY_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

            if (File.Exists(fileName))
            {
               if (File.GetLastWriteTime(fileName) > DateTime.Now.AddMinutes(-2))
                  return false;
            }
            using (WebClient wc = new WebClient())
            {
               wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

               string url;
               if (stockSerie.ShortName.Contains(':'))
               {
                  string name = stockSerie.ShortName.Split(':')[1];
                  string exchange = stockSerie.ShortName.Split(':')[0];
                  url = "https://www.google.com/finance/getprices?q={code}&i={interval}&x={exchange}";
                  url = url.Replace("{code}", name);
                  url = url.Replace("{interval}", "60");
                  url = url.Replace("{exchange}", exchange);
               }
               else
               {
                  url = "http://www.google.com/finance/getprices?q={code}&i={interval}";
                  url = url.Replace("{code}", stockSerie.ShortName);
                  url = url.Replace("{interval}", "60");
               }
               wc.DownloadFile(url, fileName);
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
                  if (!line.StartsWith("#"))
                  {
                     string[] row = line.Split(',');
                     string stockName = row[1];
                     StockSerie stockSerie = new StockSerie(stockName, row[0], (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[2]), StockDataProvider.GoogleIntraday);

                     if (!stockDictionary.ContainsKey(stockName))
                     {
                        stockDictionary.Add(stockName, stockSerie);
                     }
                     else
                     {
                        StockLog.Write("Google Entry: " + stockName + " already in stockDictionary");
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
               string line = sr.ReadLine();

               while (!(line = sr.ReadLine()).StartsWith("INTERVAL")) ;
               int interval;
               interval = int.Parse(line.Split('=')[1]);

               while (!(line = sr.ReadLine()).StartsWith("TIMEZONE_OFFSET")) ;
               int offset;
               offset = int.Parse(line.Split('=')[1]);


               string[] row;
               DateTime startDate = DateTime.Now;
               while (!sr.EndOfStream)
               {
                  line = sr.ReadLine();
                  DateTime openDate = DateTime.Now;
                  row = line.Split(new char[] { ',' });

                  if (line.StartsWith("a"))
                  {
                     // new day detected
                     string startString = row[0].Replace("a", "");
                     double seconds = double.Parse(startString);

                     startDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds).AddMinutes(offset);
                     openDate = startDate;
                  }
                  else if (line.StartsWith("TIMEZONE_OFFSET"))
                  {
                     offset = int.Parse(line.Split('=')[1]);
                     continue;
                  }
                  else
                  {
                     // just a new bar
                     openDate = startDate.AddSeconds(long.Parse(row[0]) * interval);
                  }

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

      public bool DownloadFileFromGoogle(string destFolder, string fileName, DateTime startDate, DateTime endDate, string stockName)
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
         GoogleIntradayDataProviderConfigDlg configDlg = new GoogleIntradayDataProviderConfigDlg(stockDico);
         return configDlg.ShowDialog();
      }

      public string DisplayName
      {
         get { return "Google Intraday"; }
      }
   }
}
