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

namespace StockAnalyzerApp.CustomControl.ExpectedValueDlg
{
    /// <summary>
    /// Interaction logic for ExpectedValueControl.xaml
    /// </summary>
    public partial class ExpectedValueControl : UserControl
    {
        ExpectedValueViewModel viewModel = new ExpectedValueViewModel("TOPEMA(0,80,1)", "Bullish", "Bearish");
        public ExpectedValueControl()
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        private void CalculateBtn_OnClick(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            this.viewModel.Calculate();

            this.Cursor = Cursors.Arrow;

        }
    }
}
