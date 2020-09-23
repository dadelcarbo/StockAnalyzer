using StockAnalyzer.StockClasses;
using System;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.TrendDlgs
{
    public partial class BestTrendDlg : Form
    {
        public BestTrendDlg(string group, StockBarDuration barDuration)
      {
            InitializeComponent();

            (this.elementHost1.Child as BestTrendUserControl).ViewModel.BarDuration = barDuration;
            (this.elementHost1.Child as BestTrendUserControl).ViewModel.Group = group;

            StockAnalyzerForm.MainFrame.NotifyBarDurationChanged += MainFrame_NotifyBarDurationChanged;
        }

        void MainFrame_NotifyBarDurationChanged(StockBarDuration barDuration)
        {
            (this.elementHost1.Child as BestTrendUserControl).ViewModel.BarDuration = barDuration;
        }

        protected override void OnClosed(EventArgs e)
        {
            StockAnalyzerForm.MainFrame.NotifyBarDurationChanged -= MainFrame_NotifyBarDurationChanged;
            base.OnClosed(e);
        }
    }
}
