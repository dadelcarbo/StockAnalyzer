using System.Windows.Forms;

namespace StockAnalyzer.StockData.DataProviders.SaxoTurbos.ConfigDialog
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
