﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StockAnalyzer.StockAgent
{
    public class StockTradeSummary
    {
        public StockTradeSummary()
        {
            this.Trades = new ObservableCollection<StockTrade>();
        }
        public IList<StockTrade> Trades { get; private set; }
        public float MaxDrawdown { get { return this.Trades.Count > 0 ? this.Trades.Min(t => t.DrawDown) : 0f; } }
        public float TotalGain { get { return this.Trades.Any(t => t.Gain >= 0) ? this.Trades.Where(t => t.Gain >= 0).Sum(t => t.Gain) : 0f; } }
        public float TotalLoss { get { return this.Trades.Any(t => t.Gain < 0) ? this.Trades.Where(t => t.Gain < 0).Sum(t => t.Gain) : 0f; } }
        public float AvgGain { get { return this.Trades.Any(t => t.Gain >= 0) ? this.Trades.Where(t => t.Gain >= 0).Average(t => t.Gain) : 0f; } }
        public float AvgLoss { get { return this.Trades.Any(t => t.Gain < 0) ? this.Trades.Where(t => t.Gain < 0).Average(t => t.Gain) : 0f; } }
        public int AvgDuration { get { return this.Trades.Count > 0 ? (int)this.Trades.Average(t => t.Duration) : 0; } }
        public float MaxGain { get { return this.Trades.Count > 0 ? this.Trades.Max(t => t.Gain) : 0f; } }
        public float MaxLoss { get { return this.Trades.Count > 0 ? this.Trades.Min(t => t.Gain) : 0f; } }
        public float ExpectedReturn { get { return this.Trades.Count > 0 ? this.Trades.Average(t => t.Gain) : 0f; } }
        public float ExpectedGainPerBar { get { return this.Trades.Count > 0 ? this.Trades.Average(t => t.Gain) / this.Trades.Average(t => (float)t.Duration) : 0f; } }
        public float CumulGain { get { return this.Trades.Count > 0 ? this.Trades.Sum(t => t.Gain) : 0f; } }

        public int NbTrades => this.Trades.Count;
        public int NbWinTrade => Trades.Count(t => t.Gain >= 0);
        public int NbLostTrade => Trades.Count(t => t.Gain < 0);

        public float WinTradeRatio => NbLostTrade != 0 ? NbWinTrade / (float)NbTrades : 0f;

        public float WinLossRatio => NbLostTrade != 0 ? this.TotalGain / -TotalLoss : 0f;
        public float Kelly => WinTradeRatio - (1.0f - WinTradeRatio) / WinLossRatio;

        public string ToLog()
        {
            string res = "Nb Trade: " + Trades.Count() + Environment.NewLine;
            res += "Nb Win Trade: " + NbWinTrade + Environment.NewLine;
            res += "Nb Lost Trade: " + NbLostTrade + Environment.NewLine;

            res += "Total Gain: " + TotalGain.ToString("P2") + Environment.NewLine;
            res += "Total Loss: " + TotalLoss.ToString("P2") + Environment.NewLine;
            res += "Avg Gain: " + AvgGain.ToString("P2") + Environment.NewLine;
            res += "Avg Loss: " + AvgLoss.ToString("P2") + Environment.NewLine;
            res += "Max Gain: " + MaxGain.ToString("P2") + Environment.NewLine;
            res += "Max Loss: " + MaxLoss.ToString("P2") + Environment.NewLine;
            res += "Max Drawdown: " + MaxDrawdown.ToString("P2") + Environment.NewLine;

            res += "Cumul Gain: " + CumulGain.ToString("P2") + Environment.NewLine;
            res += "Avg Duration: " + AvgDuration.ToString() + Environment.NewLine;
            res += "Win Ratio: " + WinTradeRatio.ToString("P2") + Environment.NewLine;
            res += "Exp Gain: " + ExpectedReturn.ToString("P2") + Environment.NewLine;
            res += "Exp Gain/Bar: " + ExpectedGainPerBar.ToString("P3") + Environment.NewLine;

            res += "Kelly: " + Kelly.ToString("P3") + Environment.NewLine;

            return res;
        }

        public string ToStats()
        {
            string res = Trades.Count() + "\t";
            res += NbWinTrade + "\t";
            res += NbLostTrade + "\t";

            res += MaxGain + "\t";
            res += MaxLoss + "\t";
            res += MaxDrawdown + "\t";

            res += CumulGain + "\t";
            res += AvgDuration + "\t";
            res += WinTradeRatio + "\t";
            res += ExpectedReturn + "\t";
            return res;
        }
    }
}
