using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockClasses
{
    public class StockSerieLocker : IDisposable
    {
        private StockSerie stockSerie;

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
