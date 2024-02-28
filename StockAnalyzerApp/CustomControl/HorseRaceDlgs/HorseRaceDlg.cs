using StockAnalyzer.StockClasses;
using System;
using System.Windows.Forms;

namespace StockAnalyzerApp.CustomControl.HorseRaceDlgs
{
    public partial class HorseRaceDlg : Form
    {
        public HorseRaceDlg(string group, BarDuration barDuration)
        {
            InitializeComponent();

            (this.elementHost1.Child as HorseRaceControl).ViewModel.BarDuration = barDuration;
            (this.elementHost1.Child as HorseRaceControl).ViewModel.Group = group;
            (this.elementHost1.Child as HorseRaceControl).SelectedStockChanged += StockAnalyzerForm.MainFrame.OnSelectedStockChanged;

            StockAnalyzerForm.MainFrame.NotifyBarDurationChanged += MainFrame_NotifyBarDurationChanged;
        }

        void MainFrame_NotifyBarDurationChanged(BarDuration barDuration)
        {
            (this.elementHost1.Child as HorseRaceControl).ViewModel.BarDuration = barDuration;
        }

        protected override void OnClosed(EventArgs e)
        {
            (this.elementHost1.Child as HorseRaceControl).SelectedStockChanged -= StockAnalyzerForm.MainFrame.OnSelectedStockChanged;
            StockAnalyzerForm.MainFrame.NotifyBarDurationChanged -= MainFrame_NotifyBarDurationChanged;
            base.OnClosed(e);
        }
    }
}
