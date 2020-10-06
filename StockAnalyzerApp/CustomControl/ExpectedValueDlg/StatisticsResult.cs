using StockAnalyzer;

namespace StockAnalyzerApp.CustomControl.ExpectedValueDlg
{
    public class StatisticsResult : NotifyPropertyChangedBase
    {
        public string Name { get; set; }
        public int NbEvents { get; set; }
        public int Index { get; set; }
        public float ExpectedValue { get; set; }
        public float MaxReturnValue { get; set; }
        public float MinReturnValue { get; set; }
    }
}
