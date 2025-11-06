using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.SplitDlg
{
    /// <summary>
    /// Interaction logic for StockSplitControl.xaml
    /// </summary>
    public partial class StockSplitControl : UserControl
    {
        StockSplitDlg parentDlg;
        public StockSplitControl(StockSplitDlg stockSplitDlg)
        {
            this.parentDlg = stockSplitDlg;
            InitializeComponent();
        }

        public DateTime SplitDate { get; set; } = DateTime.Today.AddDays(-7);
        public DateTime TrimBeforeDate { get; set; } = DateTime.Today.AddDays(-7);
        public DateTime TrimAfterDate { get; set; } = new DateTime(DateTime.Today.Year, 1, 1);
        public float Before { get; set; } = 1f;
        public float After { get; set; } = 1f;

        private void ApplySplitButton_Click(object sender, RoutedEventArgs e)
        {
            var dataProvider = StockDataProviderBase.GetDataProvider(StockAnalyzerForm.MainFrame.CurrentStockSerie.DataProvider);
            if (dataProvider == null) { return; }

            dataProvider.AddSplit(StockAnalyzerForm.MainFrame.CurrentStockSerie, this.SplitDate, Before, After);

            StockAnalyzerForm.MainFrame.ApplyTheme();

            this.parentDlg.Close();
        }

        public bool AllGroupSeries { get; set; }

        private void ApplyTrimButton_Click(object sender, RoutedEventArgs e)
        {
            if (AllGroupSeries)
            {
                foreach (var stockSerie in StockDictionary.Instance.Values.Where(s => s.StockGroup == StockAnalyzerForm.MainFrame.CurrentStockSerie.StockGroup))
                {
                    var dataProvider = StockDataProviderBase.GetDataProvider(stockSerie.DataProvider);
                    if (dataProvider == null) { continue; }
                    dataProvider.ApplyTrimBefore(stockSerie, this.TrimBeforeDate);
                }
            }
            else
            {
                var dataProvider = StockDataProviderBase.GetDataProvider(StockAnalyzerForm.MainFrame.CurrentStockSerie.DataProvider);
                if (dataProvider == null) { return; }
                dataProvider.ApplyTrimBefore(StockAnalyzerForm.MainFrame.CurrentStockSerie, this.TrimBeforeDate);
            }
            StockAnalyzerForm.MainFrame.ApplyTheme();

            this.parentDlg.Close();
        }

        private void ApplyABCClean_Click(object sender, RoutedEventArgs e)
        {
            var dataProvider = StockDataProviderBase.GetDataProvider(StockDataProvider.ABC) as ABCDataProvider;
            if (dataProvider == null) { return; }

            dataProvider.ApplyTrimAfter(this.TrimAfterDate);

            MessageBox.Show("ABC Data cleaned successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}