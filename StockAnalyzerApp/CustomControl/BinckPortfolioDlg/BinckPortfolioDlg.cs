using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg
{
    public partial class BinckPortfolioDlg : Form
    {
        public BinckPortfolioDlg()
        {
            InitializeComponent();

            var control = this.elementHost1.Child as BinckPortfolioControl;
            control.DataContext = new ViewModel();
        }
    }
}
