using System;

namespace StockAnalyzer.StockPortfolio
{
    public class TrailStopHistory
    {
        DateTime StartDate { get; set; }
        DateTime? Endate { get; set; }
        float Value { get; set; }
    }
}