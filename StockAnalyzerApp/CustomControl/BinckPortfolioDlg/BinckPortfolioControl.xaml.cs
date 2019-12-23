using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
