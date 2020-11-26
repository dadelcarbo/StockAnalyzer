using System.Windows;
using System.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.GraphControls.TradeDlgs
{
    /// <summary>
    /// Interaction logic for OpenTradeUserControl.xaml
    /// </summary>
    public partial class OpenTradeUserControl : UserControl
    {
        public OpenPositionDlg ParentDlg { get; set; }
        public OpenTradeUserControl()
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
    }
}
