using System.Windows.Forms;

namespace StockAnalyzer.StockPortfolio.NameMappingDlg
{
    public partial class NameMappingDlg : Form
    {
        NameMappingUserControl UserControl;
        public NameMappingDlg()
        {
            InitializeComponent();

            UserControl  = this.elementHost1.Child as NameMappingUserControl;
        }
    }
}
