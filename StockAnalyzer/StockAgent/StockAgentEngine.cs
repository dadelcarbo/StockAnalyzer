using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace StockAnalyzer.StockAgent
{
    public delegate void AgentPerformedHandler(IStockAgent agent);
    public class StockAgentEngine
    {
        public IStockAgent Agent { get; set; }
        public StockTradeSummary BestTradeSummary { get; private set; }
        public IStockAgent BestAgent { get; private set; }

        public Type AgentType { get; private set; }

        public event ProgressChangedEventHandler ProgressChanged;

        public event AgentPerformedHandler BestAgentDetected;

        public event AgentPerformedHandler AgentPerformed;

        public StockAgentEngine(Type agentType)
        {
            this.AgentType = agentType;
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

        public void GreedySelection(IEnumerable<StockSerie> series, StockBarDuration duration, int minIndex, int accuracy, Func<StockTradeSummary, float> selector)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // Calcutate Parameters Ranges
            IStockAgent bestAgent = null;
            var parameters = StockAgentBase.GetParamRanges(this.AgentType, accuracy);

            int dim = parameters.Count;
            var sizes = parameters.Select(p => p.Value.Count).ToArray();
            var indexes = parameters.Select(p => 0).ToArray();
            int nbSteps = sizes.Aggregate(1, (i, j) => i * j);
            int modulo = Math.Max(1, nbSteps / 100);
            for (int i = 0; i < nbSteps; i++)
            {
                if (Worker != null && Worker.CancellationPending)
                    return;
                if (ProgressChanged != null && i % modulo == 0)
                {
                    int percent = (i * 100) / nbSteps;
                    this.ProgressChanged(this, new ProgressChangedEventArgs(percent, null));
                }
                // Calculate indexes
                CalculateIndexes(dim, sizes, indexes, i);

                // Set parameter values
                this.Agent = StockAgentBase.CreateInstance(this.AgentType);
                int p = 0;
                foreach (var param in parameters)
                {
                    param.Key.SetValue(this.Agent, param.Value[indexes[p]], null);
                    p++;
                }

                // Perform calculation
                this.Perform(series, minIndex, duration);

                // Select Best
                var tradeSummary = this.Agent.TradeSummary;
                this.AgentPerformed?.Invoke(this.Agent);
                if (bestAgent == null || selector(tradeSummary) > selector(bestAgent.TradeSummary))
                {
                    bestAgent = this.Agent;

                    this.BestAgentDetected?.Invoke(bestAgent);
                }
            }
            stopWatch.Stop();

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            string msg = bestAgent.TradeSummary.ToLog() + Environment.NewLine;
            msg += bestAgent.ToLog() + Environment.NewLine;
            msg += "NB Series: " + series.Count() + Environment.NewLine;
            msg += "Duration: " + elapsedTime;

            this.BestTradeSummary = bestAgent.TradeSummary;
            this.BestAgent = bestAgent;

            this.Report = msg;
        }

        public BackgroundWorker Worker { get; set; }
        public string Report { get; set; }

        public static void CalculateIndexes(int dim, int[] sizes, int[] indexes, int i)
        {
            int tmpIndex = i;

            for (int j = 0; j < dim; j++)
            {
                indexes[j] = tmpIndex % sizes[j];
                tmpIndex /= sizes[j];
            }
        }

        public void GeneticSelection(int nbIteration, int nbAgents, IEnumerable<StockSerie> series, StockBarDuration duration, int minIndex)
        {
            List<IStockAgent> agents = new List<IStockAgent>();

            // Create Agents
            for (int i = 0; i < nbAgents; i++)
            {
                IStockAgent agent = StockAgentBase.CreateInstance(this.AgentType);
                agent.Randomize();
                agents.Add(agent);
            }

            StockAgentBase.GetParamRanges(this.AgentType, 20);

            List<IStockAgent> bestResults = new List<IStockAgent>();
            int nbSelected = 10;
            for (int i = 0; i < nbIteration; i++)
            {
                List<IStockAgent> results = new List<IStockAgent>();
                agents = RemoveDuplicates(agents);

                // Perform action
                foreach (var agent in agents)
                {
                    this.Agent = agent;
                    this.Perform(series, minIndex, duration);
                    results.Add(agent);
                }

                // Select fittest
                var fittest = results.OrderByDescending(a => a.TradeSummary.ExpectedReturn).Take(nbSelected).ToList();
                Console.WriteLine("Fittest:");
                Console.WriteLine(fittest.First().ToLog());
                Console.WriteLine(fittest.First().TradeSummary.ToLog());

                agents.Clear();
                agents.AddRange(fittest);
                int nb = agents.Count;
                for (int k = 0; k < nb; k++)
                {
                    var agent1 = fittest.ElementAt(k);
                    for (int j = k + 1; j < nb; j++)
                    {
                        var agent2 = fittest.ElementAt(j);

                        var newAgents = RemoveDuplicates(agent1.Reproduce(agent2, nbSelected / 2));
                        agents.AddRange(newAgents);
                    }
                }

                // Create Agents
                for (int j = 0; j < nbSelected; j++)
                {
                    IStockAgent agent = StockAgentBase.CreateInstance(this.AgentType);
                    agent.Randomize();
                    agents.Add(agent);
                }
            }
        }

        public void Perform(IEnumerable<StockSerie> series, int minIndex, StockBarDuration duration)
        {
            foreach (var serie in series.Where(s => s.Count > minIndex))
            {
                if (!this.Agent.Initialize(serie, duration))
                {
                    continue;
                }

                var size = serie.Count - 1;
                for (int i = minIndex; i < size; i++)
                {
                    switch (this.Agent.Decide(i))
                    {
                        case TradeAction.Nothing:
                            break;
                        case TradeAction.Buy:
                            i++;
                            this.Agent.OpenTrade(serie, i);
                            break;
                        case TradeAction.Sell:
                            i++;
                            this.Agent.CloseTrade(i);
                            break;
                        case TradeAction.PartSell:
                            i++;
                            this.Agent.PartlyCloseTrade(i);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                this.Agent.EvaluateOpenedPositions();
            }
        }
    }
}
