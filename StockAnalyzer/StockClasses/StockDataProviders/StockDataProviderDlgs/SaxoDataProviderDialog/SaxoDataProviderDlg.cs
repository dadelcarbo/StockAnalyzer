using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog
{
    public partial class SaxoDataProviderDlg : Form
    {
        public SaxoDataProviderDlg(StockDictionary stockDico, string cfgFile, long? saxoId)
        {
            InitializeComponent(stockDico, cfgFile, saxoId);
        }
    }
}
