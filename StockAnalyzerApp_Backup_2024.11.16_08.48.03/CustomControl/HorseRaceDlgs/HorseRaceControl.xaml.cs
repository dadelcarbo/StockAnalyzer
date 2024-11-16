using System;
using System.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace StockAnalyzerApp.CustomControl.HorseRaceDlgs
{
    /// <summary>
    /// Interaction logic for HorseRaceControl.xaml
    /// </summary>
    public partial class HorseRaceControl : UserControl
    {
        public HorseRaceViewModel ViewModel { get; set; }

        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;

        public HorseRaceControl()
        {
            InitializeComponent();

            this.ViewModel = new HorseRaceViewModel();
        }

        private void positionGridView_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            var viewModel = this.positionGridView?.SelectedItem as StockRankViewModel;
            if (viewModel == null) return;

            if (SelectedStockChanged != null)
            {
                this.SelectedStockChanged(viewModel.Name, true);
                StockAnalyzerForm.MainFrame.Activate();
            }
        }

        private void grid_FilterOperatorsLoading(object sender, FilterOperatorsLoadingEventArgs e)
        {
            var column = e.Column as Telerik.Windows.Controls.GridViewBoundColumnBase;
            if (column != null && column.DataType == typeof(string))
            {
                e.DefaultOperator1 = Telerik.Windows.Data.FilterOperator.Contains;
                e.DefaultOperator2 = Telerik.Windows.Data.FilterOperator.Contains;
            }
            else if (column != null && column.DataType == typeof(DateTime))
            {
                e.DefaultOperator1 = Telerik.Windows.Data.FilterOperator.IsGreaterThanOrEqualTo;
                e.DefaultOperator2 = Telerik.Windows.Data.FilterOperator.IsGreaterThanOrEqualTo;
            }
        }
    }
}
