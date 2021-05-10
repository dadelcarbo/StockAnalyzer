using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.TweetDlg
{
    public partial class TweetDlg2 : Form
    {
        public TweetViewModel ViewModel => (this.elementHost1.Child as TweetControl)?.ViewModel;
        public TweetDlg2()
        {
            InitializeComponent();

            this.ViewModel.TweetSent += OnTweetSent;
        }

        void OnTweetSent()
        {
            this.Close();
        }
    }
}
