using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockData;

namespace StockAnalyzer.StockScripting
{
    public class TestStockFilterImpl : StockFilterBase
    {
        protected override bool MatchFilter(DataSerie dataSerie, StockDailyValue lastBar)
        {
            return true;
        }
    }
}