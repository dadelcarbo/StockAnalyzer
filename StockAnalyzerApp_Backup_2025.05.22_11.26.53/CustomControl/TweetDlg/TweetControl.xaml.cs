using System.Windows;
using System.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.TweetDlg
{
    /// <summary>
    /// Interaction logic for TweetControl.xaml
    /// </summary>
    public partial class TweetControl : UserControl
    {
        public TweetViewModel ViewModel { get; }
        public TweetControl()
        {
            InitializeComponent();
            this.ViewModel = (TweetViewModel)this.Resources["ViewModel"];
            this.DataContext = this.ViewModel;
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.SendTweetAsync();
        }
    }
}
