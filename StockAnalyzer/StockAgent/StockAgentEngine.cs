using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace StockAnalyzer.StockAgent
{
    public delegate void AgentPerformedHandler(IStockAgent agent, IStockEntryStop entryStop, IStockEntryTarget entryTarget);
    public class StockAgentEngine
    {
        public IStockAgent Agent { get; set; }
        public Type AgentType { get; private set; }


        public IStockEntryStop EntryStop { get; set; }
        public Type EntryStopType { get; private set; }

        public IStockEntryTarget EntryTarget { get; set; }
        public Type EntryTargetType { get; private set; }


        public StockTradeSummary BestTradeSummary { get; private set; }
        public IStockAgent BestAgent { get; private set; }
        public IStockEntryStop BestEntryStop { get; set; }
        public IStockEntryTarget BestEntryTarget { get; set; }

        public event ProgressChangedEventHandler ProgressChanged;

        public event AgentPerformedHandler BestAgentDetected;

        public event AgentPerformedHandler AgentPerformed;

        public StockAgentEngine(Type agentType, Type entryStopType, Type entryTargetType)
        {
            this.AgentType = agentType;
            this.EntryStopType = entryStopType;
            this.EntryTargetType = entryTargetType;
        }


        public void GreedySelection(IEnumerable<StockSerie> series, BarDuration duration, int minIndex, Func<StockTradeSummary, float> selector)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // Calcutate Parameters Ranges
            IStockAgent bestAgent = null;
            IStockEntryStop bestEntryStop = null;
            IStockEntryTarget bestEntryTarget = null;
            var agentParameters = StockAgentBase.GetParamRanges(this.AgentType);
            var entryStopParameters = StockAgentBase.GetParamRanges(this.EntryStopType);
            var entryTargetParameters = StockAgentBase.GetParamRanges(this.EntryTargetType);
            var allParameters = agentParameters.Union(entryStopParameters).Union(entryTargetParameters);

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

                serie.BarDuration = BarDuration.Daily;
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

            int dim = allParameters.Count();
            var sizes = allParameters.Select(p => p.Value.Count).ToArray();
            var indexes = allParameters.Select(p => 0).ToArray();
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
                this.Agent = (IStockAgent)Activator.CreateInstance(this.AgentType);
                int p = 0;
                foreach (var param in agentParameters)
                {
                    param.Key.SetValue(this.Agent, param.Value[indexes[p]], null);
                    p++;
                }
                this.EntryStop = (IStockEntryStop)Activator.CreateInstance(this.EntryStopType);
                foreach (var param in entryStopParameters)
                {
                    param.Key.SetValue(this.EntryStop, param.Value[indexes[p]], null);
                    p++;
                }
                this.EntryTarget = (IStockEntryTarget)Activator.CreateInstance(this.EntryTargetType);
                foreach (var param in entryTargetParameters)
                {
                    param.Key.SetValue(this.EntryTarget, param.Value[indexes[p]], null);
                    p++;
                }

                // Perform calculation
                this.Perform(series, minIndex, duration);

                // Select Best (after cleaning outliers)
                this.Agent.TradeSummary.CleanOutliers();
                var tradeSummary = this.Agent.TradeSummary;
                this.AgentPerformed?.Invoke(this.Agent, this.EntryStop, this.EntryTarget);
                if (bestAgent == null || selector(tradeSummary) > selector(bestAgent.TradeSummary))
                {
                    bestAgent = this.Agent;
                    bestEntryStop = this.EntryStop;
                    bestEntryTarget = this.EntryTarget;

                    this.BestAgentDetected?.Invoke(bestAgent, this.EntryStop, this.EntryTarget);
                }
            }
            stopWatch.Stop();

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            string msg = bestAgent.TradeSummary.ToLog(duration) + Environment.NewLine;
            msg += bestAgent.ToLog() + Environment.NewLine;
            msg += bestEntryStop.ToLog() + Environment.NewLine;
            msg += bestEntryTarget.ToLog() + Environment.NewLine;
            msg += "NB Series: " + series.Count() + Environment.NewLine;
            msg += "Duration: " + elapsedTime;

            this.BestTradeSummary = bestAgent.TradeSummary;
            this.BestAgent = bestAgent;
            this.BestEntryStop = bestEntryStop;
            this.BestEntryTarget = bestEntryTarget;

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

        public void Perform(IEnumerable<StockSerie> series, int minIndex, BarDuration duration)
        {
            try
            {
                foreach (var serie in series.Where(s => s.Count > minIndex))
                {
                    if (!this.Agent.Initialize(serie, duration, this.EntryStop, this.EntryTarget))
                    {
                        continue;
                    }
                    if (!this.EntryStop.Initialize(serie, duration))
                    {
                        continue;
                    }
                    if (!this.EntryTarget.Initialize(serie, duration))
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
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    this.Agent.EvaluateOpenedPositions();
                }
            }
            catch (Exception exception)
            {
                StockAnalyzerException.MessageBox(exception);
            }
        }
    }
}
