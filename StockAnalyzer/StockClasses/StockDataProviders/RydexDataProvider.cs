using System;
using System.IO;
using System.Linq;
using System.Net;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
   public class RydexDataProvider : StockDataProviderBase
   {
      static private string FOLDER = @"\data\daily\Rydex";
      static private string ARCHIVE_FOLDER = @"\data\archive\daily\Rydex";

      public override bool LoadData(string rootFolder, StockSerie stockSerie)
      {
         string fileName = rootFolder + ARCHIVE_FOLDER + @"\" + stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv";
         bool res = ParseRydexData(stockSerie, fileName);
         fileName = rootFolder + FOLDER + @"\" + stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv";
         return ParseRydexData(stockSerie, fileName) || res;
      }

      public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
      {
         return;
         /*
         string line;
         string fileName = rootFolder + "\\RydexDownload.cfg";
         // Parse Rydex.cfg file// Create data folder if not existing
         if (!Directory.Exists(rootFolder + FOLDER))
         {
             Directory.CreateDirectory(rootFolder + FOLDER);
         }
         if (!Directory.Exists(rootFolder + ARCHIVE_FOLDER))
         {
             Directory.CreateDirectory(rootFolder + ARCHIVE_FOLDER);
         }

         // Parse RydexDownload.cfg file
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
                         StockSerie stockSerie = new StockSerie(row[1], row[0], (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[2]), StockDataProvider.Rydex);

                         if (!stockDictionary.ContainsKey(row[1]))
                         {
                             stockDictionary.Add(row[1], stockSerie);
                         }
                         else
                         {
                             StockLog.Write("Rydex Entry: " + row[1] + " already in stockDictionary");
                         }
                         if (download && this.needDownload)
                         {
                             this.DownloadDailyData(rootFolder, stockSerie);
                         }
                     }
                 }
             }
         }
         */
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
                   (lastDate.DayOfWeek == DayOfWeek.Friday && (DateTime.Now - lastDate).Days <= 3 && DateTime.UtcNow.Hour < 23) ||
                   (lastDate >= DateTime.Today.AddDays(-1) && DateTime.UtcNow.Hour < 23);

               NotifyProgress("Downloading " + stockSerie.StockGroup.ToString() + " - " + stockSerie.StockName);
               if (!isUpTodate)
               {
                  NotifyProgress("Downloading " + stockSerie.StockGroup.ToString() + " - " + stockSerie.StockName);
                  for (int year = lastDate.Year; year < DateTime.Today.Year; year++)
                  {
                     // Happy new year !!! it's time to archive old data...
                     if (!File.Exists(rootFolder + ARCHIVE_FOLDER + "\\" + stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + "_" + year.ToString() + ".csv"))
                     {
                        this.DownloadFileFromRydex(rootFolder + ARCHIVE_FOLDER, stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + "_" + year.ToString() + ".csv", new DateTime(year, 1, 1), new DateTime(year, 12, 31), stockSerie.ShortName);
                     }
                  }
                  DateTime startDate = new DateTime(DateTime.Today.Year, 01, 01);
                  string fileName = stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv";
                  this.DownloadFileFromRydex(rootFolder + FOLDER, fileName, startDate, DateTime.Today, stockSerie.ShortName);

                  if (stockSerie.StockName == "URSA") // Check if something new has been downloaded using URSA as the reference for all downloads
                  {
                     this.ParseRydexData(stockSerie, rootFolder + FOLDER + "\\" + fileName);
                     if (lastDate == stockSerie.Keys.Last())
                     {
                        this.needDownload = false;
                     }
                  }
               }
               else
               {
                  if (stockSerie.StockName == "URSA")
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
                  if (!this.DownloadFileFromRydex(rootFolder + ARCHIVE_FOLDER, stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + "_" + i.ToString() + ".csv", new DateTime(i, 1, 1), new DateTime(i, 12, 31), stockSerie.ShortName))
                  {
                     break;
                  }
               }
               this.DownloadFileFromRydex(rootFolder + FOLDER, stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv", lastDate, DateTime.Today, stockSerie.ShortName);
            }
         }
         return true;
      }

      private bool DownloadFileFromRydex(string destFolder, string fileName, DateTime startDate, DateTime endDate, string stockName)
      {
         string url = @"http://www.rydex-sgi.com/products/mutual_funds/info/navs_historicalDownload.rails?productId=-1&t_startDate=$START_MONTH%2F$START_DAY%2F$START_YEAR&t_endDate=$END_MONTH%2F$END_DAY%2F$END_YEAR&rydex_symbol=$NAME";

         // Build URL
         url = url.Replace("$NAME", stockName);
         url = url.Replace("$START_DAY", startDate.Day.ToString());
         url = url.Replace("$START_MONTH", startDate.Month.ToString());
         url = url.Replace("$START_YEAR", startDate.Year.ToString());
         url = url.Replace("$END_DAY", endDate.Day.ToString());
         url = url.Replace("$END_MONTH", endDate.Month.ToString());
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

      private bool ParseRydexData(StockSerie stockSerie, string fileName)
      {
         if (File.Exists(fileName))
         {
            using (StreamReader sr = new StreamReader(fileName))
            {
               StockDailyValue readValue = null;
               string line;

               // Skip 2 first lines
               while (!sr.EndOfStream && !(line = sr.ReadLine()).Contains("Date")) ;

               while (!sr.EndOfStream)
               {
                  readValue = null;
                  line = sr.ReadLine();
                  if (string.IsNullOrEmpty(line))
                  {
                     break;
                  }
                  line = line.Replace("\",\"", ";");
                  line = line.Replace("\"", "");
                  string[] row = line.Split(';');
                  if (row[2] == "N/A" || row[3] == "N/A" || row[1] == "AM") continue;
                  float value = float.Parse(row[3], usCulture) / float.Parse(row[2], usCulture);
                  readValue = new StockDailyValue(
                      stockSerie.StockName,
                      value,
                      value,
                      value,
                      value,
                      0,
                      DateTime.Parse(row[0], usCulture));
                  if (!stockSerie.ContainsKey(readValue.DATE))
                  {
                     stockSerie.Add(readValue.DATE, readValue);
                  }
               }
            }
            return true;
         }
         return false;
      }

      public override bool SupportsIntradayDownload
      {
         get { return false; }
      }
   }
}