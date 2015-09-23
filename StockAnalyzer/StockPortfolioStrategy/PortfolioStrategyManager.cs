using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalyzer.StockClasses;
using StockAnalyzer.Portofolio;

namespace StockAnalyzer.StockPortfolioStrategy
{
    public class PortfolioStrategyManager
    {
        static private List<string> strategyList = null;
        private PortfolioStrategyManager()
        {
        }
        static public List<string> GetStrategyList()
        {
            if (strategyList == null)
            {
                strategyList = new List<string>();
                PortfolioStrategyManager sm = new PortfolioStrategyManager();
                foreach (Type t in sm.GetType().Assembly.GetTypes().Where(t => t.GetInterface("IStockPortfolioStrategy") != null))
                {
                    if (t.Name != "StockPortfolioStrategyBase")
                    {
                        strategyList.Add(t.Name);
                    }
                }
            }
            strategyList.Sort();
            return strategyList;
        }
        static public IStockPortfolioStrategy CreateStrategy(string name, List<StockSerie> stockSeries, StockPortofolio portfolio, StockDictionary stockDictionary)
        {
            IStockPortfolioStrategy strategy = null;
            if (strategyList == null)
            {
                GetStrategyList();
            }
            if (strategyList.Contains(name))
            {
                PortfolioStrategyManager sm = new PortfolioStrategyManager();
                strategy = (IStockPortfolioStrategy)sm.GetType().Assembly.CreateInstance("StockAnalyzer.StockPortfolioStrategy." + name);
                strategy.Initialise(stockSeries, portfolio, stockDictionary);
            }
            return strategy;
        }
    }
}
