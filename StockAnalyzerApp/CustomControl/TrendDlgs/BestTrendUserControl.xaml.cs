using System.Windows;
using System.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.TrendDlgs
{
    /// <summary>
    /// Interaction logic for BestTrendUserControl.xaml
    /// </summary>
    public partial class BestTrendUserControl : UserControl
    {
        public BestTrendUserControl()
        {
            InitializeComponent();
        }

        public BestTrendViewModel ViewModel => (BestTrendViewModel)this.DataContext;

        private void performBtn_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Perform();
        }
    }
}
