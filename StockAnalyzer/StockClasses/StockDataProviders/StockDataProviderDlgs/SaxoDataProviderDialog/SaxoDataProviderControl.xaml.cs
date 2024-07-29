using System.Windows.Controls;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog
{
    /// <summary>
    /// Interaction logic for SaxoDataProviderControl.xaml
    /// </summary>
    public partial class SaxoDataProviderControl : UserControl
    {
        public SaxoDataProviderControl(StockDictionary stockDico, string cfgFile, long? saxoId)
        {
            InitializeComponent();
            this.DataContext = new SaxoDataProviderViewModel(stockDico, cfgFile, saxoId);
        }
    }
}
