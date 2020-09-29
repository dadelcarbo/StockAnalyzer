using StockAnalyzer;

namespace StockAnalyzerApp.CustomControl.ExpectedValueDlg
{
    public class TradeResult : NotifyPropertyChangedBase
    {
        public string Name { get; set; }
        public int NbTrades => NbWin + NbLoss;
        public int NbWin { get; set; }
        public int NbLoss { get; set; }
        public float MaxLoss { get; set; }
        public float MaxGain { get; set; }
        public float AvgLoss { get; set; }
        public float AvgGain { get; set; }
        public float ExpectedValue { get; set; }
    }
}
