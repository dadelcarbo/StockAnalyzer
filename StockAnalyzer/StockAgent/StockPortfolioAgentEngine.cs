using StockAnalyzer.StockAgent.Filters;
using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace StockAnalyzer.StockAgent
{
    public class StockPortfolioAgentEngine
    {
        public StockTradeSummary TradeSummary { get; private set; }

        public Type AgentType { get; private set; }
        public IEnumerable<StockAgentParam> Parameters { get; private set; }

        public event ProgressChangedEventHandler ProgressChanged;

        public event AgentPerformedHandler AgentPerformed;

        public StockPortfolioAgentEngine(Type agentType, IEnumerable<StockAgentParam> parameters)
        {
            this.AgentType = agentType;
            this.Parameters = parameters;
            this.TradeSummary = new StockTradeSummary();
        }

        public BackgroundWorker Worker { get; set; }
        public string Report { get; set; }

        private EquityValue[] equityCurve;
        public EquityValue[] EquityCurve => equityCurve;

        public void PerformPortfolio(IEnumerable<StockSerie> series, int minIndex, StockBarDuration duration, int maxPositions)
        {
            IStockFilter filter = new ROCFilter();

            var agentTuples = new List<Tuple<StockSerie, IStockAgent>>();
            foreach (var serie in series)
            {
                var agent = StockAgentBase.CreateInstance(this.AgentType);
                agent.SetParams(this.Parameters);
                if (agent.Initialize(serie, duration, 0))
                {
                    agentTuples.Add(Tuple.Create(serie, agent));
                }
            }

            var refSerie = StockDictionary.Instance["CAC40"];
            refSerie.BarDuration = duration;
            var refVarSerie = refSerie.GetSerie(StockDataType.VARIATION);
            var openTrades = new List<StockTrade>();

            float cash = 10000f;
            float equity = cash;
            float refEquity = cash;
            equityCurve = new EquityValue[refSerie.Keys.Count];
            int i = 0;
            for (; i < minIndex; i++)
            {
                equityCurve[i] = new EquityValue { X = i, Y = equity, Ref = refEquity };
            }

            foreach (var date in refSerie.Keys.Skip(minIndex))
            {
                equity = cash;
                refEquity *= (1.0f + refVarSerie[i]);
                // Sell Positions
                foreach (var trade in openTrades)
                {
                    var agent = agentTuples.FirstOrDefault(t => trade.Serie == t.Item1).Item2;
                    int index = trade.Serie.IndexOf(date);
                    if (index < minIndex || index >= trade.Serie.LastIndex) // Date not exists
                        continue;

                    if (agent.CanClose(index))
                    {
                        Console.WriteLine($"{date.ToShortDateString()} - Sell {trade.Serie.StockName}");
                        trade.CloseAtOpen(index + 1);
                        equity += trade.ExitAmount;
                        cash += trade.ExitAmount;
                    }
                    else
                    {
                        equity += trade.AmountAt(index);
                    }
                }
                openTrades.RemoveAll(t => t.IsClosed);

                // Identify buy list
                int nbBuys = maxPositions - openTrades.Count;
                if (nbBuys > 0)
                {
                    var buyOpportunities = new List<Tuple<int, StockSerie>>();
                    foreach (var tuple in agentTuples)
                    {
                        if (openTrades.Count >= maxPositions)
                            break;
                        if (openTrades.Any(t => t.Serie == tuple.Item1))
                            continue;
                        int index = tuple.Item1.IndexOf(date);
                        if (index < minIndex || index >= tuple.Item1.LastIndex) // Date not exists
                            continue;

                        if (tuple.Item2.CanOpen(index))
                        {
                            buyOpportunities.Add(new Tuple<int, StockSerie>(index, tuple.Item1));
                        }
                    }

                    foreach (var tuple in buyOpportunities.OrderByDescending(b => filter.EvaluateRank(b.Item2, b.Item1)).Take(nbBuys))
                    {
                        Console.WriteLine($"{date.ToShortDateString()} - Buy {tuple.Item2.StockName}");
                        var trade = new StockTrade(tuple.Item2, tuple.Item1 + 1);
                        trade.Qty = (int)(cash / (maxPositions - openTrades.Count) / trade.EntryValue);
                        cash -= trade.EntryAmount;
                        this.TradeSummary.Trades.Add(trade);
                        openTrades.Add(trade);
                    }

                }

                equityCurve[i] = new EquityValue { X = i, Y = equity, Ref = refEquity, NbPos = openTrades.Count };
                i++;
                Console.WriteLine($"Equity:{equity}");
                Console.WriteLine($"Cash:{cash}");
            }
        }

        public string ToLog()
        {
            return this.TradeSummary.ToLog();
        }
    }
}
