using StockAnalyzer.StockLogging;
using System.Linq;
using System.Threading.Tasks;
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

            long orderId = 0;
            if (this.TradeViewModel.MarketOrder)
            {
                orderId = this.TradeViewModel.Portfolio.SaxoBuyOrder(this.TradeViewModel.StockSerie, StockAnalyzer.StockPortfolio.OrderType.Market, this.TradeViewModel.EntryQty, this.TradeViewModel.StopValue);
                if (orderId != 0)
                {
                    var position = this.TradeViewModel.Portfolio.Positions.OrderByDescending(p => p.EntryDate).FirstOrDefault();
                    if (position != null && position.StockName == this.TradeViewModel.StockSerie.StockName)
                    {
                        position.EntryComment = this.TradeViewModel.EntryComment;
                        position.Theme = this.TradeViewModel.Theme;
                        position.BarDuration = this.TradeViewModel.BarDuration;
                    }
                }
                else
                {
                    StockLog.Write("orderId=0");
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
            if (orderId == 0)
            {
                return;
            }
            var order = this.TradeViewModel.Portfolio.SaxoOrders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                order = this.TradeViewModel.Portfolio.SaxoOpenOrders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                order.BarDuration = this.TradeViewModel.BarDuration;
                order.Theme = this.TradeViewModel.Theme;
                order.EntryComment = this.TradeViewModel.EntryComment;
                order.Stop = this.TradeViewModel.StopValue;
            }

            this.TradeViewModel.Portfolio.Serialize();

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
