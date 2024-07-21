using StockAnalyzer;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockPortfolio.AutoTrade;
using System.Collections.ObjectModel;
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

            Agents = new ObservableCollection<TradeAgent>(engine.Agents);
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

        public ObservableCollection<TradeAgent> Agents { get; private set; }

    }
}
