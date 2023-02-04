using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltimateChartist.DataModels.DataProviders
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
        Citifirst
    }

    public delegate void DownloadingStockEventHandler(string text);

    public interface IStockDataProvider
    {
        string DisplayName { get; }

        List<Instrument> Instruments { get; }

        void InitDictionary();
        List<StockBar> LoadData(Instrument instrument, BarDuration duration);

        event DownloadingStockEventHandler DownloadStarted;

        void OpenInDataProvider(Instrument instrument);

        //bool DownloadDailyData(Instrument instrument, BarDuration duration);
        //bool ForceDownloadData(Instrument instrument, BarDuration duration);
    }
}
