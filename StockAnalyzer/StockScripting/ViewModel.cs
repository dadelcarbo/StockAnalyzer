using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace StockAnalyzer.StockScripting
{
    public class ViewModel : NotifyPropertyChangedBase
    {
        public ViewModel()
        {
        }

        public System.Collections.IEnumerable Scripts => StockScriptManager.Instance.StockScripts;

        private StockScript script;
        public StockScript Script { get => script; set => SetProperty(ref script, value); }

        private CommandBase addCommand;
        public ICommand AddCommand => addCommand ??= new CommandBase(Add);

        private void Add()
        {
        }
    }
}
