using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public abstract class StockDataProviderBase : IStockDataProvider
    {
        protected bool needDownload = true;

        public const int ARCHIVE_START_YEAR = 2000;
        public static int LOAD_START_YEAR => StockAnalyzerSettings.Properties.Settings.Default.LoadStartYear;

        private static string rootFolder = null;
        public static string RootFolder
        {
            get => rootFolder ?? (rootFolder = StockAnalyzerSettings.Properties.Settings.Default.RootFolder);
            set
            {
                rootFolder = value;
            }
        }

        #region CONSTANTS
        static protected string DAILY_SUBFOLDER = @"\data\daily";
        static protected string INTRADAY_SUBFOLDER = @"\data\intraday";
        static protected string WEEKLY_SUBFOLDER = @"\data\weekly";
        static protected string DAILY_ARCHIVE_SUBFOLDER = @"\data\archive\daily";
        static protected string INTRADAY_ARCHIVE_SUBFOLDER = @"\data\archive\intraday";

        static protected CultureInfo frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
        static protected CultureInfo usCulture = CultureInfo.GetCultureInfo("en-US");
        #endregion
        #region IStockDataProvider default implementation

        abstract public bool SupportsIntradayDownload { get; }
        virtual public bool LoadData(StockSerie stockSerie)
        {
            // Read archive first
            string fileName = stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv";
            string fullFileName = RootFolder + "\\data\\archive\\daily\\" + stockSerie.DataProvider.ToString() + "\\" + fileName;
            bool res = ParseCSVFile(stockSerie, fullFileName);

            fullFileName = RootFolder + "\\data\\daily\\" + stockSerie.DataProvider.ToString() + "\\" + fileName;
            return ParseCSVFile(stockSerie, fullFileName) || res;
        }
        public virtual bool ForceDownloadData(StockSerie stockSerie)
        {
            return this.DownloadDailyData(stockSerie);
        }
        abstract public bool DownloadDailyData(StockSerie stockSerie);
        virtual public bool DownloadIntradayData(StockSerie stockSerie)
        {
            return this.SupportsIntradayDownload;
        }
        abstract public void InitDictionary(StockDictionary stockDictionary, bool download);

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
        public static bool LoadSerieData(StockSerie serie)
        {
            IStockDataProvider dataProvider = GetDataProvider(serie.DataProvider);
            if (dataProvider == null)
            {
                return false;
            }
            else
            {
                return dataProvider.LoadData(serie);
            }
        }
        public static bool LoadIntradayDurationArchive(StockSerie serie, StockBarDuration duration)
        {
            IStockDataProvider dataProvider = GetDataProvider(serie.DataProvider);
            if (dataProvider == null)
            {
                return false;
            }
            else
            {
                return dataProvider.LoadIntradayDurationArchiveData(serie, duration);
            }
        }

        public static IStockDataProvider GetDataProvider(StockDataProvider dataProviderType)
        {
            IStockDataProvider dataProvider = null;
            switch (dataProviderType)
            {
                case StockDataProvider.ABC:
                    dataProvider = new ABCDataProvider();
                    break;
                case StockDataProvider.BinckPortfolio:
                    dataProvider = new BinckPortfolioDataProvider();
                    break;
                case StockDataProvider.Breadth:
                    dataProvider = new BreadthDataProvider();
                    break;
                case StockDataProvider.AAII:
                    dataProvider = new AAIIDataProvider();
                    break;
                case StockDataProvider.Investing:
                    dataProvider = new InvestingDataProvider();
                    break;
                case StockDataProvider.InvestingIntraday:
                    dataProvider = new InvestingIntradayDataProvider();
                    break;
                case StockDataProvider.SocGenIntraday:
                    dataProvider = new SocGenIntradayDataProvider();
                    break;
                case StockDataProvider.Test:
                    break;
                default:
                    break;
            }
            return dataProvider;
        }
        public static bool DownloadSerieData(StockSerie serie)
        {
            IStockDataProvider dataProvider = GetDataProvider(serie.DataProvider);
            if (dataProvider == null)
            {
                return false;
            }
            else
            {
                StockBarDuration currentBarDuration = serie.BarDuration;
                serie.BarDuration = new StockBarDuration(BarDuration.Daily, 1);
                bool res = dataProvider.DownloadDailyData(serie);
                if (dataProvider.SupportsIntradayDownload)
                {
                    res |= dataProvider.DownloadIntradayData(serie);
                }

                serie.BarDuration = currentBarDuration;
                return res;
            }
        } 
        public static bool ForceDownloadSerieData(StockSerie serie)
        {
            IStockDataProvider dataProvider = GetDataProvider(serie.DataProvider);
            if (dataProvider == null)
            {
                return false;
            }
            else
            {
                StockBarDuration currentBarDuration = serie.BarDuration;
                serie.BarDuration = new StockBarDuration(BarDuration.Daily, 1);
                bool res = dataProvider.ForceDownloadData(serie);
                if (dataProvider.SupportsIntradayDownload)
                {
                    res |= dataProvider.DownloadIntradayData(serie);
                }

                serie.BarDuration = currentBarDuration;
                return res;
            }
        }
        public static void InitStockDictionary(StockDictionary stockDictionary, bool download, DownloadingStockEventHandler downloadListener)
        {
            foreach (StockDataProvider dataProviderType in Enum.GetValues(typeof(StockDataProvider)))
            {
                try
                {
                    IStockDataProvider dataProvider = GetDataProvider(dataProviderType);
                    if (dataProvider != null)
                    {
                        dataProvider.DownloadStarted += downloadListener;
                        dataProvider.InitDictionary(stockDictionary, download);
                        dataProvider.DownloadStarted -= downloadListener;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static List<IConfigDialog> configDialogs = null;
        public static List<IConfigDialog> GetConfigDialogs()
        {
            if (configDialogs == null)
            {
                configDialogs = new List<IConfigDialog>();
                //configDialogs.Add((IConfigDialog)new YahooDataProvider());
                //configDialogs.Add((IConfigDialog)new YahooIntradayDataProvider());
                //configDialogs.Add((IConfigDialog)new GoogleDataProvider());
                //configDialogs.Add((IConfigDialog)new GoogleIntradayDataProvider());
                configDialogs.Add((IConfigDialog)new ABCDataProvider());
                configDialogs.Add((IConfigDialog)new InvestingIntradayDataProvider());
                configDialogs.Add((IConfigDialog)new InvestingDataProvider());
            }
            return configDialogs;
        }
        #endregion
        #region CSV FILE IO
        //protected virtual bool ParseOptixFile(StockSerie stockSerie, string rootFolder)
        //{
        //   string fileName = rootFolder + OPTIX_SUBFOLDER + "\\" + stockSerie.ShortName + ".csv";

        //   if (File.Exists(fileName))
        //   {
        //      using (StreamReader sr = new StreamReader(fileName))
        //      {
        //         string line = sr.ReadLine();  // Skip the first line
        //         StockDailyValue dailyValue = null;
        //         while (!sr.EndOfStream)
        //         {
        //            line = sr.ReadLine();  // Skip the first line
        //            var fields = line.Split(',');
        //            if (fields.Count() < 2 || string.IsNullOrEmpty(fields[0])) continue;
        //            DateTime date = DateTime.Parse(fields[0], usCulture);

        //            // Try to find the matching value at date in the serie
        //            if (date >= stockSerie.Keys.First() && stockSerie.ContainsKey(date))
        //            {
        //               dailyValue = stockSerie[date];
        //               dailyValue.OPTIX = float.Parse(fields[1], usCulture);
        //            }
        //         }
        //      }
        //      return true;
        //   }
        //   else
        //   {
        //      return false;
        //   }
        //}

        //protected bool DownloadOptixData(StockSerie stockSerie)
        //{
        //   if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() && stockSerie.HasOptix)
        //   {
        //      NotifyProgress("Downloading Optix for" + stockSerie.StockGroup.ToString());

        //      if (!stockSerie.Initialise())
        //      {
        //         return false;
        //      }

        //      if (stockSerie.Values.Last().OPTIX != 0.0f)
        //      {
        //         return false;
        //      }

        //      string optixFileName = rootFolder + OPTIX_SUBFOLDER + "\\" + stockSerie.ShortName + ".csv";

        //      using (WebClient wc = new WebClient())
        //      {
        //         wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
        //         wc.DownloadFile(stockSerie.OptixURL, optixFileName);
        //         stockSerie.IsInitialised = false;
        //      }
        //   }
        //   return true;
        //}

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
                        if (readValue != null && readValue.DATE.Year >= LOAD_START_YEAR && !stockSerie.ContainsKey(readValue.DATE))
                        {
                            stockSerie.Add(readValue.DATE, readValue);
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
                string[] row = sr.ReadLine().Replace("\"", "").Split(',');

                switch (row.Length)
                {
                    case 5: // Date,Open,High,Low,Close
                        {
                            DateTime day = DateTime.Parse(row[0], usCulture);
                            stockValue = new StockDailyValue(
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
                                    float.Parse(row[1], usCulture),
                                    float.Parse(row[2], usCulture),
                                    float.Parse(row[3], usCulture),
                                    float.Parse(row[4], usCulture),
                                    long.Parse(row[5], usCulture),
                                    DateTime.Parse(row[0], usCulture));
                        }
                        break;
                    case 9:
                        // BARCART CSV 
                        {
                            // File format
                            // symbol,timestamp,tradingDay,open,high,low,close,volume,openInterest

                            DateTime day = DateTime.Parse(row[2], usCulture);
                            stockValue = new StockDailyValue(
                                float.Parse(row[3], usCulture),
                                float.Parse(row[4], usCulture),
                                float.Parse(row[5], usCulture),
                                float.Parse(row[6], usCulture),
                                long.Parse(row[7], usCulture),
                                day);
                        }
                        break;
                    case 10: // Date,Open,High,Low,Close,Volume,Adj Close (UpVolume, Tick, Uptick)
                        {
                            stockValue = new StockDailyValue(
                                        float.Parse(row[1], usCulture),
                                        float.Parse(row[2], usCulture),
                                        float.Parse(row[3], usCulture),
                                        float.Parse(row[4], usCulture),
                                        long.Parse(row[5], usCulture),
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
        #endregion


        public virtual bool LoadIntradayDurationArchiveData(StockSerie serie, StockBarDuration duration)
        {
            return false;
        }
    }
}
