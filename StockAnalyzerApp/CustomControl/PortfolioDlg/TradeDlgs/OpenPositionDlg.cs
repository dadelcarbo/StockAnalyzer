using StockAnalyzer.StockClasses;
using System;
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
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        internal void Cancel()
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            Console.WriteLine("override void OnMouseEnter");
            this.TradeViewModel.RaiseOrdersChanged();
        }
        private void Child_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Console.WriteLine("override void Child_MouseEnter");
            this.TradeViewModel.RaiseOrdersChanged();
        }
    }
}
