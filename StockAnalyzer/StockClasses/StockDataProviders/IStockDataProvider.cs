namespace StockAnalyzer.StockClasses.StockDataProviders
{
   public enum StockDataProvider
   {
      ABC,
      Yahoo,
      YahooIntraday,
      Google,
      GoogleIntraday,
      Rydex,
      CBOE,
      Harpex,
      Portofolio,
      Generated,
      AAII,
      Test,
      Breadth,
      Ratio,
      COT
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

      bool LoadIntradayDurationArchiveData(string rootFolder, StockSerie serie, StockSerie.StockBarDuration duration);
   }
}