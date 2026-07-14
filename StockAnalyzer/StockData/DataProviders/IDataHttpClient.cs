using StockAnalyzer.StockClasses;
using System;

namespace StockAnalyzer.StockData.DataProviders
{
    public interface IDataHttpClient
    {
        string FormatUrl(StockInstrument instrument);

        StockDailyValue[] GetData(StockInstrument instrument, DateTime startDate);
    }
}
