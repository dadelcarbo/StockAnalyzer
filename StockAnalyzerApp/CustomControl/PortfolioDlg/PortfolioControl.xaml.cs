using StockAnalyzer.StockClasses;
using StockAnalyzer.StockPortfolio;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Data.PropertyGrid;
using Telerik.Windows.Controls.GridView;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg
{
    /// <summary>
    /// Interaction logic for PortfolioControl.xaml
    /// </summary>
    public partial class PortfolioControl : System.Windows.Controls.UserControl
    {
        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;
        public event StockAnalyzerForm.SelectedStockAndDurationAndThemeChangedEventHandler SelectedStockAndDurationChanged;

        private System.Windows.Forms.Form Form { get; }
        public PortfolioControl(System.Windows.Forms.Form form)
        {
            InitializeComponent();

            this.Form = form;
            this.SelectedStockChanged += StockAnalyzerForm.MainFrame.OnSelectedStockChanged;
            this.SelectedStockAndDurationChanged += StockAnalyzerForm.MainFrame.OnSelectedStockAndDurationAndThemeChanged;
            this.ordersGridView.AddHandler(GridViewCell.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownOnCell), true);
            this.mixedOpenedPositionGridView.AddHandler(GridViewCell.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownOnCell), true);
            this.openedOrdersGridView.AddHandler(GridViewCell.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownOnCell), true);
            this.closedPositionGridView.AddHandler(GridViewCell.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownOnCell), true);

            StockAnalyzerForm.MainFrame.GraphCloseControl.StopChanged += GraphCloseControl_StopChanged;
            Form.FormClosing += Form_FormClosing;
        }

        private void Form_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            StockAnalyzerForm.MainFrame.GraphCloseControl.StopChanged -= GraphCloseControl_StopChanged;
        }

        private void GraphCloseControl_StopChanged(float stopValue)
        {
            this.mixedOpenedPositionGridView.Rebind();
        }

        private void MouseDownOnCell(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var row = ((UIElement)e.OriginalSource).ParentOfType<GridViewRow>();
                if (row?.Item == null)
                    return;
                var itemName = row.Item.GetType().Name;
                switch (itemName)
                {
                    case "StockTradeOperation":
                        {
                            var item = row.Item as StockTradeOperation;
                            if (string.IsNullOrEmpty(item.StockName)) return;
                            SelectionChanged(item.StockName, item.ISIN);
                        }
                        break;
                    case "StockPositionBaseViewModel":
                        {
                            var item = row.Item as StockPositionBaseViewModel;
                            if (string.IsNullOrEmpty(item.StockName)) return;
                            SelectionChanged(item.StockName, item.ISIN, item.BarDuration, item.Theme);
                        }
                        break;
                    case "StockOpenedOrder":
                        {
                            var item = row.Item as StockOpenedOrder;
                            if (string.IsNullOrEmpty(item.StockName)) return;
                            SelectionChanged(item.StockName, item.ISIN, item.BarDuration, item.Theme);
                        }
                        break;
                    case "OrderViewModel":
                        {
                            var item = row.Item as OrderViewModel;
                            if (string.IsNullOrEmpty(item.StockName)) return;
                            SelectionChanged(item.StockName, null);
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

        private void FilterOperatorsLoading(object sender, FilterOperatorsLoadingEventArgs e)
        {
            var column = e.Column as GridViewBoundColumnBase;
            if (column == null)
                return;
            if (column.DataType == typeof(string))
            {
                e.DefaultOperator1 = Telerik.Windows.Data.FilterOperator.Contains;
                e.DefaultOperator2 = Telerik.Windows.Data.FilterOperator.Contains;
            }
        }
        private void SelectionChanged(string stockName, string isin, BarDuration? duration = null, string theme = null)
        {
            if (StockAnalyzerForm.MainFrame.CurrentStockSerie.StockName == stockName)
                return;

            var stockSerie = StockDictionary.GetSerie(stockName, isin);
            if (stockSerie != null && SelectedStockChanged != null)
            {
                this.Form.TopMost = true;
                StockAnalyzerForm.MainFrame.Activate();
                if (duration != null)
                {
                    this.SelectedStockAndDurationChanged(stockSerie.StockName, duration.Value, theme, true);
                }
                else
                {
                    this.SelectedStockChanged(stockSerie.StockName, true);
                }
                this.Form.TopMost = false;
            }
        }
        private void savePortfolioButton_Click(object sender, RoutedEventArgs e)
        {
            var cursor = this.Cursor;
            this.Cursor = Cursors.Wait;
            var viewModel = (ViewModel)this.DataContext;
            viewModel.Portfolio.Serialize();

            Task.Delay(500).Wait();
            this.Cursor = cursor;
        }

        static int index = 0;
        private void RadPropertyGrid_AutoGeneratingPropertyDefinition(object sender, AutoGeneratingPropertyDefinitionEventArgs e)
        {
            var attribute = e.PropertyDefinition.PropertyDescriptor.Attributes[typeof(PropertyAttribute)] as PropertyAttribute;
            if (attribute == null)
                e.Cancel = true;
            else
            {
                e.Cancel = false;
                if (!string.IsNullOrEmpty(attribute.Format))
                {
                    e.PropertyDefinition.Binding.StringFormat = attribute.Format;
                }
                if (!string.IsNullOrEmpty(attribute.Group))
                {
                    e.PropertyDefinition.GroupName = attribute.Group;
                }
                e.PropertyDefinition.OrderIndex = index++;
            }
        }

        private void reportButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as ViewModel;
            StockAnalyzerForm.MainFrame.GeneratePortfolioReportFile(viewModel.Portfolio);
        }
        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            var cursor = this.Cursor;
            this.Cursor = Cursors.Wait;

            var viewModel = this.DataContext as ViewModel;
            viewModel.Portfolio.Refresh();
            viewModel.Portfolio_Refreshed(viewModel.Portfolio);
            this.Cursor = cursor;
        }
    }
}
