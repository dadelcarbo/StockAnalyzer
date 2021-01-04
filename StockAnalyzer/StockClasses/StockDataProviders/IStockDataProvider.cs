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

        bool LoadData(StockSerie stockSerie);
        bool DownloadDailyData(StockSerie stockSerie);
        bool ForceDownloadData(StockSerie stockSerie);
        bool DownloadIntradayData(StockSerie stockSerie);
        void InitDictionary(StockDictionary stockDictionary, bool download);

        bool LoadIntradayDurationArchiveData(StockSerie serie, StockBarDuration duration);
    }
}