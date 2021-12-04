using StockAnalyzer.StockPortfolio;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzerSettings.Properties;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using System.Linq;
using System.Diagnostics;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg
{
    /// <summary>
    /// Interaction logic for BinckPortfolioControl.xaml
    /// </summary>
    public partial class BinckPortfolioControl : System.Windows.Controls.UserControl
    {
        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;
        public event StockAnalyzerForm.SelectedStockAndDurationAndThemeChangedEventHandler SelectedStockAndDurationChanged;

        private System.Windows.Forms.Form Form { get; }
        public BinckPortfolioControl(System.Windows.Forms.Form form)
        {
            InitializeComponent();

            this.Form = form;
            this.SelectedStockChanged += StockAnalyzerForm.MainFrame.OnSelectedStockChanged;
            this.SelectedStockAndDurationChanged += StockAnalyzerForm.MainFrame.OnSelectedStockAndDurationAndThemeChanged;
            this.operationGridView.AddHandler(GridViewCell.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownOnCell), true);
            this.openedPositionGridView.AddHandler(GridViewCell.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownOnCell), true);
            this.closedPositionGridView.AddHandler(GridViewCell.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownOnCell), true);
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
                            SelectionChanged(item.StockName, item.BarDuration, item.Theme);
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
        private void SelectionChanged(string stockName, StockBarDuration duration = null, string theme = null)
        {
            var mapping = StockPortfolio.GetMapping(stockName);
            if (mapping != null)
                stockName = mapping.StockName;
            if (StockAnalyzerForm.MainFrame.CurrentStockSerie.StockName == stockName)
                return;
            if (StockDictionary.Instance.ContainsKey(stockName) && SelectedStockChanged != null)
            {
                StockAnalyzerForm.MainFrame.Activate();
                if (!string.IsNullOrEmpty(theme) && duration != null)
                {
                    this.SelectedStockAndDurationChanged(stockName, duration, theme, true);
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
            var viewModel = (ViewModel)this.DataContext;
            viewModel.Portfolio.Serialize(Path.Combine(Settings.Default.RootFolder, PortfolioDataProvider.PORTFOLIO_FOLDER));
        }

        private void RadPropertyGrid_AutoGeneratingPropertyDefinition(object sender, Telerik.Windows.Controls.Data.PropertyGrid.AutoGeneratingPropertyDefinitionEventArgs e)
        {
            var attribute = e.PropertyDefinition.PropertyDescriptor.Attributes[typeof(PropertyAttribute)];
            if (attribute == null)
                e.Cancel = true;
            else
                e.Cancel = false;
        }
        const string AlertLineTemplate = "<a class=\"tooltip\">%MSG%<span><img src=\"%IMG%\"></a>";

        private void reportButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as ViewModel;
            StockAnalyzerForm.MainFrame.GeneratePortfolioReport(viewModel.Portfolio);
        }
    }
}
