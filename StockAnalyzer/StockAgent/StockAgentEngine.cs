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


        public void GreedySelection(IEnumerable<StockSerie> series, StockBarDuration duration, int minIndex, float stopATR, Func<StockTradeSummary, float> selector)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // Calcutate Parameters Ranges
            IStockAgent bestAgent = null;
            var parameters = StockAgentBase.GetParamRanges(this.AgentType);


            var stockSeries = new List<StockSerie>();
            if (ProgressChanged != null)
            {
                this.ProgressChanged(this, new ProgressChangedEventArgs(0, null));
            }


            int nbSteps = series.Count();
            int modulo = Math.Max(1, nbSteps / 100);
            int nb = 0;
            foreach (var serie in series)
            {
                if (Worker != null && Worker.CancellationPending)
                    return;

                serie.BarDuration = StockBarDuration.Daily;
                serie.IsInitialised = false;
                if (serie.Initialise())
                {
                    serie.BarDuration = duration;
                    if (serie.Count > minIndex)
                    {
                        stockSeries.Add(serie);
                    }
                    if (ProgressChanged != null && nb % modulo == 0)
                    {
                        int percent = (nb * 100) / nbSteps;
                        this.ProgressChanged(this, new ProgressChangedEventArgs(percent, null));
                    }
                }
                nb++;
            }

            int dim = parameters.Count;
            var sizes = parameters.Select(p => p.Value.Count).ToArray();
            var indexes = parameters.Select(p => 0).ToArray();
            nbSteps = sizes.Aggregate(1, (i, j) => i * j);
            modulo = Math.Max(1, nbSteps / 100);
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
                this.Perform(series, minIndex, duration, stopATR);

                // Select Best (after cleaning outliers)
                this.Agent.TradeSummary.CleanOutliers();
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
            string msg = bestAgent.TradeSummary.ToLog(duration) + Environment.NewLine;
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

        public void Perform(IEnumerable<StockSerie> series, int minIndex, StockBarDuration duration, float stopATR)
        {
            foreach (var serie in series.Where(s => s.Count > minIndex))
            {
                if (!this.Agent.Initialize(serie, duration, stopATR))
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
                            this.Agent.OpenTrade(serie, i + 1);
                            break;
                        case TradeAction.Sell:
                            this.Agent.CloseTrade(i + 1);
                            break;
                        case TradeAction.PartSell:
                            i++;
                            this.Agent.PartlyCloseTrade(i + 1);
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
