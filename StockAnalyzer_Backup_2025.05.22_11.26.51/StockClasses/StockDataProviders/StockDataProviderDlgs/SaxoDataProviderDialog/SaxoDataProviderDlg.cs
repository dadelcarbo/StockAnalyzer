using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog
{
    public partial class SaxoDataProviderDlg : Form
    {
        public SaxoDataProviderDlg()
        {
            InitializeComponent();
        }

        public SaxoDataProviderViewModel ViewModel => this.saxoDataProviderControl1.ViewModel;
    }
}
