using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs
{
    /// <summary>
    /// Interaction logic for OpenTradeUserControl.xaml
    /// </summary>
    public partial class OpenTradeUserControl : UserControl
    {
        public OpenPositionDlg ParentDlg { get; set; }
        public OpenTradeUserControl()
        {
            InitializeComponent();
        }

        private OpenTradeViewModel TradeViewModel => this.ParentDlg.TradeViewModel;

        private void okButton_Click(object sender, RoutedEventArgs e)
        {

            if (!this.TradeViewModel.Portfolio.SaxoLogin())
            {
                return;
            }

            string orderId = null;
            if (this.TradeViewModel.MarketOrder)
            {
                orderId = this.TradeViewModel.Portfolio.SaxoBuyOrder(this.TradeViewModel.StockSerie, StockAnalyzer.StockPortfolio.OrderType.Market, this.TradeViewModel.EntryQty, this.TradeViewModel.StopValue);
                if (orderId != null)
                {
                    var position = this.TradeViewModel.Portfolio.OpenedPositions.OrderByDescending(p => p.EntryDate).FirstOrDefault();
                    if (position != null && position.StockName == this.TradeViewModel.StockSerie.StockName)
                    {
                        position.EntryComment = this.TradeViewModel.EntryComment;
                        position.Theme = this.TradeViewModel.Theme;
                        position.BarDuration = this.TradeViewModel.BarDuration.Duration;
                        this.TradeViewModel.Portfolio.Serialize();
                    }
                }
            }
            else if (this.TradeViewModel.LimitOrder)
            {
                if (this.TradeViewModel.EntryValue > this.TradeViewModel.StockSerie.LastValue.CLOSE)
                {
                    MessageBox.Show("Order on the wrong side of the market !", "Saxo Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                orderId = this.TradeViewModel.Portfolio.SaxoBuyOrder(this.TradeViewModel.StockSerie, StockAnalyzer.StockPortfolio.OrderType.Limit, this.TradeViewModel.EntryQty, this.TradeViewModel.StopValue, this.TradeViewModel.EntryValue);
            }
            else if (this.TradeViewModel.ThresholdOrder)
            {
                if (this.TradeViewModel.EntryValue < this.TradeViewModel.StockSerie.LastValue.CLOSE)
                {
                    MessageBox.Show("Order on the wrong side of the market !", "Saxo Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                orderId = this.TradeViewModel.Portfolio.SaxoBuyOrder(this.TradeViewModel.StockSerie, StockAnalyzer.StockPortfolio.OrderType.Threshold, this.TradeViewModel.EntryQty, this.TradeViewModel.StopValue, this.TradeViewModel.EntryValue);
            }
            if (string.IsNullOrEmpty(orderId))
            {
                return;
            }
            var openedOrder = this.TradeViewModel.Portfolio.OpenOrders.FirstOrDefault(o => o.Id == long.Parse(orderId));
            if (openedOrder != null)
            {
                openedOrder.BarDuration = this.TradeViewModel.BarDuration.Duration;
                openedOrder.Theme = this.TradeViewModel.Theme;
                openedOrder.EntryComment = this.TradeViewModel.EntryComment;
                this.TradeViewModel.Portfolio.Serialize();
            }

            this.ParentDlg.Ok();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.ParentDlg.Cancel();
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.TradeViewModel.Refresh();
        }
    }
}
