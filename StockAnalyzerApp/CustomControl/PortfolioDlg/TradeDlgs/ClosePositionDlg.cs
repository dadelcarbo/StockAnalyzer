using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs
{
    public partial class ClosePositionDlg : Form
    {
        public CloseTradeViewModel TradeViewModel { get; }
        CloseTradeUserControl closeTradeUserControl;
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
            if (!this.TradeViewModel.Portfolio.SaxoLogin())
            {
                return;
            }

            string orderId = null; // this.TradeViewModel.Portfolio.SaxoUpdateStopOrder(this.TradeViewModel.Position, this.TradeViewModel.ExitValue);
            if (string.IsNullOrEmpty(this.TradeViewModel.Position?.TrailStopId))
            {
                orderId = this.TradeViewModel.Portfolio.SaxoSellOrder(this.TradeViewModel.StockSerie, StockAnalyzer.StockPortfolio.OrderType.Market, this.TradeViewModel.ExitQty);
            }
            if (string.IsNullOrEmpty(orderId))
            {
                MessageBox.Show("Order non executed !", "Saxo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
