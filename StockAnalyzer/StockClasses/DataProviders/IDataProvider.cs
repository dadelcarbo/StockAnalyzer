using StockAnalyzer.StockData;
using StockAnalyzerApp.StockData;
using System;

namespace StockAnalyzer.StockClasses.DataProviders
{
    public enum DataProvider
    {
        All = 0,
        ABC,
        Yahoo,
        YahooIntraday,
        SocGenIntraday,
        Portfolio,
        Generated,
        Replay,
        SaxoIntraday,
        SaxoIntraday_M5,
        VontobelIntraday,
        BnpIntraday,
        Saxo,
        CNN,
        Breadth,
        yCharts
    }

    public delegate void DownloadingEventHandler(string text);

    public interface IDataProvider
    {
        string DisplayName { get; }

        DataProvider Provider { get; }

        /// <summary>
        /// Initialize the dictionary of available instruments. If download is true, it will download the list of instruments from the data provider.
        /// </summary>
        /// <param name="download"></param>
        void InitDictionary(bool download);

        /// <summary>
        /// Get data from data provider for a given instrument and bar duration. Returns null if not available.
        /// data provider is in charge of loading or downloading.
        /// </summary>
        /// <param name="instrument"></param>
        /// <param name="barDuration"></param>
        /// <returns></returns>
        DataSerie GetData(StockInstrument instrument, BarDuration barDuration);

        /// <summary>
        /// Force download data from data provider for a given instrument. Returns true if download was successful.
        /// </summary>
        /// <param name="instrument"></param>
        /// <returns></returns>
        bool ForceDownloadData(StockInstrument instrument);

        /// <summary>
        /// Open the stock instrument in the data provider's website.
        /// </summary>
        /// <param name="stockInstrument"></param>
        /// 
        void OpenInDataProvider(StockInstrument stockInstrument);

        /// <summary>
        /// Return true if removed, otherwise need to exclude
        /// </summary>
        /// <param name="stockSerie"></param>
        /// <returns></returns>
        bool RemoveEntry(StockInstrument instrument);

        void AddSplit(StockInstrument instrument, DateTime date, float before, float after);

        void ApplyTrimBefore(StockInstrument instrument, DateTime upToDate);

        bool SupportsDuration(BarDuration duration);

        BarDuration[] SupportedDurations { get; }

        BarDuration DefaultDuration { get; }
    }
}