using StockAnalyzer.StockClasses.StockDataProviders;
using System;
using System.Windows;
using System.Windows.Controls;

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

        public DateTime Date { get; set; } = DateTime.Today.AddDays(-7);
        public int Before { get; set; } = 1;
        public int After { get; set; } = 1;

        private void ApplySplitButton_Click(object sender, RoutedEventArgs e)
        {
            var dataProvider = StockDataProviderBase.GetDataProvider(StockAnalyzerForm.MainFrame.CurrentStockSerie.DataProvider);
            if (dataProvider == null) { return; }

            dataProvider.ApplySplit(StockAnalyzerForm.MainFrame.CurrentStockSerie, this.Date, (float)Before / (float)After);

            StockAnalyzerForm.MainFrame.ApplyTheme();
        }

        private void ApplyTrimButton_Click(object sender, RoutedEventArgs e)
        {
            var dataProvider = StockDataProviderBase.GetDataProvider(StockAnalyzerForm.MainFrame.CurrentStockSerie.DataProvider);
            if (dataProvider == null) { return; }

            dataProvider.ApplyTrim(StockAnalyzerForm.MainFrame.CurrentStockSerie, this.Date);

            StockAnalyzerForm.MainFrame.ApplyTheme();
        }
    }
}