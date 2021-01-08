using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.PalmaresDlg
{
    /// <summary>
    /// Interaction logic for PalmaresControl.xaml
    /// </summary>
    public partial class PalmaresControl : UserControl
    {
        private System.Windows.Forms.Form Form { get; }
        public event StockAnalyzerForm.SelectedStockAndDurationChangedEventHandler SelectedStockChanged;

        public PalmaresViewModel ViewModel;
        public PalmaresControl(System.Windows.Forms.Form form)
        {
            InitializeComponent();
            this.Form = form;
            this.DataContext = this.ViewModel = this.Resources["ViewModel"] as PalmaresViewModel;
        }

        private void CalculateBtn_OnClick(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            this.ViewModel.Calculate();

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

        private void RadGridView_AutoGeneratingColumn(object sender, Telerik.Windows.Controls.GridViewAutoGeneratingColumnEventArgs e)
        {
            var columnName = e.Column.Header.ToString();
            var col = e.Column as GridViewDataColumn;
            switch (columnName)
            {
                case "Variation":
                case "Stop":
                    col.DataFormatString = "P2";
                    break;
                case "Indicator1":
                    if (!string.IsNullOrEmpty(ViewModel.Indicator1))
                    {
                        col.Header = ViewModel.Indicator1.Split('(')[0];
                    }
                    break;
                case "Indicator2":
                    if (!string.IsNullOrEmpty(ViewModel.Indicator1))
                    {
                        col.Header = ViewModel.Indicator2.Split('(')[0];
                    }
                    break;
                case "Indicator3":
                    if (!string.IsNullOrEmpty(ViewModel.Indicator1))
                    {
                        col.Header = ViewModel.Indicator3.Split('(')[0];
                    }
                    break;
            }
        }
        private void RadGridView_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            // Open on the alert stock
            var line = ((RadGridView)sender).SelectedItem as PalmaresLine;

            if (line == null) return;

            if (SelectedStockChanged != null)
            {
                StockAnalyzerForm.MainFrame.Activate();
                this.SelectedStockChanged(line.Name, ViewModel.BarDuration, true);
                this.Form.TopMost = true;
                this.Form.TopMost = false;
            }
        }
    }
}
