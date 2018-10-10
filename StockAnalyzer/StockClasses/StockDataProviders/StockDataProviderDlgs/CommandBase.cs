using System;
using System.Windows.Input;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs
{
    public class CommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public CommandBase(Action action)
        {
            _action = action;
        }

        private Action _action;
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke();
        }
    }
}
