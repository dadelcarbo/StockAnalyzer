using System.Collections.Generic;

namespace UltimateChartist.DataModels.DataProviders;

public enum StockDataProvider
{
    ABC,
    Boursorama,
    Yahoo,
    YahooIntraday,
    InvestingIntraday,
    SocGenIntraday,
    Portfolio,
    Generated,
    Replay,
    Breadth,
    Investing,
    SaxoTurbo,
    Citifirst
}

public delegate void DownloadingStockEventHandler(string text);

public interface IStockDataProvider
{
    /// <summary>
    /// Used in folder structure
    /// </summary>
    string Name { get; }
    /// <summary>
    /// Used in UIs
    /// </summary>
    string DisplayName { get; }

    List<Instrument> Instruments { get; }

    BarDuration[] BarDurations { get; }
    BarDuration DefaultBarDuration { get; }

    void InitDictionary();
    List<StockBar> LoadData(Instrument instrument, BarDuration duration);

    List<StockBar> DownloadData(Instrument instrument, BarDuration duration);

    event DownloadingStockEventHandler DownloadStarted;

    void OpenInDataProvider(Instrument instrument);
}
