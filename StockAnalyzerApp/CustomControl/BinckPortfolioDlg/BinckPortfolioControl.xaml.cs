using StockAnalyzer.StockClasses;
using System.Windows.Controls;
using System.Windows.Input;

namespace StockAnalyzerApp.CustomControl.BinckPortfolioDlg
{
    /// <summary>
    /// Interaction logic for BinckPortfolioControl.xaml
    /// </summary>
    public partial class BinckPortfolioControl : UserControl
    {
        public event StockAnalyzerForm.SelectedStockAndDurationChangedEventHandler SelectedStockChanged;
        public BinckPortfolioControl()
        {
            InitializeComponent();

            this.SelectedStockChanged += StockAnalyzerForm.MainFrame.OnSelectedStockAndDurationChanged;
        }

        private void positionGridView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = this.positionGridView.SelectedCells[0].Item as StockPositionViewModel;
            if (viewModel == null || !viewModel.IsValidName) return;

            if (SelectedStockChanged != null)
                this.SelectedStockChanged(viewModel.StockName, StockBarDuration.Daily, true);

            StockAnalyzerForm.MainFrame.WindowState = System.Windows.Forms.FormWindowState.Normal;
        }
    }
}
