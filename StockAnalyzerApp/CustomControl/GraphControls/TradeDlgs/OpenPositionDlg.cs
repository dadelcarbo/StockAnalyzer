using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.GraphControls.TradeDlgs
{
    public partial class OpenPositionDlg : Form
    {
        public OpenTradeViewModel TradeViewModel { get;}
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
