using StockAnalyzer.StockClasses;
using System.Windows.Controls;
using System.Windows.Input;

namespace StockAnalyzerApp.CustomControl.MultiTimeFrameDlg
{
    /// <summary>
    /// Interaction logic for MTFUserControl.xaml
    /// </summary>
    public partial class MTFUserControl : UserControl
    {
        public event StockAnalyzerForm.SelectedStockAndDurationChangedEventHandler SelectedStockChanged;
        private MTFViewModel viewModel;

        public MTFUserControl()
        {
            viewModel = new MTFViewModel();
            this.DataContext = viewModel;

            InitializeComponent();
        }


        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = (sender as DataGrid);
            if (SelectedStockChanged == null || dataGrid.SelectedCells.Count < 1) return;

            MTFViewModel.MTFTrend trend = dataGrid.SelectedCells[0].Item as MTFViewModel.MTFTrend;

            StockBarDuration duration = StockBarDuration.Daily;
            string headerName = dataGrid.SelectedCells[0].Column.Header.ToString();


            // ####if (headerName != "Name" && dataGrid.SelectedCells.Count > 0) Enum.TryParse((dataGrid.SelectedCells[0].Column.Header as TextBlock).Text, out duration);

            // #### Bar duration

            this.SelectedStockChanged(trend.Name, duration, true);

            StockAnalyzerForm.MainFrame.SetThemeFromIndicator("TRAILSTOP|" + viewModel.IndicatorName);
        }
    }
}
