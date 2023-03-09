using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using UltimateChartist.DataModels.DataProviders.ABC;
using UltimateChartist.DataModels.DataProviders.Boursorama;
using UltimateChartist.DataModels.DataProviders.SaxoTurbo;
using UltimateChartist.Helpers;

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
    public abstract BarDuration DefaultBarDuration { get; }

    protected Instrument RefInstrument { get; set; }

    #region Cache Management

    static protected CultureInfo frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
    static protected CultureInfo usCulture = CultureInfo.GetCultureInfo("en-US");

    public static int LOAD_START_YEAR => Settings.Default.LoadStartYear;

    public static int ARCHIVE_END_YEAR => Settings.Default.LoadStartYear;

    protected string CACHE_FOLDER => Path.Combine(Folders.DataFolder, "Cache", Name);
    protected string ARCHIVE_FOLDER => Path.Combine(Folders.DataFolder, "Archive", Name);
    protected string TEMP_FOLDER => Path.Combine(Folders.DataFolder, "Temp", Name);

    protected string CONFIG_FILE => Path.Combine(Folders.PersonalFolder, $"{Name}.user.cfg");

    protected void InitCacheFolders()
    {
        foreach (var barDuration in this.BarDurations)
        {
            var folder = Path.Combine(CACHE_FOLDER, barDuration.ToString());
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        if (Directory.Exists(TEMP_FOLDER))
        {
            // Purge files at each start
            Directory.Delete(TEMP_FOLDER, true);
        }
    }

    protected virtual string GetCacheFilePath(Instrument instrument)
    {
        return Path.Combine(CACHE_FOLDER, this.DefaultBarDuration.ToString(), GetFileName(instrument));
    }
    protected virtual string GetArchiveFilePath(Instrument instrument)
    {
        return Path.Combine(ARCHIVE_FOLDER, this.DefaultBarDuration.ToString(), GetFileName(instrument));
    }
    protected virtual string GetFileName(Instrument instrument)
    {
        return $"{instrument.Symbol}_{instrument.ISIN}.csv";
    }

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
                case StockDataProvider.SaxoTurbo:
                    dataProvider = new SaxoTurboDataProvider();
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
                    break;
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
