using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.BinckPortfolioDlg
{
    public partial class BinckPortfolioDlg : Form
    {
        public BinckPortfolioDlg()
        {
            InitializeComponent();

            var control = this.elementHost1.Child as BinckPortfolioControl;
            control.DataContext = new BinckPortfolioViewModel();
        }
    }
}
