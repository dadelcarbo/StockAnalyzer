using StockAnalyzer;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockPortfolio.AutoTrade;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StockAnalyzerApp.CustomControl.AutoTradeDlg
{
    public class AutoTradeViewModel : NotifyPropertyChangedBase
    {
        TradeEngine engine => TradeEngine.Instance;

        public AutoTradeViewModel()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            AgentDefs = TradeAgentDef.AgentDefs;
            AgentRuns = new ObservableCollection<AgentRunViewModel>(engine.Agents.Where(a => !a.AgentDef.Draft).Select(a => new AgentRunViewModel(a)));
        }

        #region Start & Stop

        public string StartButtonText => engine.IsRunning ? "Stop" : "Start";

        private CommandBase startCmd;
        public ICommand StartCmd => startCmd ??= new CommandBase(PerformStartCmd);

        private void PerformStartCmd()
        {
            if (engine.IsRunning)
            {
                engine.Stop();

                OnPropertyChanged(nameof(StartButtonText));
            }
            else
            {
                engine.Start();

                OnPropertyChanged(nameof(StartButtonText));
            }
        }

        #endregion

        public ObservableCollection<TradeAgentDef> AgentDefs { get; private set; }


        public ObservableCollection<AgentRunViewModel> AgentRuns { get; private set; }

    }
}
