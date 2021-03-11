using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockAgent.Filters
{
    public interface IStockFilter
    {
        float EvaluateRank(StockSerie stockSerie, int index);
    }
}
