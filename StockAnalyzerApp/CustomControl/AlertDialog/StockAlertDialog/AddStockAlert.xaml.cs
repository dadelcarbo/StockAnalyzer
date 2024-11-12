using StockAnalyzer.StockClasses;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.AlertDialog.StockAlertDialog
{
    /// <summary>
    /// Interaction logic for AddStockAlert.xaml
    /// </summary>
    public partial class AddStockAlert : UserControl
    {
        readonly AddStockAlertViewModel ViewModel;
        readonly AddStockAlertDlg ParentDlg;

        public event StockAnalyzerForm.SelectedStockAndDurationAndThemeChangedEventHandler SelectedStockAndDurationChanged;

        public AddStockAlert(AddStockAlertDlg parent, AddStockAlertViewModel viewModel)
        {
            this.ParentDlg = parent;
            this.ViewModel = viewModel;
            InitializeComponent();
            this.DataContext = viewModel;

            this.SelectedStockAndDurationChanged += StockAnalyzerForm.MainFrame.OnSelectedStockAndDurationAndThemeChanged;
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
