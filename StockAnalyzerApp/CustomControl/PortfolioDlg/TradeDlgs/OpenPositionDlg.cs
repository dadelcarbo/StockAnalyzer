using StockAnalyzer.StockClasses;
using System.Linq;
using System.Windows.Forms;

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
                if (orderId != null)
                {
                    var position = this.TradeViewModel.Portfolio.OpenedPositions.OrderByDescending(p => p.EntryDate).FirstOrDefault();
                    if (position != null && position.StockName == this.TradeViewModel.StockSerie.StockName)
                    {
                        position.EntryComment = this.TradeViewModel.EntryComment;
                        position.Theme = this.TradeViewModel.Theme;
                        position.BarDuration = this.TradeViewModel.BarDuration;
                        this.TradeViewModel.Portfolio.Serialize();
                    }
                }
            }
            else if (this.TradeViewModel.LimitOrder)
            {
                if (this.TradeViewModel.EntryValue > this.TradeViewModel.StockSerie.LastValue.CLOSE)
                {
                    MessageBox.Show("Order on the wrong side of the market !", "Saxo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                orderId = this.TradeViewModel.Portfolio.SaxoBuyOrder(this.TradeViewModel.StockSerie, StockAnalyzer.StockPortfolio.OrderType.Limit, this.TradeViewModel.EntryQty, this.TradeViewModel.StopValue, this.TradeViewModel.EntryValue);
            }
            else if (this.TradeViewModel.ThresholdOrder)
            {
                if (this.TradeViewModel.EntryValue < this.TradeViewModel.StockSerie.LastValue.CLOSE)
                {
                    MessageBox.Show("Order on the wrong side of the market !", "Saxo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
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
