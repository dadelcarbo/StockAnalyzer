using System.Windows.Input;
using Telerik.Windows.Controls;

namespace UltimateChartist.ChartControls
{
    public class IndicatorViewModel : ViewModelBase
    {
        public ChartViewModel ChartViewModel { get; set; }
        public IndicatorViewModel(ChartViewModel chartViewModel)
        {
            this.ChartViewModel = chartViewModel;
        }

        private string name;
        public string Name { get => name; set { if (name != value) { name = value; RaisePropertyChanged(); } } }




        #region Commands

        private DelegateCommand deleteIndicatorCommand;
        public ICommand DeleteIndicatorCommand => deleteIndicatorCommand ??= new DelegateCommand(DeleteIndicator);

        private void DeleteIndicator(object commandParameter)
        {
            this.ChartViewModel.RemoveIndicator(this);
        }
        #endregion
    }
}
