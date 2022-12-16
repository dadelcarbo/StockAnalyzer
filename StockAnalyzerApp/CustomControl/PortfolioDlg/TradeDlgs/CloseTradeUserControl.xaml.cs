using System.Windows;
using System.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs
{
    /// <summary>
    /// Interaction logic for CloseTradeUserControl.xaml
    /// </summary>
    public partial class CloseTradeUserControl : UserControl
    {
        public ClosePositionDlg ParentDlg { get; set; }
        public CloseTradeUserControl()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.ParentDlg.Ok();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.ParentDlg.Cancel();
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.ParentDlg.TradeViewModel.Refresh();
        }
    }
}
