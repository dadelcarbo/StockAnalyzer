using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace StockAnalyzer.StockAgent
{
    public class StockPortfolioAgentEngine
    {
        public StockTradeSummary TradeSummary { get; private set; }
        public IEnumerable<StockAgentParam> Parameters { get; private set; }

        public event ProgressChangedEventHandler ProgressChanged;

        public event AgentPerformedHandler AgentPerformed;

        public StockPortfolioAgentEngine()
        {
            this.TradeSummary = new StockTradeSummary();
        }

        public BackgroundWorker Worker { get; set; }
        public string Report { get; set; }

        private EquityValue[] equityCurve;
        public EquityValue[] EquityCurve => equityCurve;

        public void PerformPortfolio(List<IStockPortfolioAgent> agents, int minIndex, StockBarDuration duration, PositionManagement positionManagement)
        {
            var candidates = new List<Tuple<IStockPortfolioAgent, float>>();

            var refSerie = StockDictionary.Instance["CAC40"];
            refSerie.BarDuration = duration;
            var refVarSerie = refSerie.GetSerie(StockDataType.VARIATION);
            var openTrades = new List<StockTrade>();

            float cash = positionManagement.PortfolioInitialBalance;
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
                    var agent = agents.FirstOrDefault(a => trade.Serie == a.StockSerie);

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
                int nbBuys = positionManagement.MaxPositions - openTrades.Count;
                if (nbBuys > 0)
                {
                    var buyOpportunities = new List<Tuple<int, IStockPortfolioAgent>>();
                    foreach (var agent in agents)
                    {
                        if (openTrades.Count >= positionManagement.MaxPositions)
                            break;
                        if (openTrades.Any(t => t.Serie == agent.StockSerie))
                            continue;
                        int index = agent.StockSerie.IndexOf(date);
                        if (index < minIndex || index >= agent.StockSerie.LastIndex) // Date not exists
                            continue;

                        if (agent.CanOpen(index))
                        {
                            buyOpportunities.Add(new Tuple<int, IStockPortfolioAgent>(index, agent));
                        }
                    }

                    foreach (var tuple in buyOpportunities.OrderByDescending(b => b.Item2.RankSerie[b.Item1]).Take(nbBuys))
                    {
                        Console.WriteLine($"{date.ToShortDateString()} - Buy {tuple.Item2.StockSerie.StockName}");
                        var trade = new StockTrade(tuple.Item2.StockSerie, tuple.Item1 + 1);
                        trade.Qty = (int)(cash / (positionManagement.MaxPositions - openTrades.Count) / trade.EntryValue);
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

        public string ToLog(StockBarDuration duration)
        {
            return this.TradeSummary.ToLog(duration);
        }
    }
}
