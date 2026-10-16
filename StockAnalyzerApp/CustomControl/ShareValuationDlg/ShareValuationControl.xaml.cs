using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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

namespace StockAnalyzerApp.CustomControl.ShareValuationDlg
{
    /// <summary>
    /// Interaction logic for ShareValuationControl.xaml
    /// </summary>
    public partial class ShareValuationControl : UserControl
    {
        public ShareValuationControl()
        {
            InitializeComponent();

            this.Loaded += ShareValuationControl_Loaded;
        }

        private void ShareValuationControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ShareValuationViewModel viewModel)
            {
                viewModel.Initialize(MainFrameViewModel.Instance.Instrument);
            }
        }
    }
}
