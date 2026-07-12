using StockAnalyzer.StockData;
using StockAnalyzer.StockData;
using System;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public enum StockDataProvider
    {
        All,
        ABC,
        Yahoo,
        YahooIntraday,
        InvestingIntraday,
        SocGenIntraday,
        Portfolio,
        Generated,
        Replay,
        Investing,
        SaxoIntraday,
        SaxoIntraday_M5,
        VontobelIntraday,
        BnpIntraday,
        Saxo,
        CNN,
        Breadth,
        yCharts
    }

    public delegate void DownloadingStockEventHandler(string text);

    public interface IStockDataProvider
    {
        event DownloadingStockEventHandler DownloadStarted;

        string DisplayName { get; }

        bool SupportsIntradayDownload { get; }

        /// <summary>
        /// Get data from data provider for a given instrument and bar duration. Returns null if not available.
        /// data provider is in charge of loading or downloading.
        /// </summary>
        /// <param name="instrument"></param>
        /// <param name="barDuration"></param>
        /// <returns></returns>
        DataSerie GetData(StockInstrument instrument, BarDuration barDuration);

        bool LoadData(StockSerie stockSerie);
        bool DownloadDailyData(StockSerie stockSerie);
        bool ForceDownloadData(StockSerie stockSerie);
        bool DownloadIntradayData(StockSerie stockSerie);
        void InitDictionary(StockDictionary stockDictionary, bool download);
        void OpenInDataProvider(StockInstrument stockInstrument);
        /// <summary>
        /// Return true if removed, otherwise need to exclude
        /// </summary>
        /// <param name="stockSerie"></param>
        /// <returns></returns>
        bool RemoveEntry(StockSerie stockSerie);

        void AddSplit(StockSerie stockSerie, DateTime date, float before, float after);
        void ApplyTrimBefore(StockSerie stockSerie, DateTime upToDate);

        bool SupportsDuration(BarDuration duration);

        BarDuration[] SupportedDurations { get; }

        BarDuration DefaultDuration { get; }
    }
}