using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StockAnalyzerApp.CustomControl.ExpectedValueDlg
{
    /// <summary>
    /// Interaction logic for ExpectedValueControl.xaml
    /// </summary>
    public partial class ExpectedValueControl : UserControl
    {
        readonly ExpectedValueViewModel viewModel;
        public ExpectedValueControl()
        {
            InitializeComponent();
            this.DataContext = this.viewModel = this.Resources["ViewModel"] as ExpectedValueViewModel;
        }

        private void CalculateBtn_OnClick(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            this.viewModel.Calculate();

            this.Cursor = Cursors.Arrow;
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var datagrid = sender as DataGrid;
            if (datagrid == null)
                return;
            if (datagrid.Columns.Any(c => c.Header.ToString() == e.PropertyName))
            {
                e.Cancel = true;
            }
        }
    }
}
