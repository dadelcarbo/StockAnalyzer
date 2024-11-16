using System;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public enum StockDataProvider
    {
        ABC,
        Yahoo,
        YahooIntraday,
        InvestingIntraday,
        SocGenIntraday,
        Portfolio,
        Generated,
        Replay,
        Investing,
        BoursoIntraday,
        SaxoIntraday,
        SaxoIntraday_M5,
        VontobelIntraday,
        BnpIntraday,
        Saxo,
        CNN,
        Breadth
    }

    public delegate void DownloadingStockEventHandler(string text);

    public interface IStockDataProvider
    {
        event DownloadingStockEventHandler DownloadStarted;

        string DisplayName { get; }

        bool SupportsIntradayDownload { get; }

        bool LoadData(StockSerie stockSerie);
        bool DownloadDailyData(StockSerie stockSerie);
        bool ForceDownloadData(StockSerie stockSerie);
        bool DownloadIntradayData(StockSerie stockSerie);
        void InitDictionary(StockDictionary stockDictionary, bool download);
        void OpenInDataProvider(StockSerie stockSerie);
        /// <summary>
        /// Return true if removed, otherwise need to exclude
        /// </summary>
        /// <param name="stockSerie"></param>
        /// <returns></returns>
        bool RemoveEntry(StockSerie stockSerie);

        void ApplySplit(StockSerie stockSerie, DateTime date, float ratio);
        void ApplyTrim(StockSerie stockSerie, DateTime upToDate);
    }
}