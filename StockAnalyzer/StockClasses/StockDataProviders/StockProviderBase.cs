using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
   public abstract class StockDataProviderBase : IStockDataProvider
   {
      protected bool needDownload = true;

      static public
                 StockSerie.StockBarDuration[] cacheDurations = new StockSerie.StockBarDuration[]
                  {
                     StockAnalyzer.StockClasses.StockSerie.StockBarDuration.TwoLineBreaks_3D,
                     StockAnalyzer.StockClasses.StockSerie.StockBarDuration.TwoLineBreaks_6D,
                     StockAnalyzer.StockClasses.StockSerie.StockBarDuration.TwoLineBreaks_9D,
                     StockAnalyzer.StockClasses.StockSerie.StockBarDuration.TwoLineBreaks_27D
                  };

      #region CONSTANTS
      static protected string DAILY_SUBFOLDER = @"\data\daily";
      static protected string INTRADAY_SUBFOLDER = @"\data\intraday";
      static protected string OPTIX_SUBFOLDER = DAILY_SUBFOLDER + @"\Optix";
      static protected string WEEKLY_SUBFOLDER = @"\data\weekly";
      static protected string DAILY_ARCHIVE_SUBFOLDER = @"\data\archive\daily";
      static protected string INTRADAY_ARCHIVE_SUBFOLDER = @"\data\archive\intraday";

      static private string ABC_SUBFOLDER = DAILY_SUBFOLDER + @"\ABC";
      static private string YAHOO_SUBFOLDER = DAILY_SUBFOLDER + @"\Yahoo";
      static private string CBOE_SUBFOLDER = DAILY_SUBFOLDER + @"\CBOE";
      static protected CultureInfo frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
      static protected CultureInfo usCulture = CultureInfo.GetCultureInfo("en-US");
      #endregion
      #region IStockDataProvider default implementation

      abstract public bool SupportsIntradayDownload { get; }
      virtual public bool LoadData(string rootFolder, StockSerie stockSerie)
      {
         // Read archive first
         string fileName = stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv";
         string fullFileName = rootFolder + "\\data\\archive\\daily\\" + stockSerie.DataProvider.ToString() + "\\" + fileName;
         bool res = ParseCSVFile(stockSerie, fullFileName);

         fullFileName = rootFolder + "\\data\\daily\\" + stockSerie.DataProvider.ToString() + "\\" + fileName;
         return ParseCSVFile(stockSerie, fullFileName) || res;
      }
      abstract public bool DownloadDailyData(string rootFolder, StockSerie stockSerie);
      virtual public bool DownloadIntradayData(string rootFolder, StockSerie stockSerie)
      {
         return this.SupportsIntradayDownload;
      }
      abstract public void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download);

      #endregion
      #region NOTIFICATION EVENTS
      public event DownloadingStockEventHandler DownloadStarted;
      protected void NotifyProgress(string text)
      {
         if (this.DownloadStarted != null)
         {
            this.DownloadStarted(text);
         }
      }
      #endregion
      #region STATIC HELPERS
      public static bool LoadSerieData(string rootFolder, StockSerie serie)
      {
         IStockDataProvider dataProvider = GetDataProvider(serie.DataProvider);
         if (dataProvider == null)
         {
            return false;
         }
         else
         {
            return dataProvider.LoadData(rootFolder, serie);
         }
      }
      public static bool LoadIntradayDurationArchive(string rootFolder, StockSerie serie, StockSerie.StockBarDuration duration)
      {
         IStockDataProvider dataProvider = GetDataProvider(serie.DataProvider);
         if (dataProvider == null)
         {
            return false;
         }
         else
         {
            return dataProvider.LoadIntradayDurationArchiveData(rootFolder, serie, duration);
         }
      }

      private static IStockDataProvider GetDataProvider(StockDataProvider dataProviderType)
      {
         IStockDataProvider dataProvider = null;
         switch (dataProviderType)
         {
            case StockDataProvider.ABC:
               dataProvider = new ABCDataProvider();
               break;
            case StockDataProvider.Yahoo:
               dataProvider = new YahooDataProvider();
               break;
            case StockDataProvider.YahooIntraday:
               dataProvider = new YahooIntradayDataProvider();
               break;
            case StockDataProvider.Google:
               dataProvider = new GoogleDataProvider();
               break;
            case StockDataProvider.GoogleIntraday:
               dataProvider = new GoogleIntradayDataProvider();
               break;
            case StockDataProvider.CBOE:
               dataProvider = new CBOEDataProvider();
               break;
            case StockDataProvider.Harpex:
               dataProvider = new HarpexDataProvider();
               break;
            case StockDataProvider.COT:
               dataProvider = new COTDataProvider();
               break;
            case StockDataProvider.Portofolio:
               break;
            case StockDataProvider.Generated:
               dataProvider = new GeneratedDataProvider();
               break;
            case StockDataProvider.Breadth:
               dataProvider = new BreadthDataProvider();
               break;
            case StockDataProvider.Rydex:
               dataProvider = new RydexDataProvider();
               break;
            case StockDataProvider.AAII:
               dataProvider = new AAIIDataProvider();
               break;
            case StockDataProvider.Ratio:
               dataProvider = new RatioDataProvider();
               break;
            case StockDataProvider.Test:
               break;
            default:
               break;
         }
         return dataProvider;
      }
      public static bool DownloadSerieData(string rootFolder, StockSerie serie)
      {
         IStockDataProvider dataProvider = GetDataProvider(serie.DataProvider);
         if (dataProvider == null)
         {
            return false;
         }
         else
         {
            StockSerie.StockBarDuration currentBarDuration = serie.BarDuration;
            serie.BarDuration = StockSerie.StockBarDuration.Daily;
            bool res = dataProvider.DownloadDailyData(rootFolder, serie);
            if (dataProvider.SupportsIntradayDownload)
            {
               res |= dataProvider.DownloadIntradayData(rootFolder, serie);
            }
            serie.BarDuration = currentBarDuration;
            return res;
         }
      }
      public static void InitStockDictionary(string rootFolder, StockDictionary stockDictionary, bool download, DownloadingStockEventHandler downloadListener)
      {
         foreach (StockDataProvider dataProviderType in Enum.GetValues(typeof(StockDataProvider)))
         {
            IStockDataProvider dataProvider = GetDataProvider(dataProviderType);
            if (dataProvider != null)
            {
               dataProvider.DownloadStarted += downloadListener;
               dataProvider.InitDictionary(rootFolder, stockDictionary, download);
               dataProvider.DownloadStarted -= downloadListener;
            }
         }
      }

      private static List<IConfigDialog> configDialogs = null;
      public static List<IConfigDialog> GetConfigDialogs()
      {
         if (configDialogs == null)
         {
            configDialogs = new List<IConfigDialog>();
            configDialogs.Add((IConfigDialog)new YahooDataProvider());
            configDialogs.Add((IConfigDialog)new YahooIntradayDataProvider());
            configDialogs.Add((IConfigDialog)new GoogleDataProvider());
            configDialogs.Add((IConfigDialog)new GoogleIntradayDataProvider());
            configDialogs.Add((IConfigDialog)new ABCDataProvider());
         }
         return configDialogs;
      }
      #endregion
      #region CSV FILE IO
      protected virtual bool ParseOptixFile(StockSerie stockSerie, string rootFolder)
      {
         string fileName = rootFolder + OPTIX_SUBFOLDER + "\\" + stockSerie.ShortName + ".csv";

         if (File.Exists(fileName))
         {
            using (StreamReader sr = new StreamReader(fileName))
            {
               string line = sr.ReadLine();  // Skip the first line
               StockDailyValue dailyValue = null;
               while (!sr.EndOfStream)
               {
                  line = sr.ReadLine();  // Skip the first line
                  var fields = line.Split(',');
                  if (fields.Count() < 2 || string.IsNullOrEmpty(fields[0])) continue;
                  DateTime date = DateTime.Parse(fields[0], usCulture);

                  // Try to find the matching value at date in the serie
                  if (date >= stockSerie.Keys.First() && stockSerie.ContainsKey(date))
                  {
                     dailyValue = stockSerie[date];
                     dailyValue.OPTIX = float.Parse(fields[1], usCulture);
                  }
               }
            }
            return true;
         }
         else
         {
            return false;
         }
      }

      protected bool DownloadOptixData(string rootFolder, StockSerie stockSerie)
      {
         if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() && stockSerie.HasOptix)
         {
            NotifyProgress("Downloading Optix for" + stockSerie.StockGroup.ToString());

            if (!stockSerie.Initialise())
            {
               return false;
            }

            if (stockSerie.Values.Last().OPTIX != 0.0f)
            {
               return false;
            }

            string optixFileName = rootFolder + OPTIX_SUBFOLDER + "\\" + stockSerie.ShortName + ".csv";

            using (WebClient wc = new WebClient())
            {
               wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
               wc.DownloadFile(stockSerie.OptixURL, optixFileName);
               stockSerie.IsInitialised = false;
            }
         }
         return true;
      }


      protected virtual bool ParseCSVFile(StockSerie stockSerie, string fileName)
      {
         if (File.Exists(fileName))
         {
            using (StreamReader sr = new StreamReader(fileName))
            {
               sr.ReadLine();  // Skip the first line
               StockDailyValue readValue = null;
               while (!sr.EndOfStream)
               {
                  readValue = this.ReadMarketDataFromCSVStream(sr, stockSerie.StockName, true);
                  if (readValue != null && !stockSerie.ContainsKey(readValue.DATE))
                  {
                     stockSerie.Add(readValue.DATE, readValue);
                     readValue.Serie = stockSerie;
                  }
               }
            }
            return true;
         }
         else
         {
            return false;
         }
      }
      protected virtual StockDailyValue ReadMarketDataFromCSVStream(StreamReader sr, string stockName, bool useAdjusted)
      {
         StockDailyValue stockValue = null;
         try
         {
            // File format
            // Date,Open,High,Low,Close,Volume,Adj Close (UpVolume, Tick, Uptick)
            // 2010-06-18,10435.00,10513.75,10379.60,10450.64,4555360000,10450.64
            string[] row = sr.ReadLine().Split(',');
            switch (row.Length)
            {
               case 5: // Date,Open,High,Low,Close
                  {
                     DateTime day = DateTime.Parse(row[0], usCulture);
                     stockValue = new StockDailyValue(
                        stockName,
                        float.Parse(row[1], usCulture),
                        float.Parse(row[2], usCulture),
                        float.Parse(row[3], usCulture),
                        float.Parse(row[4], usCulture),
                        0,
                        day);
                  }
                  break;
               case 6: // Date,Open,High,Low,Close,Volume
                  {
                     string[] dateFields = row[0].Split('/');

                     if (row[1] == "-" || row[2] == "-" || row[3] == "-" || row[4] == "-")
                        return null;

                     if (row[5] == "-") row[5] = "0";

                     DateTime day = DateTime.Parse(row[0]);
                     // new DateTime(int.Parse(dateFields[2]), int.Parse(dateFields[0]), int.Parse(dateFields[1]));
                     int index = row[5].IndexOf('.');
                     if (index > 0)
                     {
                        row[5] = row[5].Remove(index);
                     }
                     stockValue = new StockDailyValue(
                        stockName,
                        float.Parse(row[1], usCulture),
                        float.Parse(row[2], usCulture),
                        float.Parse(row[3], usCulture),
                        float.Parse(row[4], usCulture),
                        long.Parse(row[5], usCulture),
                        day);
                  }
                  break;
               case 7: // Date,Open,High,Low,Close,Volume,Adj Close
                  if (useAdjusted || row[4] != row[6])
                  {
                     float close = float.Parse(row[4], usCulture);
                     float adjClose = float.Parse(row[6], usCulture);
                     float adjRatio = adjClose / close;
                     stockValue = new StockDailyValue(
                         stockName,
                         float.Parse(row[1], usCulture) * adjRatio,
                         float.Parse(row[2], usCulture) * adjRatio,
                         float.Parse(row[3], usCulture) * adjRatio,
                         adjClose,
                         long.Parse(row[5], usCulture),
                         DateTime.Parse(row[0], usCulture));
                  }
                  else
                  {
                     stockValue = new StockDailyValue(
                             stockName,
                             float.Parse(row[1], usCulture),
                             float.Parse(row[2], usCulture),
                             float.Parse(row[3], usCulture),
                             float.Parse(row[4], usCulture),
                             long.Parse(row[5], usCulture),
                             DateTime.Parse(row[0], usCulture));
                  }
                  break;
               case 10: // Date,Open,High,Low,Close,Volume,Adj Close (UpVolume, Tick, Uptick)
                  {
                     stockValue = new StockDailyValue(
                                 stockName,
                                 float.Parse(row[1], usCulture),
                                 float.Parse(row[2], usCulture),
                                 float.Parse(row[3], usCulture),
                                 float.Parse(row[4], usCulture),
                                 long.Parse(row[5], usCulture),
                                 long.Parse(row[7], usCulture),
                                 int.Parse(row[8], usCulture),
                                 int.Parse(row[9], usCulture),
                                 DateTime.Parse(row[0], usCulture));
                  }
                  break;
            }
         }
         catch (System.Exception ex)
         {
            StockLog.Write(ex.Message);
            // Assume input is right, Ignore invalid lines
         }
         return stockValue;
      }
      public static StockDailyValue ReadPCRFromCSVStream(StreamReader sr, string name)
      {
         StockDailyValue stockValue = null;
         try
         {
            string[] row = sr.ReadLine().Split(',');
            if (row.GetLength(0) == 5)
            {
               stockValue = new StockDailyValue(
                   name,
                   float.Parse(row[4], usCulture),
                   float.Parse(row[4], usCulture),
                   float.Parse(row[4], usCulture),
                   float.Parse(row[4], usCulture),
                   long.Parse(row[3], usCulture),
                   DateTime.Parse(row[0], usCulture));
            }
         }
         catch (System.Exception)
         {
            // Assume input is right, Ignore invalid lines
         }
         return stockValue;
      }
      public static StockDailyValue ReadCBOEIndexDataFromCSVStream(StreamReader sr, string name)
      {
         StockDailyValue stockValue = null;
         try
         {
            // File format
            // Date,Close
            // 10-May-07,27.09
            string[] row = sr.ReadLine().Split(',');
            if (row.GetLength(0) == 2 && row[1] != "")
            {
               stockValue = new StockDailyValue(
                   name,
                   float.Parse(row[1], usCulture),
                   float.Parse(row[1], usCulture),
                   float.Parse(row[1], usCulture),
                   float.Parse(row[1], usCulture),
                   100,
                   DateTime.Parse(row[0], usCulture));
            }
         }
         catch (System.Exception)
         {
            // Assume input is right, Ignore invalid lines
         }
         return stockValue;
      }
      #endregion


      public virtual bool LoadIntradayDurationArchiveData(string rootFolder, StockSerie serie, StockSerie.StockBarDuration duration)
      {
         return false;
      }
   }
}
