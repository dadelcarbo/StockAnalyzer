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
    }
}
