using System;

namespace ConsoleApp5
{
    public interface IStockPriceProvider
    {
        float GetClosingPrice(string stockName, DateTime date);
    }
}