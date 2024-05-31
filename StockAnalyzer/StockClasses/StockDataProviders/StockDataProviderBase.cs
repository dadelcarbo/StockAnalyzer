using StockAnalyzer.StockClasses.StockDataProviders.Bourso;
using StockAnalyzer.StockClasses.StockDataProviders.CitiFirst;
using StockAnalyzer.StockClasses.StockDataProviders.CNN;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockClasses.StockDataProviders.Vontobel;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using StockAnalyzerSettings.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public abstract class StockDataProviderBase : IStockDataProvider
    {
        protected static readonly DateTime refDate = new DateTime(1970, 01, 01);

        protected bool needDownload = true;

        public const int ARCHIVE_START_YEAR = 2000;
        public static int LOAD_START_YEAR => Settings.Default.LoadStartYear;

        private static string dataFolder = null;
        public static string DataFolder
        {
            get => dataFolder ??= Folders.DataFolder;
            set
            {
                dataFolder = value;
            }
        }
        public static bool IntradayDownloadSuspended { get; set; } = false;

        protected StockSerie RefSerie { get; set; }

        public abstract string DisplayName { get; }

        #region CONSTANTS
        static protected string DAILY_SUBFOLDER = @"\daily";
        static protected string INTRADAY_SUBFOLDER = @"\intraday";
        static protected string WEEKLY_SUBFOLDER = @"\weekly";
        static protected string DAILY_ARCHIVE_SUBFOLDER = @"\archive\daily";
        static protected string INTRADAY_ARCHIVE_SUBFOLDER = @"\archive\intraday";

        public static string URL_PREFIX_INVESTING => Settings.Default.InvestingUrlRoot;

        static protected CultureInfo frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
        static protected CultureInfo usCulture = CultureInfo.GetCultureInfo("en-US");
        #endregion
        #region IStockDataProvider default implementation

        abstract public bool SupportsIntradayDownload { get; }
        virtual public bool LoadData(StockSerie stockSerie)
        {
            // Read archive first
            string fileName = stockSerie.Symbol + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv";
            string fullFileName = DataFolder + DAILY_ARCHIVE_SUBFOLDER + stockSerie.DataProvider.ToString() + "\\" + fileName;
            bool res = ParseCSVFile(stockSerie, fullFileName);

            fullFileName = DataFolder + DAILY_SUBFOLDER + stockSerie.DataProvider.ToString() + "\\" + fileName;
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

        private static readonly SortedDictionary<StockDataProvider, IStockDataProvider> dataProviders = new SortedDictionary<StockDataProvider, IStockDataProvider>();
        public static IStockDataProvider GetDataProvider(StockDataProvider dataProviderType)
        {
            if (!dataProviders.ContainsKey(dataProviderType))
            {
                IStockDataProvider dataProvider = null;
                switch (dataProviderType)
                {
                    case StockDataProvider.ABC:
                        dataProvider = new ABCDataProvider();
                        break;
                    case StockDataProvider.Portfolio:
                        dataProvider = new PortfolioDataProvider();
                        break;
                    case StockDataProvider.Breadth:
                        dataProvider = new BreadthDataProvider();
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
                    case StockDataProvider.SaxoIntraday:
                        dataProvider = new SaxoIntradayDataProvider();
                        break;
                    case StockDataProvider.VontobelIntraday:
                        dataProvider = new VontobelIntradayDataProvider();
                        break;
                    case StockDataProvider.Saxo:
                        dataProvider = new SaxoDataProvider();
                        break;
                    case StockDataProvider.CNN:
                        dataProvider = new CnnDataProvider();
                        break;
                    case StockDataProvider.Citifirst:
                        dataProvider = new CitifirstDataProvider();
                        break;
                    case StockDataProvider.Yahoo:
                        dataProvider = new YahooDataProvider();
                        break;
                    case StockDataProvider.YahooIntraday:
                        dataProvider = new YahooIntradayDataProvider();
                        break;
                    case StockDataProvider.BoursoIntraday:
                        dataProvider = new BoursoIntradayDataProvider();
                        break;
                    default:
                        break;
                }
                dataProviders.Add(dataProviderType, dataProvider);
            }
            return dataProviders[dataProviderType];
        }
        public static bool DownloadSerieData(StockSerie serie)
        {
            if (serie == null)
                return false;

            IStockDataProvider dataProvider = GetDataProvider(serie.DataProvider);
            if (dataProvider == null)
            {
                return false;
            }
            else
            {
                using (new StockSerieLocker(serie))
                {
                    BarDuration currentBarDuration = serie.BarDuration;
                    if (serie.DataProvider != StockDataProvider.Saxo)
                        serie.BarDuration = BarDuration.Daily;
                    bool res = dataProvider.DownloadDailyData(serie);
                    if (dataProvider.SupportsIntradayDownload)
                    {
                        res |= dataProvider.DownloadIntradayData(serie);
                    }

                    serie.BarDuration = currentBarDuration;
                    return res;
                }
            }
        }
        public static bool DownloadIntadaySerieData(StockSerie serie, BarDuration barDuration)
        {
            if (serie == null)
                return false;

            IStockDataProvider dataProvider = GetDataProvider(serie.DataProvider);
            if (dataProvider == null)
            {
                return false;
            }
            else
            {
                using (new StockSerieLocker(serie))
                {
                    if (serie.DataProvider != StockDataProvider.Saxo)
                        serie.BarDuration = barDuration;
                    bool res = dataProvider.DownloadDailyData(serie);

                    return res;
                }
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
                BarDuration currentBarDuration = serie.BarDuration;
                serie.BarDuration = BarDuration.Daily;
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
                configDialogs.Add(new ABCDataProvider());
                configDialogs.Add(new InvestingIntradayDataProvider());
                configDialogs.Add(new InvestingDataProvider());
                configDialogs.Add(new SaxoIntradayDataProvider());
                configDialogs.Add(new CitifirstDataProvider());
                configDialogs.Add(new YahooDataProvider());
                configDialogs.Add(new BoursoIntradayDataProvider());
            }
            return configDialogs;
        }
        #endregion
        #region CSV FILE IO

        protected virtual bool ParseCSVFile(StockSerie stockSerie, string fileName)
        {
            if (File.Exists(fileName))
            {
                using StreamReader sr = new StreamReader(fileName);
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
            catch (Exception ex)
            {
                StockLog.Write(ex.Message);
                // Assume input is right, Ignore invalid lines
            }
            return stockValue;
        }
        #endregion


        public virtual void OpenInDataProvider(StockSerie stockSerie)
        {
            MessageBox.Show($"Open in {this.GetType().Name.Replace("DataProvider", "")} not implemeted", "Error", MessageBoxButton.OK);
        }

        public virtual bool RemoveEntry(StockSerie stockSerie)
        {
            return false;
        }
    }
}