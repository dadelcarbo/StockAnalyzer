using System.Windows.Controls;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs
{
    /// <summary>
    /// Interaction logic for InvestingConfigControl.xaml
    /// </summary>
    public partial class InvestingConfigControl : UserControl
    {
        public InvestingConfigViewModel ViewModel { get; set; }

        public InvestingConfigControl()
        {
            InitializeComponent();

            this.ViewModel = new InvestingConfigViewModel();
            this.DataContext = this.ViewModel;
        }
    }
}
