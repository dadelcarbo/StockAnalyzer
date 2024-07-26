using StockAnalyzer;
using StockAnalyzer.StockPortfolio.AutoTrade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzerApp.CustomControl.AutoTradeDlg
{
    public enum State
    {
        Stopped,
        Running
    }
    public class AgentRunViewModel : NotifyPropertyChangedBase
    {
        public TradeAgent Agent { get; set; }
        public AgentRunViewModel(TradeAgent agent)
        {
            this.Agent = agent;

            this.Agent.StateChanged += Agent_StateChanged;
            this.Agent.PositionChanged += Agent_PositionChanged;
        }

        private void Agent_PositionChanged(TradeAgent sender)
        {
            this.OnPropertyChanged(nameof(Position));
        }
        public State State => Agent.Running ? State.Running : State.Stopped;

        private void Agent_StateChanged(TradeAgent sender)
        {
            this.OnPropertyChanged(nameof(State));
        }
        public TradePosition Position => Agent.Position;

        public bool IsConnected => Agent.Portfolio.SaxoSilentLogin();

    }
}
