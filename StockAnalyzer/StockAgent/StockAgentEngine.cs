using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System;
using System.Linq;
using System.Collections.Generic;

namespace StockAnalyzer.StockAgent
{
    public class StockAgentEngine
    {
        public StockContext Context { get; set; }
        public IStockAgent Agent { get; private set; }

        public StockAgentEngine()
        {
            this.Context = new StockContext();

            this.Agent = new StupidAgent(this.Context);
        }

        public void GeneticSelection(int nbIteration, int nbAgents, IEnumerable<StockSerie> series, int minIndex)
        {
            int iteration = 0;
            List<IStockAgent> agents = new List<IStockAgent>();

            // Create Agents
            for (int i = 0; i < nbAgents; i++)
            {
                //IStockAgent agent = new StupidAgent(this.Context);
                IStockAgent agent = new HigherLowAgent(this.Context);
                agent.Randomize();
                agents.Add(agent);
            }

            Dictionary<IStockAgent, float> bestResults = new Dictionary<IStockAgent, float>();

            for (int i = 0; i < nbIteration; i++)
            {
                Dictionary<IStockAgent, StockTradeSummary> results = new Dictionary<IStockAgent, StockTradeSummary>();
                // Perform action
                foreach (var agent in agents)
                {
                    this.Agent = agent;
                    this.Perform(series, minIndex);

                    var tradeSummary = this.Context.GetTradeSummary();

                    results.Add(agent, tradeSummary);
                }

                // Select fittest
                int nbSelected = 10;
                var fittest = results.OrderByDescending(a => a.Value.AvgGain).Take(nbSelected).ToList();
                Console.WriteLine("Fittest:");
                Console.WriteLine(fittest.First().Value.ToLog());
                Console.WriteLine(fittest.First().Key.ToLog());

                //Console.WriteLine(tradeSummary.ToLog());
                agents.Clear();
                agents.AddRange(fittest.Select(k=>k.Key));
                for (int k = 0; k < nbSelected; k++)
                {
                    var agent1 = fittest.ElementAt(k).Key;
                    for (int j = k + 1; j < nbSelected; j++)
                    {
                        var agent2 = fittest.ElementAt(j).Key;
                        agents.AddRange(agent1.Reproduce(agent2, nbSelected / 2));
                    }
                }
            }
        }

        public void Perform(IEnumerable<StockSerie> series, int minIndex)
        {
            this.Context.Clear();

            foreach (var serie in series)
            {
                this.Context.Serie = serie;

                for (int i = minIndex; i < this.Context.Serie.Count; i++)
                {
                    this.Context.CurrentIndex = i;

                    switch (this.Agent.Decide())
                    {
                        case TradeAction.Nothing:
                            break;
                        case TradeAction.Buy:
                            this.Context.OpenTrade(i + 1);
                            break;
                        case TradeAction.Sell:
                            this.Context.CloseTrade(i);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            //var tradeSummary = this.Context.GettradeSummary();
            //Console.WriteLine(tradeSummary.ToLog());

            //foreach (var trade in this.Context.TradeLog)
            //{
            //    Console.WriteLine(trade.ToLog());
            //}
        }
    }
}
