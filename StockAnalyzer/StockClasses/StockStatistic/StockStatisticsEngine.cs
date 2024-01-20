using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockStatistic
{
    public class StockStatisticsEngine
    {
        private readonly int sampleBefore = 0;
        private readonly int sampleAfter = 0;
        private readonly int sampleSize = 0;

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
