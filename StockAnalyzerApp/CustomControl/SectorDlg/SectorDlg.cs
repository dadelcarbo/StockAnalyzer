using StockAnalyzer.StockClasses;
using System;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.SectorDlg
{
    public partial class SectorDlg : Form
    {
        public SectorDlg(string group, StockBarDuration barDuration)
        {
            InitializeComponent();

            (this.elementHost1.Child as SectorUserControl).ViewModel.BarDuration = barDuration;
            StockAnalyzerForm.MainFrame.NotifyBarDurationChanged += MainFrame_NotifyBarDurationChanged;
        }

        void MainFrame_NotifyBarDurationChanged(StockBarDuration barDuration)
        {
            (this.elementHost1.Child as SectorUserControl).ViewModel.BarDuration = barDuration;
        }

        protected override void OnClosed(EventArgs e)
        {
            StockAnalyzerForm.MainFrame.NotifyBarDurationChanged -= MainFrame_NotifyBarDurationChanged;
            base.OnClosed(e);
        }
    }
}
