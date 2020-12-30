using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalyzer.StockBinckPortfolio;

namespace StockAnalyzer.StockAgent
{
    public class StockTradeSummary
    {
        public StockTradeSummary()
        {
            this.Trades = new List<StockTrade>();
        }
        public List<StockTrade> Trades { get; private set; }
        public float MaxDrawdown { get { return this.Trades.Count > 0 ? this.Trades.Min(t => t.DrawDown) : 0f; } }
        public float MaxGain { get { return this.Trades.Count > 0 ? this.Trades.Max(t => t.Gain) : 0f; } }
        public float MaxLoss { get { return this.Trades.Count > 0 ? this.Trades.Min(t => t.Gain) : 0f; } }
        public float AvgGain { get { return this.Trades.Count > 0 ? this.Trades.Average(t => t.Gain) : 0f; } }
        public float CumulGain { get { return this.Trades.Count > 0 ? this.Trades.Sum(t => t.Gain) : 0f; } }
        public float CompoundGain { get { return this.Trades.Count > 0 ? this.Trades.Select(t => t.Gain + 1).Aggregate(1f, (i, j) => i * j) - 1f : 0f; } }
        private StockPortfolio portfolio;
        public StockPortfolio Portfolio
        {
            get
            {
                if (portfolio == null)
                {
                    portfolio = StockPortfolio.CreateSimulationPortfolio();
                    portfolio.InitFromTradeSummary(this.Trades);
                }
                return portfolio;
            }
            set
            {
                this.portfolio = value;
                portfolio?.InitFromTradeSummary(this.Trades);
            }
        }
        public int NbWinTrade { get { return Trades.Count(t => t.Gain >= 0); } }
        public int NbLostTrade { get { return Trades.Count(t => t.Gain < 0); } }
        public float WinRatio { get { return NbLostTrade != 0 ? NbWinTrade / (float)NbLostTrade : 0f; } }
        public float Expectancy { get { return this.Trades.Count > 0 ? this.Trades.Sum(t => t.Gain) / this.Trades.Count : float.NaN; } }

        public string ToLog()
        {
            string res = "Max Drawdown: " + MaxDrawdown.ToString("P2") + Environment.NewLine;
            res += "Max Gain: " + MaxGain.ToString("P2") + Environment.NewLine;
            res += "Max Loss: " + MaxLoss.ToString("P2") + Environment.NewLine;
            res += "Avg Gain: " + AvgGain.ToString("P2") + Environment.NewLine;
            res += "Cumul Gain: " + CumulGain.ToString("P2") + Environment.NewLine;
            res += "Compound Gain: " + CompoundGain + Environment.NewLine;
            res += "Nb Trade: " + Trades.Count() + Environment.NewLine;
            res += "Nb Win Trade: " + NbWinTrade + Environment.NewLine;
            res += "Nb Lost Trade: " + NbLostTrade + Environment.NewLine;
            res += "Win Ratio: " + WinRatio + Environment.NewLine;
            res += "Expectancy: " + Expectancy + Environment.NewLine;
            res += "Portfolio Return: " + Portfolio.Return.ToString("P2") + Environment.NewLine;

            //res += Environment.NewLine;
            //res += "Best trades" + Environment.NewLine;
            //foreach (var tg in this.Trades.GroupBy(t => t.Serie))
            //{
            //    foreach (var t in tg.OrderByDescending(t => t.Gain).Take(5))
            //    {
            //        res += t.Serie.StockName + ";" + t.EntryDate + ";" + t.ExitDate + ";" + ";" + t.Gain.ToString("P2") + Environment.NewLine;
            //    }
            //}
            return res;
        }

        public string ToStats()
        {
            string res = MaxDrawdown + "\t";
            res += MaxGain + "\t";
            res += MaxLoss + "\t";
            res += AvgGain + "\t";
            res += CumulGain + "\t";
            res += CompoundGain + "\t";
            res += Trades.Count() + "\t";
            res += NbWinTrade + "\t";
            res += NbLostTrade + "\t";
            res += WinRatio + "\t";
            res += Portfolio.Return;
            res += StockPortfolio.MaxPositions + "\t";
            return res;
        }

        public string GetOpenPositionLog()
        {
            string openedPositions = Environment.NewLine + "Opened position: " + Environment.NewLine;
            foreach (var trade in this.Trades.Where(t => !t.IsClosed).OrderBy(t => t.EntryDate))
            {
                var pos = this.Portfolio.OpenedPositions.FirstOrDefault(p => p.StockName == trade.Serie.StockName);
                if (pos == null)
                {
                    openedPositions += "* " + trade.Serie.StockName + Environment.NewLine;
                }
                else
                {
                    openedPositions += trade.Serie.StockName + Environment.NewLine;
                }
            }
            openedPositions += Environment.NewLine + "(*) not in portfolio" + Environment.NewLine;
            return openedPositions;
        }
    }
}
