using StockAnalyzer.StockPortfolio;
using StockAnalyzer.StockClasses;
using System;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using System.Threading.Tasks;

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
            this.operationGridView.AddHandler(GridViewCell.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MouseDownOnCell), true);
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
                switch (row.Item.GetType().Name)
                {
                    case "StockTradeOperation":
                        {
                            var item = row.Item as StockTradeOperation;
                            SelectionChanged(item.StockName, item.ISIN);
                        }
                        break;
                    case "StockPositionBaseViewModel":
                        {
                            var item = row.Item as StockPositionBaseViewModel;
                            SelectionChanged(item.StockName, item.ISIN, item.BarDuration, item.Theme);
                            //item.PropertyChanged += Position_PropertyChanged;
                        }
                        break;
                    case "StockOpenedOrder":
                        {
                            var item = row.Item as StockOpenedOrder;
                            SelectionChanged(item.StockName, item.ISIN, item.BarDuration, item.Theme);
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

        //private void Position_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    switch (e.PropertyName)
        //    {
        //        case "Stop":
        //        case "TrailStop":
        //            StockAnalyzerForm.MainFrame.RefreshGraphCloseControl();
        //            break;
        //        default:
        //            break;
        //    }
        //}

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
        private void SelectionChanged(string stockName, string isin, StockBarDuration duration = null, string theme = null)
        {
            if (StockAnalyzerForm.MainFrame.CurrentStockSerie.StockName == stockName)
                return;

            var stockSerie = StockDictionary.GetSerie(stockName, isin);
            if (stockSerie != null && SelectedStockChanged != null)
            {
                StockAnalyzerForm.MainFrame.Activate();
                if (!string.IsNullOrEmpty(theme) && duration != null)
                {
                    this.SelectedStockAndDurationChanged(stockSerie.StockName, duration, theme, true);
                }
                else
                {
                    this.SelectedStockChanged(stockSerie.StockName, true);
                }
                this.Form.TopMost = true;
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
        private void RadPropertyGrid_AutoGeneratingPropertyDefinition(object sender, Telerik.Windows.Controls.Data.PropertyGrid.AutoGeneratingPropertyDefinitionEventArgs e)
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
