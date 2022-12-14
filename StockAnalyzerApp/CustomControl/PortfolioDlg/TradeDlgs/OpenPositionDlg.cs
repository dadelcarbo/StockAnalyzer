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
            if (string.IsNullOrEmpty(this.TradeViewModel.Portfolio.SaxoAccountId))
            {
                MessageBox.Show("Portfolio is not conected to SAXO");
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            if (this.TradeViewModel.MarketOrder)
            {
            }
            else if (this.TradeViewModel.LimitOrder)
            {
            }
            else if (this.TradeViewModel.ThresholdOrder)
            {
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
