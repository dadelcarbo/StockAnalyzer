using StockAnalyzer.StockBinckPortfolio;
using StockAnalyzer.StockClasses;
using System.Windows.Controls;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.BinckPortfolioDlg
{
    /// <summary>
    /// Interaction logic for BinckPortfolioControl.xaml
    /// </summary>
    public partial class BinckPortfolioControl : System.Windows.Controls.UserControl
    {
        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;
        private System.Windows.Forms.Form Form { get; }
        public BinckPortfolioControl(System.Windows.Forms.Form form)
        {
            InitializeComponent();

            this.Form = form;
            this.SelectedStockChanged += StockAnalyzerForm.MainFrame.OnSelectedStockChanged;
        }

        private void FilterOperatorsLoading(object sender, Telerik.Windows.Controls.GridView.FilterOperatorsLoadingEventArgs e)
        {
            var column = e.Column as Telerik.Windows.Controls.GridViewBoundColumnBase;
            if (column != null && column.DataType == typeof(string))
            {
                e.DefaultOperator1 = Telerik.Windows.Data.FilterOperator.Contains;
                e.DefaultOperator2 = Telerik.Windows.Data.FilterOperator.Contains;
            }
        }

        private void OperationGridView_AutoGeneratingColumn(object sender, Telerik.Windows.Controls.GridViewAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "NameMapping")
            {
                e.Cancel = true;
            }
        }

        private void positionGridView_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            var viewModel = this.positionGridView.SelectedItem as StockPositionViewModel;
            if (viewModel == null || !viewModel.IsValidName) return;

            if (SelectedStockChanged != null)
            {
                StockAnalyzerForm.MainFrame.Activate();
                this.SelectedStockChanged(viewModel.StockName, true);
            }

            this.Form.TopMost = true;
            this.Form.TopMost = false;
        }

        private void operationGridView_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            if (this.operationGridView.SelectedItem == null)
                return;
            var viewModel = this.operationGridView.SelectedItem as StockOperation;

            var stockName = viewModel.StockName;
            var mapping = StockPortfolio.GetMapping(viewModel.StockName);
            if (mapping != null)
            {
                stockName = mapping.StockName;
            }
            if (StockDictionary.StockDictionarySingleton.ContainsKey(stockName) && SelectedStockChanged != null)
            {
                this.SelectedStockChanged(viewModel.StockName, true);
                StockAnalyzerForm.MainFrame.Activate();
            }
        }
    }
}
