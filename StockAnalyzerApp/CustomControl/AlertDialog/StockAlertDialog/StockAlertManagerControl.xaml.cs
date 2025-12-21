using StockAnalyzer.StockClasses;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;

namespace StockAnalyzerApp.CustomControl.AlertDialog.StockAlertDialog
{
    /// <summary>
    /// Interaction logic for StockAlertManager.xaml
    /// </summary>
    public partial class StockAlertManagerControl : UserControl
    {
        readonly StockAlertManagerViewModel ViewModel;
        readonly StockAlertManagerDlg ParentDlg;

        public event StockAnalyzerForm.SelectedStockAndDurationAndThemeChangedEventHandler SelectedStockAndDurationChanged; 

        public StockAlertManagerControl(StockAlertManagerDlg parent, StockAlertManagerViewModel viewModel)
        {
            this.ParentDlg = parent;
            this.ViewModel = viewModel;
            InitializeComponent();
            this.DataContext = viewModel;

            this.SelectedStockAndDurationChanged += StockAnalyzerForm.MainFrame.OnSelectedStockAndDurationAndThemeChanged;

            alertGrid.SortDescriptors.Clear();
            alertGrid.SortDescriptors.Add(new SortDescriptor
            {
                Member = this.speedColumn.DataMemberBinding.Path.Path,
                SortDirection = System.ComponentModel.ListSortDirection.Descending
            });
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ViewModel == null) return;

            var tabItem = this.tabControl.SelectedItem as TabItem;
            if (tabItem == null) return;

            this.ViewModel.AlertType = (AlertType)tabItem.Tag;
        }

        private void AlertDefGridView_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            var gridView = sender as RadGridView;
            var alertDef = gridView.SelectedItem as StockAlertDef;
            if (alertDef == null)
                return;

            this.ViewModel.Init(alertDef);
        }

        private void SelectedAlertGridView_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            var gridView = sender as RadGridView;
            var alertDef = gridView.SelectedItem as SelectedAlertDef;
            if (alertDef == null)
                return;

            alertDef.IsSelected = !alertDef.IsSelected;
        }

        private void AlertGridView_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            var gridView = sender as RadGridView;
            var alertValue = gridView.SelectedItem as StockAlertValue;
            if (alertValue == null)
                return;

            this.ParentDlg.TopMost = true;
            StockAnalyzerForm.MainFrame.Activate();

            this.SelectedStockAndDurationChanged(alertValue.StockSerie.StockName, alertValue.AlertDef.BarDuration, alertValue.AlertDef.Theme, true);
            this.ParentDlg.TopMost = false;
        }
    }
}
