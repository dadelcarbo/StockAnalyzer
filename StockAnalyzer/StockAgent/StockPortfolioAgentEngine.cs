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

        public void PerformPortfolio(IEnumerable<StockSerie> series, int minIndex, StockBarDuration duration, int maxPositions)
        {
            var agentTuples = new List<Tuple<StockSerie, IStockAgent>>();
            foreach (var serie in series)
            {
                var agent = StockAgentBase.CreateInstance(this.AgentType);
                if (agent.Initialize(serie, duration))
                {
                    foreach (var param in this.Parameters)
                    {
                        agent.SetParam(param.Property, param.Value);
                    }
                    agentTuples.Add(Tuple.Create(serie, agent));
                }
            }

            var refSerie = StockDictionary.Instance["CAC40"];
            refSerie.Initialise();
            var openTrades = new List<StockTrade>();
            foreach (var date in refSerie.Keys.Skip(minIndex))
            {
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
                    }
                }
                openTrades.RemoveAll(t => t.IsClosed);

                foreach (var tuple in agentTuples)
                {
                    if (openTrades.Any(t => t.Serie == tuple.Item1))
                        continue;
                    int index = tuple.Item1.IndexOf(date);
                    if (index < minIndex || index >= tuple.Item1.LastIndex) // Date not exists
                        continue;

                    if (tuple.Item2.CanOpen(index))
                    {
                        Console.WriteLine($"{date.ToShortDateString()} - Buy {tuple.Item1.StockName}");
                        var trade = new StockTrade(tuple.Item1, index + 1);
                        this.TradeSummary.Trades.Add(trade);
                        openTrades.Add(trade);

                        if (openTrades.Count > maxPositions)
                            break;
                    }
                }
            }
        }

        public string ToLog()
        {
            return this.TradeSummary.ToLog();
        }
    }
}
