using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs.TradeManager
{
    public partial class TradeManagerDlg : Form
    {
        public TradeManagerDlg(TradeManagerViewModel viewModel)
        {
            InitializeComponent();

            this.tradeManagerControl.ViewModel = viewModel;

            this.tradeManagerControl.ViewModel.Dispatcher = this.tradeManagerControl.Dispatcher;

        }

    }
}
