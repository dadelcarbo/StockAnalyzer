using StockAnalyzer.StockClasses;
using System.Collections.ObjectModel;
using System.Linq;

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
                    Instrument = StockDictionary.GetInstrument("TURBO_DAX5M LONG"),
                    StrategyName = "TrailAtr",
                    PortfolioName = "AutoTradeTest",
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
