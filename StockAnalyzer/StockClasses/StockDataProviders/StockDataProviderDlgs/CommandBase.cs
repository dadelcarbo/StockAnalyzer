using System;
using System.ComponentModel;
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

        private readonly Action _action;
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke();
        }
    }
    public class ParamCommandBase<T> : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public ParamCommandBase(Action<T> action)
        {
            _action = action;
        }

        private readonly Action<T> _action;
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke((T)parameter);
        }
    }
    public class CommandBase<T> : ICommand where T : INotifyPropertyChanged
    {
        public event EventHandler CanExecuteChanged;

        readonly string _propertyName;
        readonly T _sourceObject;
        readonly Func<T, bool> _selector;
        public CommandBase(Action action, T sourceObject, Func<T, bool> selector, string propertyName)
        {
            _action = action;
            _propertyName = propertyName;
            _selector = selector;
            _sourceObject = sourceObject;
            _sourceObject.PropertyChanged += SourceObject_PropertyChanged;
        }

        private void SourceObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _propertyName)
            {
                this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private readonly Action _action;
        public bool CanExecute(object parameter)
        {
            return _selector(_sourceObject);
        }

        public void Execute(object parameter)
        {
            _action?.Invoke();
        }
    }
}
