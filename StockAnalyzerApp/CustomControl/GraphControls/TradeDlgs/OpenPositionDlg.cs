using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            this.openTradeUserControl.Trade = tradeLogViewModel;
            this.TradeViewModel = tradeLogViewModel;
        }
    }
}
