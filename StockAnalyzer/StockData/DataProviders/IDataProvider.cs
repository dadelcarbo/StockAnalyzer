using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace StockAnalyzer.StockData.DataProviders
{
    public delegate void DownloadingEventHandler(string text);

    public interface IDataProvider
    {
        string DisplayName { get; }

        DataProvider Provider { get; }

        string ConfigFile { get; }

        BarDuration[] SupportedDurations { get; }
        BarDuration DefaultDuration { get; }
        bool SupportsDuration(BarDuration duration);

        /// <summary>
        /// Initialize the dictionary of available instruments. If download is true, it will download the list of instruments from the data provider.
        /// </summary>
        /// <param name="download"></param>
        void InitDictionary(bool download);

        /// <summary>
        /// Load data from data provider for a given instrument and bar duration. Returns null if not available.
        /// data provider is in charge of loading or downloading.
        /// </summary>
        /// <param name="instrument"></param>
        /// <param name="barDuration"></param>
        /// <returns></returns>
        DataSerie LoadData(StockInstrument instrument, BarDuration barDuration);

        /// <summary>
        /// Download data from data provider for a given instrument. Returns null if not available.
        /// </summary>
        /// <param name="instrument"></param>
        /// <returns></returns>
        DataSerie DownloadData(StockInstrument instrument);

        /// <summary>
        /// Check if download is required
        /// </summary>
        /// <param name="instrument"></param>
        /// <returns></returns>
        bool NeedDownload(StockInstrument instrument);

        /// <summary>
        /// Force download data from data provider for a given instrument.
        /// </summary>
        /// <param name="instrument"></param>
        /// <returns></returns>
        void ForceDownloadData(StockInstrument instrument);

        /// <summary>
        /// Download data in intraday (used for UI refresh)
        /// </summary>
        /// <param name="instrument"></param>
        /// <returns>True if a new value has been downloaded</returns>
        bool UpdateIntradayData(StockInstrument instrument);

        /// <summary>
        /// Open the stock instrument in the data provider's website.
        /// </summary>
        /// <param name="stockInstrument"></param>
        /// 
        void OpenInDataProvider(StockInstrument stockInstrument);

        /// <summary>
        /// Exclude instrument from provider.
        /// </summary>
        /// <param name="instruments"></param>
        /// <returns></returns>
        void Remove(IEnumerable<StockInstrument> instruments);

        void AddSplit(StockInstrument instrument, DateTime date, float before, float after);

        /// <summary>
        /// Keep only data matching predicate
        /// </summary>
        /// <param name="instrument"></param>
        /// <param name="predicate">Condition that specifies which data to be kept</param>
        void KeepOnlyBars(StockInstrument instrument, Func<StockDailyValue, bool> predicate);

        /// <summary>
        ///
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        DialogResult ShowConfigDialog(object param);
    }
}