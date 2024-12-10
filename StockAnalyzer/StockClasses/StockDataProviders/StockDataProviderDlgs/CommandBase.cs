using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs
{
    /// <summary>
    /// Can always execute
    /// </summary>
    public class AsyncCommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged;

        Func<bool> _canExecute;
        IEnumerable<string> _properties;
        public AsyncCommandBase(Func<Task> execute, Func<bool> canExecute = null, INotifyPropertyChanged viewModel = null, IEnumerable<string> properties = null)
        {
            _action = execute;

            _canExecute = canExecute;
            _properties = properties;

            if (viewModel != null)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_properties == null || _properties.Contains(e.PropertyName))
                this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        private readonly Func<Task> _action;
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }

        public async void Execute(object parameter)
        {
            await _action();
        }
    }
    public class CommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged;

        Func<bool> _canExecute;
        IEnumerable<string> _properties;
        public CommandBase(Action action, Func<bool> canExecute = null, INotifyPropertyChanged viewModel = null, IEnumerable<string> properties = null)
        {
            _action = action;

            _canExecute = canExecute;
            _properties = properties;

            if (viewModel != null)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private readonly Action _action;
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }

        public void Execute(object parameter)
        {
            _action?.Invoke();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_properties == null || _properties.Contains(e.PropertyName))
                this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
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
