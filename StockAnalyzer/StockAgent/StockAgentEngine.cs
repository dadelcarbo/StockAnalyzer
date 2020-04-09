using StockAnalyzer.StockAgent.Agents;
using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace StockAnalyzer.StockAgent
{
    public class StockAgentEngine
    {
        public StockContext Context { get; set; }
        public IStockAgent Agent { get; private set; }
        public StockTradeSummary BestTradeSummary { get; private set; }
        public IStockAgent BestAgent { get; private set; }

        public StockAgentEngine()
        {
            this.Context = new StockContext();
        }

        private List<IStockAgent> RemoveDuplicates(IList<IStockAgent> agents)
        {
            var newList = new List<IStockAgent>();
            if (agents.Count == 0) return newList;
            newList.Add(agents[0]);
            for (int i = agents.Count - 1; i >= 1; i--)
            {
                bool same = false;
                foreach (var agent in newList)
                {
                    if (agent.AreSameParams(agents[i]))
                    {
                        same = true;
                        break;
                    }
                }
                if (!same)
                    newList.Insert(0, agents[i]);
            }
            return newList;
        }

        public void GreedySelection(IEnumerable<StockSerie> series, int minIndex)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // Calcutate Parameters Ranges
            IStockAgent agent = new EMAMMAgent(this.Context);
            agent.Randomize();

            this.Agent = agent;
            var bestAgent = agent;
            StockTradeSummary bestTradeSummary = null;

            var parameters = StockAgentBase.GetParamRanges(typeof(EMAMMAgent), 50);

            int dim = parameters.Count;
            var sizes = parameters.Select(p => p.Value.Count).ToArray();
            var indexes = parameters.Select(p => 0).ToArray();
            int nbSteps = sizes.Aggregate(1, (i, j) => i * j);
            for (int i = 0; i < nbSteps; i++)
            {
                // Calculate indexes
                CalculateIndexes(dim, sizes, indexes, i);

                // Set parameter values
                int p = 0;
                foreach (var param in parameters)
                {
                    param.Key.SetValue(agent, param.Value[indexes[p]], null);
                    p++;
                }

                // Perform calculation
                this.Agent = agent;
                this.Perform(series, minIndex);

                // Select Best
                var tradeSummary = this.Context.GetTradeSummary();
                if (bestTradeSummary == null || tradeSummary.CompoundGain > bestTradeSummary.CompoundGain)
                {
                    bestTradeSummary = tradeSummary;
                    bestAgent = agent;

                    agent = new EMAMMAgent(this.Context);
                }
            }
            stopWatch.Stop();

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            string msg = "Fittest:" + Environment.NewLine;
            msg += bestTradeSummary.ToLog();
            msg += bestAgent.ToLog();
            msg += "Duration: " + elapsedTime;

            this.BestTradeSummary = bestTradeSummary;
            this.BestAgent = bestAgent;

            MessageBox.Show(msg, "Greedy calculation over", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void CalculateIndexes(int dim, int[] sizes, int[] indexes, int i)
        {
            int tmpIndex = i;

            for (int j = 0; j < dim; j++)
            {
                indexes[j] = tmpIndex % sizes[j];
                tmpIndex /= sizes[j];
            }
        }

        public void GeneticSelection(int nbIteration, int nbAgents, IEnumerable<StockSerie> series, int minIndex)
        {
            List<IStockAgent> agents = new List<IStockAgent>();

            // Create Agents
            for (int i = 0; i < nbAgents; i++)
            {
                //IStockAgent agent = new StupidAgent(this.Context);
                //IStockAgent agent = new HigherLowAgent(this.Context);
                IStockAgent agent = new EMAAgent(this.Context);
                agent.Randomize();
                agents.Add(agent);
            }

            StockAgentBase.GetParamRanges(typeof(EMAAgent), 20);

            Dictionary<IStockAgent, float> bestResults = new Dictionary<IStockAgent, float>();
            int nbSelected = 10;
            for (int i = 0; i < nbIteration; i++)
            {
                Dictionary<IStockAgent, StockTradeSummary> results = new Dictionary<IStockAgent, StockTradeSummary>();
                agents = RemoveDuplicates(agents);

                // Perform action
                foreach (var agent in agents)
                {
                    this.Agent = agent;
                    this.Perform(series, minIndex);

                    var tradeSummary = this.Context.GetTradeSummary();

                    results.Add(agent, tradeSummary);
                }

                // Select fittest
                var fittest = results.OrderByDescending(a => a.Value.AvgGain).Take(nbSelected).ToList();
                Console.WriteLine("Fittest:");
                Console.WriteLine(fittest.First().Value.ToLog());
                Console.WriteLine(fittest.First().Key.ToLog());

                agents.Clear();
                agents.AddRange(fittest.Select(k => k.Key));
                int nb = agents.Count;
                for (int k = 0; k < nb; k++)
                {
                    var agent1 = fittest.ElementAt(k).Key;
                    for (int j = k + 1; j < nb; j++)
                    {
                        var agent2 = fittest.ElementAt(j).Key;

                        var newAgents = RemoveDuplicates(agent1.Reproduce(agent2, nbSelected / 2));
                        agents.AddRange(newAgents);
                    }
                }
                // Create Agents
                for (int j = 0; j < nbSelected; j++)
                {
                    //IStockAgent agent = new StupidAgent(this.Context);
                    //IStockAgent agent = new HigherLowAgent(this.Context);
                    IStockAgent agent = new EMAAgent(this.Context);
                    agent.Randomize();
                    agents.Add(agent);
                }
            }
        }

        public void Perform(IEnumerable<StockSerie> series, int minIndex)
        {
            this.Context.Clear();

            foreach (var serie in series)
            {
                serie.ResetIndicatorCache();
                this.Context.Serie = serie;
                this.Agent.Initialize(serie);

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
