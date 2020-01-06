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

        static public StockBarDuration[] cacheDurations = new StockBarDuration[]
                  {
                     StockAnalyzer.StockClasses.StockBarDuration.TLB_3D,
                     StockAnalyzer.StockClasses.StockBarDuration.TLB_6D,
                     StockAnalyzer.StockClasses.StockBarDuration.TLB_9D,
                     StockAnalyzer.StockClasses.StockBarDuration.Bar_3, // 15 Min
                     StockAnalyzer.StockClasses.StockBarDuration.Bar_6, // 30 Min
                  };

        public const int ARCHIVE_START_YEAR = 1999;
        public const int LOAD_START_YEAR = 1999;

        private static string rootFolder = null;
        public static string RootFolder
        {
            get
            {
                if (rootFolder == null) rootFolder = StockAnalyzerSettings.Properties.Settings.Default.RootFolder;
                return rootFolder;
            }
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
        public static bool LoadIntradayDurationArchive(string rootFolder, StockSerie serie, StockBarDuration duration)
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

        public static IStockDataProvider GetDataProvider(StockDataProvider dataProviderType)
        {
            IStockDataProvider dataProvider = null;
            switch (dataProviderType)
            {
                case StockDataProvider.ABC:
                    dataProvider = new ABCDataProvider();
                    break;
                case StockDataProvider.CommerzBankIntraday:
                    dataProvider = new CommerzBankIntradayDataProvider();
                    break;
                case StockDataProvider.CBOE:
                    dataProvider = new CBOEDataProvider();
                    break;
                //case StockDataProvider.COT:
                //    dataProvider = new COTDataProvider();
                //    break;
                case StockDataProvider.Portofolio:
                    dataProvider = new PortfolioDataProvider();
                    break;
                case StockDataProvider.BinckPortfolio:
                    dataProvider = new BinckPortfolioDataProvider();
                    break;
                case StockDataProvider.Generated:
                    dataProvider = new GeneratedDataProvider();
                    break;
                case StockDataProvider.Breadth:
                    dataProvider = new BreadthDataProvider();
                    break;
                case StockDataProvider.AAII:
                    dataProvider = new AAIIDataProvider();
                    break;
                case StockDataProvider.Ratio:
                    dataProvider = new RatioDataProvider();
                    break;
                //case StockDataProvider.NASDACQShortInterest:
                //    //dataProvider = new NASDACQShortInterestDataProvider();
                //    break;
                //case StockDataProvider.BarChart:
                //    dataProvider = new BarChartDataProvider();
                //    break;
                case StockDataProvider.Investing:
                    dataProvider = new InvestingDataProvider();
                    break;
                case StockDataProvider.InvestingIntraday:
                    dataProvider = new InvestingIntradayDataProvider();
                    break;
                case StockDataProvider.BNPIntraday:
                    dataProvider = new BNPIntradayDataProvider();
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
                StockBarDuration currentBarDuration = serie.BarDuration;
                serie.BarDuration = new StockBarDuration(BarDuration.Daily, 1);
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
                try
                {
                    IStockDataProvider dataProvider = GetDataProvider(dataProviderType);
                    if (dataProvider != null)
                    {
                        dataProvider.DownloadStarted += downloadListener;
                        dataProvider.InitDictionary(rootFolder, stockDictionary, download);
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

        //protected bool DownloadOptixData(string rootFolder, StockSerie stockSerie)
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
                string[] row = sr.ReadLine().Replace("\"", "").Split(',');

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
                    case 9:
                        // BARCART CSV 
                        {
                            // File format
                            // symbol,timestamp,tradingDay,open,high,low,close,volume,openInterest

                            DateTime day = DateTime.Parse(row[2], usCulture);
                            stockValue = new StockDailyValue(
                                stockName,
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


        public virtual bool LoadIntradayDurationArchiveData(string rootFolder, StockSerie serie, StockBarDuration duration)
        {
            return false;
        }
    }
}
