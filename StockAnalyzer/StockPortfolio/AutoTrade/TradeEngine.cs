using StockAnalyzer.StockClasses;
using StockAnalyzer.StockPortfolio.AutoTrade.TradeStrategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace StockAnalyzer.StockPortfolio.AutoTrade
{
    public class TradeEngine
    {
        public static bool IsTest { get; set; } = true;
        public List<TradeAgent> Agents { get; set; } = new List<TradeAgent>();

        static TradeEngine instance;
        public static TradeEngine Instance => instance ??= new TradeEngine();

        private TradeEngine()
        {
            if (IsTest)
            {
                Agents.Add(new TradeAgent()
                {
                    BarDuration = BarDuration.M_5,
                    StockSerie = StockDictionary.Instance["TURBO_DAX LONG"],
                    Strategy = TradeStrategyManager.CreateInstance("Bottom"),
                    Portfolio = StockPortfolio.Portfolios.FirstOrDefault(p => p.Name == "AutoTradeTest"),
                    Ready = true
                });
            }
            else
            {
                Agents.Add(new TradeAgent()
                {
                    BarDuration = BarDuration.M_5,
                    StockSerie = StockDictionary.Instance["TURBO_DAX LONG"],
                    Strategy = TradeStrategyManager.CreateInstance("Bottom"),
                    Portfolio = StockPortfolio.Portfolios.FirstOrDefault(p => p.Name == "AutoTradeTest"),
                    Ready = true
                });
            }
        }

        public bool IsRunning { get; set; }
        public void Start()
        {
            foreach (var agent in Agents.Where(a => a.Ready))
            {
                agent.Start();
            }
            this.IsRunning = true;
        }

        public void Stop()
        {
            foreach (var agent in Agents)
            {
                agent.Stop();
            }
            this.IsRunning = false;
        }

    }
}
