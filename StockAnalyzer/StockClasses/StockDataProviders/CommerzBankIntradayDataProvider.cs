using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
   public class CommerzBankIntradayDataProvider : StockDataProviderBase, IConfigDialog
   {
      static private string ARCHIVE_FOLDER = INTRADAY_ARCHIVE_SUBFOLDER + @"\CommerzBankIntraday";
      static private string INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\CommerzBankIntraday";
      static private string CONFIG_FILE = @"\CommerzBankIntradayDownload.cfg";
      static private string CONFIG_FILE_USER = @"\CommerzBankIntradayDownload.user.cfg";

      public string UserConfigFileName { get { return CONFIG_FILE_USER; } }

      public override bool LoadIntradayDurationArchiveData(string rootFolder, StockSerie serie, StockBarDuration duration)
      {
         StockLog.Write("LoadIntradayDurationArchiveData Name:" + serie.StockName + " duration:" + duration);
         string durationFileName = rootFolder + ARCHIVE_FOLDER + "\\" + duration + "\\" + serie.ShortName.Replace(':', '_') + "_" + serie.StockName + "_" + serie.StockGroup.ToString() + ".txt";
         if (File.Exists(durationFileName))
         {
            var values = serie.GetValues(duration);
            if (values == null)
               StockLog.Write("LoadIntradayDurationArchiveData Cache File Found, current size is: 0");
            else  StockLog.Write("LoadIntradayDurationArchiveData Cache File Found, current size is: " + values.Count);
            serie.ReadFromCSVFile(durationFileName, duration);

            
            StockLog.Write("LoadIntradayDurationArchiveData New serie size is: " + serie.GetValues(duration).Count);
            if (serie.GetValues(duration).Count > 0)
            {
               StockLog.Write("LoadIntradayDurationArchiveData First bar: " +
                              serie.GetValues(duration).First().ToString());
               StockLog.Write("LoadIntradayDurationArchiveData Last bar: " + serie.GetValues(duration).Last().ToString());
            }
            else
            {
               return false;
            }
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
         foreach (StockBarDuration duration in cacheDurations)
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

         // Parse CommerzBankDownload.cfg file
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

               stockSerie.SaveToCSVFromDateToDate(archiveFileName, firstArchiveDate, lastDate.AddDays(-5).Date);

               // Archive other time frames
               string durationFileName;
               StockBarDuration previousDuration = stockSerie.BarDuration;
               foreach (StockBarDuration duration in cacheDurations)
               {
                  durationFileName = rootFolder + ARCHIVE_FOLDER + "\\" + duration + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

                  if (File.Exists(durationFileName) &&
                      File.GetLastWriteTime(durationFileName).Date == DateTime.Today.Date) break; // Only cache once a day.
                  stockSerie.BarDuration = duration;
                  stockSerie.SaveToCSVFromDateToDate(durationFileName, stockSerie.Keys.First(), lastDate.AddDays(-1).Date);
               }

               // Set back to previous duration.
                if (previousDuration != stockSerie.BarDuration)
                {
                    stockSerie.BarDuration = previousDuration;
                }
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
         string fileName = rootFolder + INTRADAY_FOLDER + "\\" + stockSerie.ShortName + "_" + stockSerie.StockName + "_" +
                           stockSerie.StockGroup.ToString() + ".txt";
         if (File.Exists(fileName))
         {
            DateTime fileDate = File.GetLastWriteTime(fileName);
            if (fileDate.Date == DateTime.Today)
               return false;
         }
         this.DownloadIntradayData(rootFolder, stockSerie);
         return true;
      }
      public override bool DownloadIntradayData(string rootFolder, StockSerie stockSerie)
      {
         if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
         {
            NotifyProgress("Downloading intraday for " + stockSerie.StockName);

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

                string isin = stockSerie.ISIN;
                string market = "CBFR";

                url = "http://www5.warrants.commerzbank.com/services/RetailMobile.svc/v1_0_3/charthistory?isin={isin}&mkt={market}&property=Bid&period=5&dayshistory=10&includebarprice=5";
                url = url.Replace("{isin}", isin);
                url = url.Replace("{market}", market);
                try
                {
                    wc.DownloadFile(url, fileName);
                    stockSerie.IsInitialised = false;
                }
                catch (Exception)
                {
                    return false;
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
                  if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                  {
                     string[] row = line.Split(',');
                     string stockName = row[2];
                     StockSerie stockSerie = new StockSerie(stockName, stockName, row[0], StockSerie.Groups.TURBO, StockDataProvider.CommerzBankIntraday);

                     if (!stockDictionary.ContainsKey(stockName))
                     {
                        stockDictionary.Add(stockName, stockSerie);
                     }
                     else
                     {
                        StockLog.Write("CommerzBank Entry: " + stockName + " already in stockDictionary");
                     }
                     if (download && this.needDownload)
                     {
                        this.needDownload = this.DownloadDailyData(rootFolder, stockSerie);
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
               while (!sr.EndOfStream)
               {
                  string line = sr.ReadLine();
                  string[] row = line.Split(',');
                  if (row.Length <2) continue;

                  DateTime openDate;
                  if (!DateTime.TryParse(row[0], out openDate)) continue;
                  float value = float.Parse(row[1]);

                  if (!stockSerie.ContainsKey(openDate))
                  {
                     StockDailyValue dailyValue = new StockDailyValue(stockSerie.StockName,
                            value,
                            value,
                            value,
                            value,
                            0,
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

      public DialogResult ShowDialog(StockDictionary stockDico)
      {
         //CommerzBankIntradayDataProviderConfigDlg configDlg = new CommerzBankIntradayDataProviderConfigDlg(stockDico);
         //return configDlg.ShowDialog();
         throw new NotImplementedException();
      }

      public string DisplayName
      {
         get { return "CommerzBank Intraday"; }
      }
   }
}
