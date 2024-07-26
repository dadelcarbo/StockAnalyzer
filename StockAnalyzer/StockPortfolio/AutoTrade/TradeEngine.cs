using StockAnalyzer.StockClasses;
using StockAnalyzer.StockPortfolio.AutoTrade.TradeStrategies;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace StockAnalyzer.StockPortfolio.AutoTrade
{
    public class TradeEngine
    {
        public static bool IsTest { get; set; } = false;
        public ObservableCollection<TradeAgent> Agents { get; set; }

        static TradeEngine instance;
        public static TradeEngine Instance => instance ??= new TradeEngine();

        private TradeEngine()
        {
            var agentDefs = TradeAgentDef.AgentDefs;
            if (agentDefs.Count == 0)
            {
                agentDefs.Add(new TradeAgentDef()
                {
                    Id = -1,
                    BarDuration = BarDuration.M_5,
                    StockName = "TURBO_DAX5M LONG",
                    StrategyName = "TrailAtr",
                    PortfolioName = "AutoTradeTest",
                    Draft = false,
                    AutoStart = false
                });
                agentDefs.Add(new TradeAgentDef()
                {
                    Id = -2,
                    BarDuration = BarDuration.M_5,
                    StockName = "TURBO_DAX5M SHORT",
                    StrategyName = "TrailAtr",
                    PortfolioName = "AutoTradeTest",
                    Draft = false,
                    AutoStart = false
                });
                agentDefs.Add(new TradeAgentDef()
                {
                    Id = 1,
                    BarDuration = BarDuration.M_5,
                    StockName = "TURBO_DAX5M LONG",
                    StrategyName = "TrailAtr",
                    PortfolioName = "@SaxoTitre",
                    Draft = false,
                    AutoStart = false
                });
                agentDefs.Add(new TradeAgentDef()
                {
                    Id = 2,
                    BarDuration = BarDuration.M_5,
                    StockName = "TURBO_DAX5M SHORT",
                    StrategyName = "TrailAtr",
                    PortfolioName = "@SaxoTitre",
                    Draft = false,
                    AutoStart = false
                });
                agentDefs.Add(new TradeAgentDef()
                {
                    Id = 3,
                    BarDuration = BarDuration.H_2,
                    StockName = "TURBO_DAX LONG",
                    StrategyName = "TrailAtr",
                    PortfolioName = "@SaxoTitre",
                    Draft = false,
                    AutoStart = false
                });
                agentDefs.Add(new TradeAgentDef()
                {
                    Id = 4,
                    BarDuration = BarDuration.H_2,
                    StockName = "TURBO_DAX SHORT",
                    StrategyName = "TrailAtr",
                    PortfolioName = "@SaxoTitre",
                    Draft = false,
                    AutoStart = false
                });

                TradeAgentDef.Save();
            }

            this.Agents = new ObservableCollection<TradeAgent>(agentDefs.Select(ad => new TradeAgent(ad)));

            foreach (var agent in Agents.Where(a => a.AgentDef.AutoStart && a.Portfolio.SaxoLogin()))
            {
                agent.Start();
            }
        }

        public bool IsRunning { get; set; }
        public void Start()
        {
            foreach (var agent in Agents.Where(a => !a.AgentDef.Draft && a.Portfolio.SaxoLogin()))
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
