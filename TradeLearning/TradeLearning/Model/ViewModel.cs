using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Telerik.Windows.Controls;
using TradeLearning.Model.Trading;

namespace TradeLearning.Model
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region Start Command
        private DelegateCommand startCommand;
        public ICommand StartCommand
        {
            get
            {
                if (startCommand == null)
                {
                    startCommand = new DelegateCommand(start);
                }

                return startCommand;
            }
        }

        private void start(object commandParameter)
        {
            var engine = new TradingSimulator(this.dataSerie.Data, new BasicTradingStrategy(), 1000);

            engine.Run();

            this.Portfolio = DataSerie.FromArray(engine.PortfolioValue, "Portfolio");
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }
        #endregion

        public ViewModel()
        {
            this.DataSerie = DataSerie.FromArray(DataSerie.GenerateSin(500, 100, 5, 100, 0.01), "Sin");
        }

        private DataSerie dataSerie;
        public DataSerie DataSerie { get => dataSerie; set => SetProperty(ref dataSerie, value); }

        private DataSerie portfolio;
        public DataSerie Portfolio { get => portfolio; set => SetProperty(ref portfolio, value); }
    }
}
