using StockAnalyzer.StockClasses;
using System.Windows.Controls;
using System.Windows.Input;

namespace StockAnalyzerApp.CustomControl.GroupViewDlg
{
    /// <summary>
    /// Interaction logic for GroupUserViewControl.xaml
    /// </summary>
    public partial class GroupUserViewControl : UserControl
    {
        public event StockAnalyzerForm.SelectedStockAndDurationChangedEventHandler SelectedStockChanged;

        public GroupUserViewControl()
        {
            InitializeComponent();
        }

        private void RadGridView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = this.grid.SelectedItem as GroupLineViewModel;
            if (viewModel == null) return;

            if (SelectedStockChanged != null)
                this.SelectedStockChanged(viewModel.StockSerie.StockName, StockBarDuration.Daily, true);

            StockAnalyzerForm.MainFrame.WindowState = System.Windows.Forms.FormWindowState.Normal;
        }
    }
}
