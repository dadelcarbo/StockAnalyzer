using System;

namespace StockAnalyzer.StockClasses
{
    public interface IStockPriceProvider
    {
        float GetClosingPrice(string stockName, DateTime date, StockClasses.BarDuration duration);

        float GetLastClosingPrice(string stockName);
    }
}