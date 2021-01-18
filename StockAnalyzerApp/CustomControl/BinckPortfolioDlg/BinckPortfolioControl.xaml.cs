using StockAnalyzer.StockBinckPortfolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzerSettings.Properties;
using System;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Data;
using System.Reflection;

namespace StockAnalyzerApp.CustomControl.BinckPortfolioDlg
{
    /// <summary>
    /// Interaction logic for BinckPortfolioControl.xaml
    /// </summary>
    public partial class BinckPortfolioControl : System.Windows.Controls.UserControl
    {
        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;
        public event StockAnalyzerForm.SelectedStockAndDurationChangedEventHandler SelectedStockAndDurationChanged;

        private System.Windows.Forms.Form Form { get; }
        public BinckPortfolioControl(System.Windows.Forms.Form form)
        {
            InitializeComponent();

            this.Form = form;
            this.SelectedStockChanged += StockAnalyzerForm.MainFrame.OnSelectedStockChanged;
            this.SelectedStockAndDurationChanged += StockAnalyzerForm.MainFrame.OnSelectedStockAndDurationChanged;
            this.operationGridView.AddHandler(GridViewCell.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownOnCell), true);
            this.positionGridView.AddHandler(GridViewCell.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownOnCell), true);
        }

        private void MouseDownOnCell(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var row = ((UIElement)e.OriginalSource).ParentOfType<GridViewRow>();
                if (row?.Item == null)
                    return;
                switch (row.Item.GetType().Name)
                {
                    case "StockTradeOperation":
                        {
                            StockTradeOperation item = row.Item as StockTradeOperation;
                            SelectionChanged(item.StockName);
                        }
                        break;
                    case "StockPositionViewModel":
                        {
                            StockPositionViewModel item = row.Item as StockPositionViewModel;
                            SelectionChanged(item.StockName);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        private void FilterOperatorsLoading(object sender, Telerik.Windows.Controls.GridView.FilterOperatorsLoadingEventArgs e)
        {
            var column = e.Column as Telerik.Windows.Controls.GridViewBoundColumnBase;
            if (column == null)
                return;
            if (column.DataType == typeof(string))
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
        private void tradeLogGridView_AutoGeneratingColumn(object sender, Telerik.Windows.Controls.GridViewAutoGeneratingColumnEventArgs e)
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
            var viewModel = this.operationGridView.SelectedItem as StockTradeOperation;

            var stockName = viewModel.StockName;
            var mapping = StockPortfolio.GetMapping(viewModel.StockName);
            if (mapping != null)
            {
                stockName = mapping.StockName;
            }
            if (StockDictionary.Instance.ContainsKey(stockName) && SelectedStockChanged != null)
            {
                this.SelectedStockChanged(viewModel.StockName, true);
                StockAnalyzerForm.MainFrame.Activate();
            }
        }
        private void SelectionChanged(string stockName, StockBarDuration duration = null, string indicator = null)
        {
            var mapping = StockPortfolio.GetMapping(stockName);
            if (mapping != null)
                stockName = mapping.StockName;
            if (StockAnalyzerForm.MainFrame.CurrentStockSerie.StockName == stockName)
                return;
            if (StockDictionary.Instance.ContainsKey(stockName) && SelectedStockChanged != null)
            {
                StockAnalyzerForm.MainFrame.Activate();
                if (!string.IsNullOrEmpty(indicator) && duration != null)
                {
                    this.SelectedStockAndDurationChanged(stockName, duration, true);
                    StockAnalyzerForm.MainFrame.SetThemeFromIndicator(indicator);
                }
                else
                {
                    this.SelectedStockChanged(stockName, true);
                }
                this.Form.TopMost = true;
                this.Form.TopMost = false;
            }
        }
        private void savePortfolioButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (BinckPortfolioViewModel)this.DataContext;
            viewModel.Portfolio.Serialize(Path.Combine(Settings.Default.RootFolder, BinckPortfolioDataProvider.PORTFOLIO_FOLDER));
        }

        private void RadPropertyGrid_AutoGeneratingPropertyDefinition(object sender, Telerik.Windows.Controls.Data.PropertyGrid.AutoGeneratingPropertyDefinitionEventArgs e)
        {
            var viewModel = (BinckPortfolioViewModel)this.DataContext;

            var attribute = e.PropertyDefinition.PropertyDescriptor.Attributes[typeof(PropertyAttribute)];
            if (attribute == null)
                e.Cancel = true;
            else
                e.Cancel = false;
        }
    }
}
