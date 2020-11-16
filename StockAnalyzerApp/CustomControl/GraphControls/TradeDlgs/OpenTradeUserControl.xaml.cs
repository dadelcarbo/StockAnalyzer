using StockAnalyzer.StockBinckPortfolio;
using StockAnalyzer.StockClasses;
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

namespace StockAnalyzerApp.CustomControl.GraphControls.TradeDlgs
{
    /// <summary>
    /// Interaction logic for OpenTradeUserControl.xaml
    /// </summary>
    public partial class OpenTradeUserControl : UserControl
    {
        private OpenTradeViewModel trade;
        public OpenTradeViewModel Trade
        {
            get { return trade; }
            set
            {
                if (value != trade)
                {
                    this.trade = value;
                    this.Resources["ViewModel"] = trade;
                }
            }
        }

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
