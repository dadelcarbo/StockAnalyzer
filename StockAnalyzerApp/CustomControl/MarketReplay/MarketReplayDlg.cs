using StockAnalyzer.StockClasses;
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
        }
    }
}