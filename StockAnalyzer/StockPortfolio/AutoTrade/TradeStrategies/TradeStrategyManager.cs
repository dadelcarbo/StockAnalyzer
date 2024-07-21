using StockAnalyzer.StockAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockPortfolio.AutoTrade.TradeStrategies
{
    public class TradeStrategyManager
    {
        #region Static Methods
        static List<string> strategyNames = null;
        public static List<string> GetStrategyNames()
        {
            if (strategyNames != null)
                return strategyNames;

            strategyNames = new List<string>();
            foreach (Type t in typeof(ITradeStrategy).Assembly.GetTypes())
            {
                Type st = t.GetInterface("ITradeStrategy");
                if (st != null)
                {
                    if (!t.Name.EndsWith("Base"))
                    {
                        strategyNames.Add(t.Name.Replace("Strategy", ""));
                    }
                }
            }
            strategyNames.Sort();
            return strategyNames;
        }

        static public ITradeStrategy CreateInstance(string shortName)
        {
            Type type = typeof(ITradeStrategy).Assembly.GetType($"StockAnalyzer.StockPortfolio.AutoTrade.TradeStrategies.{shortName}Strategy");
            return (ITradeStrategy)Activator.CreateInstance(type);
        }
        #endregion
    }
}