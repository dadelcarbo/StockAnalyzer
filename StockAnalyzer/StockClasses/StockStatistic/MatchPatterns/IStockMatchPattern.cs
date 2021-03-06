﻿namespace StockAnalyzer.StockClasses.StockStatistic.MatchPatterns
{
    public interface IStockMatchPattern
    {
        // Return true or false in the stock is matching the pattern at index
        bool MatchPattern(StockSerie stockSerie, int index);

        string Suffix { get; }
    }
}
