using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
