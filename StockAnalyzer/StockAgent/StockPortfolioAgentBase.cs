using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System.Reflection;

namespace StockAnalyzer.StockAgent
{
    public abstract class StockPortfolioAgentBase : StockAgentBase, IStockPortfolioAgent
    {
        public FloatSerie RankSerie { get; set; }
    }
}
