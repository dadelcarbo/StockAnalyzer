using StockAnalyzer.StockClasses.StockDataProviders;
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

namespace StockAnalyzerApp.CustomControl.SplitDlg
{
    /// <summary>
    /// Interaction logic for StockSplitControl.xaml
    /// </summary>
    public partial class StockSplitControl : UserControl
    {
        public StockSplitControl()
        {
            InitializeComponent();
        }

        public DateTime Date { get; set; } = DateTime.Today;
        public int Before { get; set; } = 1;
        public int After { get; set; } = 1;

        private void ApplySplitButton_Click(object sender, RoutedEventArgs e)
        {
            var dataProvider = StockDataProviderBase.GetDataProvider(StockAnalyzerForm.MainFrame.CurrentStockSerie.DataProvider);
            if (dataProvider == null) { return; }

            dataProvider.ApplySplit(StockAnalyzerForm.MainFrame.CurrentStockSerie, this.Date, (float)Before / (float)After);
        }

        private void ApplyTrimButton_Click(object sender, RoutedEventArgs e)
        {
            var dataProvider = StockDataProviderBase.GetDataProvider(StockDataProvider.BoursoIntraday);
            if (dataProvider == null) { return; }

            dataProvider.ApplyTrim(StockAnalyzerForm.MainFrame.CurrentStockSerie, this.Date);
        }
    }
}