﻿using System;

namespace StockAnalyzer.StockClasses
{
    public interface IStockPriceProvider
    {
        float GetClosingPrice(string stockName, DateTime date, BarDuration duration);

        float GetLastClosingPrice(string stockName);
    }
}