using System;

namespace StockAnalyzer.StockClasses
{
    public class StockSerieLocker : IDisposable
    {
        private readonly StockSerie stockSerie;

        public StockSerieLocker(StockSerie serie)
        {
            this.stockSerie = serie;
            stockSerie.Lock();
        }
        public void Dispose()
        {
            stockSerie.UnLock();
        }
    }
}
