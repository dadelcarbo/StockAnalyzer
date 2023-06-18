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
        Breadth,
        Investing,
        SaxoIntraday,
        Saxo,
        Citifirst
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
    }
}