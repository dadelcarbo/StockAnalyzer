using System.Windows.Controls;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog
{
    /// <summary>
    /// Interaction logic for SaxoDataProviderControl.xaml
    /// </summary>
    public partial class SaxoDataProviderControl : UserControl
    {
        public SaxoDataProviderControl(StockDictionary stockDico, string cfgFile)
        {
            InitializeComponent();
            this.DataContext = new SaxoDataProviderViewModel(stockDico, cfgFile);
        }
    }
}
