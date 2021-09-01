using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs
{
    public partial class ClosePositionDlg : Form
    {
        public CloseTradeViewModel TradeViewModel { get;}
        CloseTradeUserControl closeTradeUserControl;
        public ClosePositionDlg(CloseTradeViewModel tradeLogViewModel)
        {
            InitializeComponent();

            this.closeTradeUserControl = this.elementHost1.Child as CloseTradeUserControl;
            this.closeTradeUserControl.DataContext = tradeLogViewModel;
            this.closeTradeUserControl.ParentDlg = this;
            this.TradeViewModel = tradeLogViewModel;
        }

        internal void Ok()
        {
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
