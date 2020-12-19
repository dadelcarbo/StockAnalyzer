namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public enum StockDataProvider
    {
        ABC,
        BarChart,
        InvestingIntraday,
        SocGenIntraday,
        BinckPortfolio,
        Generated,
        AAII,
        Test,
        Replay,
        Breadth,
        Investing
    }

    public delegate void DownloadingStockEventHandler(string text);

    public interface IStockDataProvider
    {
        event DownloadingStockEventHandler DownloadStarted;

        bool SupportsIntradayDownload { get; }

        bool LoadData(string rootFolder, StockSerie stockSerie);
        bool DownloadDailyData(string rootFolder, StockSerie stockSerie);
        bool DownloadIntradayData(string rootFolder, StockSerie stockSerie);
        void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download);

        bool LoadIntradayDurationArchiveData(string rootFolder, StockSerie serie, StockBarDuration duration);
    }
}