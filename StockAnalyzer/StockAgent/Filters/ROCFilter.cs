using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockAgent.Filters
{
    public class ROCFilter : IStockFilter
    {
        public float EvaluateRank(StockSerie stockSerie, int index)
        {
            return -stockSerie.GetIndicator("ROC(100)").Series[0][index];
        }
    }
}
