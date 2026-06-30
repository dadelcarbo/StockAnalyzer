using StockAnalyzer.StockData;
using StockAnalyzerApp.StockData;
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

        DataSerie LoadData(StockInstrument instrument, BarDuration barDuration);

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
    }
}