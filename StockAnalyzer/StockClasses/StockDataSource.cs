using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockClasses
{
    public class StockDataSource
    {
        public StockDataSource()
        {
            this.Values = new List<StockDailyValue>();
        }
        public List<StockDailyValue> Values;
        public BarDuration Duration;

        public DateTime[] Date { get; private set; }
        public FloatSerie Open { get; private set; }
        public FloatSerie High { get; private set; }
        public FloatSerie Low { get; private set; }
        public FloatSerie Close { get; private set; }
        public long[] Volume { get; private set; }
    }
}
