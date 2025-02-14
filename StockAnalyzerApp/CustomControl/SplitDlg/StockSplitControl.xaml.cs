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

        public DateTime Date { get; set; } = DateTime.Today.AddDays(-7);
        public float Before { get; set; } = 1f;
        public float After { get; set; } = 1f;

        private void ApplySplitButton_Click(object sender, RoutedEventArgs e)
        {
            var dataProvider = StockDataProviderBase.GetDataProvider(StockAnalyzerForm.MainFrame.CurrentStockSerie.DataProvider);
            if (dataProvider == null) { return; }

            dataProvider.AddSplit(StockAnalyzerForm.MainFrame.CurrentStockSerie, this.Date, Before, After);

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
                    dataProvider.ApplyTrim(stockSerie, this.Date);
                }
            }
            else
            {
                var dataProvider = StockDataProviderBase.GetDataProvider(StockAnalyzerForm.MainFrame.CurrentStockSerie.DataProvider);
                if (dataProvider == null) { return; }
                dataProvider.ApplyTrim(StockAnalyzerForm.MainFrame.CurrentStockSerie, this.Date);
            }
            StockAnalyzerForm.MainFrame.ApplyTheme();

            this.parentDlg.Close();
        }
    }
}