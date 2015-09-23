using System;

namespace StockAnalyzer.StockClasses
{
    public class StockSession
    {
        public TimeSpan OpenTime { get; private set; }
        public TimeSpan CloseTime { get; private set; }

        public StockSession(TimeSpan openTime, TimeSpan closeTime)
        {
            this.OpenTime = openTime;
            this.CloseTime = closeTime;
        }
    }
}
