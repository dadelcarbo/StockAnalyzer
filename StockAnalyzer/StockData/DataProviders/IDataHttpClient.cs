using StockAnalyzer.StockClasses;
using System;

namespace StockAnalyzer.StockData.DataProviders
{
    public interface IDataHttpClient
    {
        StockDailyValue[] GetData(StockInstrument instrument, DateTime startDate);
    }
}
