using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;

namespace UltimateDownloader.Downloader
{
    public class DownloadItem
    {
        public string StockName { get; set; }
        public long Id { get; set; }
        public BarDuration BarDuration { get; set; }
        public StockDataProvider DataProvider { get; set; }

        public DateTime? LastDownload { get; set; }
    }
}
