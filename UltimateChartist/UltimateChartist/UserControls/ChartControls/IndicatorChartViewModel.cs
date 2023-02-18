using System.Windows.Input;
using Telerik.Windows.Controls;
using UltimateChartist.Indicators;
using UltimateChartist.UserControls.ChartControls.Indicators;

namespace UltimateChartist.UserControls.ChartControls;

public class IndicatorChartViewModel : ViewModelBase
{
    public ChartViewModel ChartViewModel { get; }
    public IndicatorViewModel Indicator { get; }

    public IndicatorChartViewModel(ChartViewModel chartViewModel, IIndicator indicator)
    {
        ChartViewModel = chartViewModel;
        Indicator = new IndicatorViewModel(indicator, chartViewModel.StockSerie);
    }

    private string name;
    public string Name { get => name; set { if (name != value) { name = value; RaisePropertyChanged(); } } }


    #region Commands

    private DelegateCommand deleteIndicatorCommand;
    public ICommand DeleteIndicatorCommand => deleteIndicatorCommand ??= new DelegateCommand(DeleteIndicator);

    private void DeleteIndicator(object commandParameter)
    {
        ChartViewModel.RemoveIndicator(this);
    }
    #endregion
}
