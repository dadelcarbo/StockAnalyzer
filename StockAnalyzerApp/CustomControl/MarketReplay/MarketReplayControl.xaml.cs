using System.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.MarketReplay
{
    /// <summary>
    /// Interaction logic for MarketReplayControl.xaml
    /// </summary>
    public partial class MarketReplayControl : UserControl
    {
        public MarketReplayViewModel ViewModel { get; set; }
        public MarketReplayControl()
        {
            InitializeComponent();
        }
    }
}
