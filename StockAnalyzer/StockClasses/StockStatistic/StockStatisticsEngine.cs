using System;
using System.Collections.Generic;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockStatistic.MatchPatterns;

namespace StockAnalyzer.StockClasses.StockStatistic
{
    public class StockStatisticsEngine
    {
        private int sampleBefore = 0;
        private int sampleAfter = 0;
        private int sampleSize = 0;

        public StockStatisticsEngine(int before, int after)
        {
            this.Patterns = new List<StockPatternIndex>();
            sampleBefore = before;
            sampleAfter = after;
            sampleSize = before + after;
        }
        private List<StockPatternIndex> Patterns { get; set; }
        private void Clear()
        {
            this.Patterns.Clear();
        }
    }
}
