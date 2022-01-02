using StockAnalyzer.StockClasses;
using StockAnalyzerApp.CustomControl.GraphControls;
using System;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.MarketReplay
{
    public partial class MarketReplayDlg : Form
    {
        public MarketReplayDlg(StockSerie.Groups selectedGroup, StockBarDuration barDuration)
        {
            InitializeComponent();

            marketReplayControl1.DataContext = marketReplayControl1.ViewModel = new MarketReplayViewModel(selectedGroup, barDuration);
            marketReplayControl1.ViewModel.IndicatorNames = StockAnalyzerForm.MainFrame.GetIndicatorsFromCurrentTheme();

            this.FormClosing += MarketReplayDlg_FormClosing;
        }

        private void MarketReplayDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            marketReplayControl1.ViewModel.Closing();
        }

        internal void OnStopValueChanged(float stop)
        {
            marketReplayControl1.ViewModel.Stop = stop;
        }
    }
}