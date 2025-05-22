using StockAnalyzer.StockPortfolio;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs
{
    public partial class ClosePositionDlg : Form
    {
        public CloseTradeViewModel TradeViewModel { get; }
        readonly CloseTradeUserControl closeTradeUserControl;
        public ClosePositionDlg(CloseTradeViewModel tradeLogViewModel)
        {
            InitializeComponent();

            this.closeTradeUserControl = this.elementHost1.Child as CloseTradeUserControl;
            this.closeTradeUserControl.DataContext = tradeLogViewModel;
            this.closeTradeUserControl.ParentDlg = this;
            this.TradeViewModel = tradeLogViewModel;
        }

        internal void Cancel()
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        internal void Ok()
        {
            string orderId;
            if (this.TradeViewModel.MarketOrder)
            {
                orderId = this.TradeViewModel.Portfolio.SaxoClosePosition(this.TradeViewModel.Position, OrderType.Market);
            }
            else
            {
                orderId = this.TradeViewModel.Portfolio.SaxoClosePosition(this.TradeViewModel.Position, this.TradeViewModel.LimitOrder ? OrderType.Limit : OrderType.Threshold, this.TradeViewModel.ExitValue);
            }
            if (string.IsNullOrEmpty(orderId))
            {
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void Child_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.TradeViewModel.RaiseOrdersChanged();
        }
    }
}
