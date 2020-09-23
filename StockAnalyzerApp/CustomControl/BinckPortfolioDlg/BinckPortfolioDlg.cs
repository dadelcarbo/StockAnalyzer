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
