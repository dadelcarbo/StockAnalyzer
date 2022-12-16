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

        private void RadAutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                (this.DataContext as SaxoDataProviderViewModel).UnderlyingChanged((Entry)e.AddedItems[0]);
            }
        }
    }
}
