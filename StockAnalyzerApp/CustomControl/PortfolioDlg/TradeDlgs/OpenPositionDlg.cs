using System.Windows.Forms;
using Tweetinvi.Core.Extensions;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs
{
    public partial class OpenPositionDlg : Form
    {
        public OpenTradeViewModel TradeViewModel { get; }
        OpenTradeUserControl openTradeUserControl;
        public OpenPositionDlg(OpenTradeViewModel tradeLogViewModel)
        {
            InitializeComponent();

            this.openTradeUserControl = this.elementHost1.Child as OpenTradeUserControl;
            this.openTradeUserControl.DataContext = tradeLogViewModel;
            this.openTradeUserControl.ParentDlg = this;
            this.TradeViewModel = tradeLogViewModel;
        }

        internal void Ok()
        {
            if (!this.TradeViewModel.Portfolio.SaxoLogin())
            {
                return;
            }

            string orderId = null;
            if (this.TradeViewModel.MarketOrder)
            {
                orderId = this.TradeViewModel.Portfolio.SaxoBuyOrder(this.TradeViewModel.StockSerie, StockAnalyzer.StockPortfolio.OrderType.Market, this.TradeViewModel.EntryQty, this.TradeViewModel.StopValue);
            }
            else if (this.TradeViewModel.LimitOrder)
            {
                orderId = this.TradeViewModel.Portfolio.SaxoBuyOrder(this.TradeViewModel.StockSerie, StockAnalyzer.StockPortfolio.OrderType.Limit, this.TradeViewModel.EntryQty, this.TradeViewModel.StopValue, this.TradeViewModel.EntryValue);
            }
            else if (this.TradeViewModel.ThresholdOrder)
            {
                orderId = this.TradeViewModel.Portfolio.SaxoBuyOrder(this.TradeViewModel.StockSerie, StockAnalyzer.StockPortfolio.OrderType.Threshold, this.TradeViewModel.EntryQty, this.TradeViewModel.StopValue, this.TradeViewModel.EntryValue);
            }
            if (string.IsNullOrEmpty(orderId))
            {
                MessageBox.Show("Order non executed !", "Saxo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        internal void Cancel()
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
