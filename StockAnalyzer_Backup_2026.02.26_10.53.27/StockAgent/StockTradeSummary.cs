using StockAnalyzer.StockClasses;
using StockAnalyzerApp;
using System;
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

        public void CleanOutliers()
        {
            if (this.Trades != null && this.Trades.Count > 50)
            {
                this.Trades = new ObservableCollection<StockTrade>(this.Trades.OrderByDescending(t => t.Gain).Skip(3).Reverse().Skip(3));
            }
        }

        public float MaxDrawdown => this.Trades.Count > 0 ? this.Trades.Min(t => t.Drawdown) : 0f;
        public float TotalGain => this.Trades.Any(t => t.Gain >= 0) ? this.Trades.Where(t => t.Gain >= 0).Sum(t => t.Gain) : 0f;
        public float TotalLoss => this.Trades.Any(t => t.Gain < 0) ? this.Trades.Where(t => t.Gain < 0).Sum(t => t.Gain) : 0f;
        public float AvgGain => this.Trades.Any(t => t.Gain >= 0) ? this.Trades.Where(t => t.Gain >= 0).Average(t => t.Gain) : 0f;
        public float AvgLoss => this.Trades.Any(t => t.Gain < 0) ? this.Trades.Where(t => t.Gain < 0).Average(t => t.Gain) : 0f;
        public int AvgDuration => this.Trades.Count > 0 ? (int)this.Trades.Average(t => t.Duration) : 0;
        public float MaxGain => this.Trades.Count > 0 ? this.Trades.Max(t => t.Gain) : 0f;
        public float MaxLoss => this.Trades.Count > 0 ? this.Trades.Min(t => t.Gain) : 0f;
        public float ExpectedReturn => this.Trades.Count > 0 ? this.Trades.Average(t => t.Gain) : 0f;
        public float ExpectedGainPerBar => this.Trades.Count > 0 ? this.Trades.Average(t => t.Gain) / this.Trades.Average(t => (float)t.Duration) : 0f;
        public float CumulGain => this.Trades.Count > 0 ? this.Trades.Sum(t => t.Gain) : 0f;

        public int NbTrades => this.Trades.Count;
        public int NbWinTrade => Trades.Count(t => t.Gain >= 0);
        public int NbLostTrade => Trades.Count(t => t.Gain < 0);

        public float WinTradeRatio => NbLostTrade != 0 ? NbWinTrade / (float)NbTrades : 0f;
        public float RiskRewardRatio => this.Trades.Count > 0 ? this.Trades.Average(t => t.RiskRewardRatio) : 0f;

        public float WinLossRatio => NbLostTrade != 0 ? this.AvgGain / -AvgLoss : 0f;
        public float Kelly => WinTradeRatio - (1.0f - WinTradeRatio) / WinLossRatio;

        public string ToLog(BarDuration duration)
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
            res += Environment.NewLine;
            res += "Avg Duration: " + AvgDuration + Environment.NewLine;
            res += "Win/Loss Ratio: " + this.WinLossRatio.ToString("#.##") + Environment.NewLine;
            res += "Risk/Reward Ratio: " + this.RiskRewardRatio.ToString("#.##") + Environment.NewLine;
            res += "Win Rate: " + WinTradeRatio.ToString("P2") + Environment.NewLine;
            res += "Exp Gain: " + ExpectedReturn.ToString("P2") + Environment.NewLine;
            res += "Exp Gain/Bar: " + ExpectedGainPerBar.ToString("P3") + Environment.NewLine;

            if (duration == BarDuration.Daily)
                res += "Exp Gain/Year: " + (260 * ExpectedGainPerBar).ToString("P3") + Environment.NewLine;
            else if (duration == BarDuration.Weekly)
                res += "Exp Gain/Year: " + (52 * ExpectedGainPerBar).ToString("P3") + Environment.NewLine;
            else if (duration == BarDuration.Monthly)
                res += "Exp Gain/Year: " + (12 * ExpectedGainPerBar).ToString("P3") + Environment.NewLine;

            res += "Kelly: " + Kelly.ToString("P3") + Environment.NewLine;

            return res;
        }

        public string ToStats()
        {
            string res = Trades.Count() + "\t";
            res += NbWinTrade + "\t";
            res += NbLostTrade + "\t";

            res += AvgDuration + "\t";
            res += WinLossRatio.ToString("#.##") + "\t";
            res += RiskRewardRatio.ToString("#.##") + "\t";
            res += WinTradeRatio.ToString("P2", Global.USCulture) + "\t";
            res += ExpectedReturn.ToString("P2", Global.USCulture) + "\t";
            res += ExpectedGainPerBar.ToString("P2", Global.USCulture) + "\t";
            return res;
        }
    }
}
