using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockDataProviders.AbcDataProvider;
using StockAnalyzer.StockData;
using StockAnalyzer.StockData.DataProviders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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
        public DateTime TrimDate { get; set; } = DateTime.Today.AddDays(-7);
        public float Before { get; set; } = 1f;
        public float After { get; set; } = 1f;

        private void ApplySplitButton_Click(object sender, RoutedEventArgs e)
        {
            var dataProvider = DataProviderBase.GetDataProvider(MainFrameViewModel.Instance.Instrument.Provider);
            if (dataProvider == null) { return; }

            dataProvider.AddSplit(MainFrameViewModel.Instance.Instrument, this.SplitDate, Before, After);

            StockAnalyzerForm.MainFrame.ApplyTheme();

            this.parentDlg.Close();
        }

        public bool AllProviderInstruments { get; set; }

        private void ApplyTrimButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<StockInstrument> instruments;
            if (AllProviderInstruments)
            {
                instruments = StockDictionary.Instruments.Values.Where(s => s.Provider == MainFrameViewModel.Instance.Instrument.Provider);
            }
            else
            {
                instruments = new StockInstrument[] { MainFrameViewModel.Instance.Instrument };
            }
            var dataProvider = DataProviderBase.GetDataProvider(MainFrameViewModel.Instance.Instrument.Provider);


            Func<StockDailyValue, bool> trimPredicate;
            if (sender == this.trimBeforeBtn)
                trimPredicate = v => v.DATE >= TrimDate;
            else
                trimPredicate = v => v.DATE < TrimDate;

            foreach (var instrument in instruments)
            {
                dataProvider.KeepOnlyBars(instrument, trimPredicate);
            }

            StockAnalyzerForm.MainFrame.ApplyTheme();

            this.parentDlg.Close();
        }
    }
}