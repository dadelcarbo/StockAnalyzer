using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace UltimateChartist.DataModels.DataProviders;

public abstract class StockDataProviderBase : IStockDataProvider
{
    List<Type> instanceTypes = new List<Type>();
    public StockDataProviderBase()
    {
        if (instanceTypes.Contains(this.GetType()))
            throw new InvalidOperationException($"Instance of {this.GetType().Name} already exists");
        this.instanceTypes.Add(this.GetType());
    }
    public List<Instrument> Instruments { get; } = new List<Instrument>();
    public abstract string Name { get; }
    public abstract string DisplayName { get; }
    public abstract BarDuration[] BarDurations { get; }

    protected Instrument RefInstrument { get; set; }

    #region CONSTANTS
    static protected string DAILY_SUBFOLDER = @"daily";
    static protected string INTRADAY_SUBFOLDER = @"intraday";
    static protected string WEEKLY_SUBFOLDER = @"weekly";
    static protected string DAILY_ARCHIVE_SUBFOLDER = @"archive\daily";
    static protected string INTRADAY_ARCHIVE_SUBFOLDER = @"archive\intraday";

    static protected CultureInfo frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
    static protected CultureInfo usCulture = CultureInfo.GetCultureInfo("en-US");

    public const int ARCHIVE_START_YEAR = 2000;
    public static int LOAD_START_YEAR => Settings.Default.LoadStartYear;

    #endregion

    #region IStockDataProvider default implementation

    abstract public List<StockBar> LoadData(Instrument instrument, BarDuration duration);

    abstract public List<StockBar> DownloadData(Instrument instrument, BarDuration duration);

    public abstract void InitDictionary();

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

    private static SortedDictionary<StockDataProvider, IStockDataProvider> dataProviders = new SortedDictionary<StockDataProvider, IStockDataProvider>();
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
                case StockDataProvider.Boursorama:
                    dataProvider = new BoursoramaDataProvider();
                    break;
                //case StockDataProvider.Portfolio:
                //    dataProvider = new PortfolioDataProvider();
                //    break;
                //case StockDataProvider.Breadth:
                //    dataProvider = new BreadthDataProvider();
                //    break;
                //case StockDataProvider.Investing:
                //    dataProvider = new InvestingDataProvider();
                //    break;
                //case StockDataProvider.InvestingIntraday:
                //    dataProvider = new InvestingIntradayDataProvider();
                //    break;
                //case StockDataProvider.SocGenIntraday:
                //    dataProvider = new SocGenIntradayDataProvider();
                //    break;
                //case StockDataProvider.SaxoIntraday:
                //    dataProvider = new SaxoIntradayDataProvider();
                //    break;
                //case StockDataProvider.Citifirst:
                //    dataProvider = new CitifirstDataProvider();
                //    break;
                //case StockDataProvider.Yahoo:
                //    dataProvider = new YahooDataProvider();
                //    break;
                //case StockDataProvider.YahooIntraday:
                //    dataProvider = new YahooIntradayDataProvider();
                // break;
                default:
                    throw new ArgumentException($"DataProvider {dataProviderType} not supported");
            }
            dataProviders.Add(dataProviderType, dataProvider);
        }
        return dataProviders[dataProviderType];
    }

    public static IEnumerable<Instrument> InitStockDictionary()
    {
        var allInstruments = new List<Instrument>();
        foreach (StockDataProvider dataProviderType in Enum.GetValues(typeof(StockDataProvider)))
        {
            try
            {
                IStockDataProvider dataProvider = GetDataProvider(dataProviderType);
                if (dataProvider != null)
                {
                    dataProvider.InitDictionary();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        return dataProviders.Values.Where(dp => dp != null).SelectMany(dp => dp.Instruments);
    }

    #region Config Dialogs
    //private static List<IConfigDialog> configDialogs = null;
    //public static List<IConfigDialog> GetConfigDialogs()
    //{
    //    if (configDialogs == null)
    //    {
    //        configDialogs = new List<IConfigDialog>();
    //        configDialogs.Add(new ABCDataProvider());
    //        configDialogs.Add(new InvestingIntradayDataProvider());
    //        configDialogs.Add(new InvestingDataProvider());
    //        configDialogs.Add(new SaxoIntradayDataProvider());
    //        configDialogs.Add(new CitifirstDataProvider());
    //        configDialogs.Add(new YahooDataProvider());
    //        configDialogs.Add(new YahooIntradayDataProvider());
    //    }
    //    return configDialogs;
    //}
    #endregion

    #endregion

    public virtual void OpenInDataProvider(Instrument instrument)
    {
        MessageBox.Show($"Open in {this.GetType().Name.Replace("DataProvider", "")} not implemeted", "Error", MessageBoxButton.OK);
    }
}
