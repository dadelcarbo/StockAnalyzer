using StockAnalyzer;

namespace StockAnalyzerApp.CustomControl.StatisticsDlg
{
    public class StatisticsResult : NotifyPropertyChangedBase
    {
        private string name;
        public string Name { get { return name; } set { if (value != name) { name = value; OnPropertyChanged("Name"); } } }

        private float totalReturn;
        public float TotalReturn { get { return totalReturn; } set { if (value != totalReturn) { totalReturn = value; OnPropertyChanged("TotalReturn"); } } }

        public float WinRatio => (float)r1Count / (float)(this.TotalTrades);

        private int s1Count;
        public int S1Count { get { return s1Count; } set { if (value != s1Count) { s1Count = value; OnPropertyChanged("S1Count"); OnPropertyChanged("WinRatio"); OnPropertyChanged("TotalTrades"); } } }

        private int r1Count;
        public int R1Count { get { return r1Count; } set { if (value != r1Count) { r1Count = value; OnPropertyChanged("R1Count"); OnPropertyChanged("WinRatio"); OnPropertyChanged("TotalTrades"); } } }

        public int TotalTrades => S1Count + R1Count;

        private int r2Count;
        public int R2Count { get { return r2Count; } set { if (value != r2Count) { r2Count = value; OnPropertyChanged("R2Count"); } } }
    }
}
