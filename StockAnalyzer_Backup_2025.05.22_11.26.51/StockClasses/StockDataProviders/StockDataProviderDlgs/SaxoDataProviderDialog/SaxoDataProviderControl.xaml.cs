using System.Windows.Controls;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog
{
    /// <summary>
    /// Interaction logic for SaxoDataProviderControl.xaml
    /// </summary>
    public partial class SaxoDataProviderControl : UserControl
    {
        private SaxoDataProviderViewModel viewModel;

        public SaxoDataProviderControl()
        {
            InitializeComponent();
            this.DataContext = this.viewModel = new SaxoDataProviderViewModel();
        }

        public SaxoDataProviderViewModel ViewModel => this.viewModel;
    }
}
