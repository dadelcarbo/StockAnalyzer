using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg
{
    public partial class PortfolioDlg : Form
    {
        public PortfolioDlg()
        {
            InitializeComponent();

            var control = this.elementHost1.Child as PortfolioControl;
            control.DataContext = new ViewModel();
        }
    }
}
