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
        public AddStockAlert(AddStockAlertDlg parent, AddStockAlertViewModel viewModel)
        {
            this.ParentDlg = parent;
            this.ViewModel = viewModel;
            InitializeComponent();
            this.DataContext = viewModel;
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ViewModel == null) return;

            var tabItem = this.tabControl.SelectedItem as TabItem;
            if (tabItem == null) return;

            this.ViewModel.AlertType = (AlertType)tabItem.Tag;
        }

        private void RadGridView_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            var gridView = sender as RadGridView;
            var alertDef = gridView.SelectedItem as StockAlertDef;

            this.ViewModel.Init(alertDef);
        }
    }
}
