using StockAnalyzer;

namespace StockAnalyzerApp.CustomControl.ConditionalStatisticsDlg
{
    public class CondStatisticsResult : NotifyPropertyChangedBase
    {
        private string name;
        public string Name { get { return name; } set { if (value != name) { name = value; OnPropertyChanged("Name"); } } }

        private int totalCount;
        public int TotalCount { get { return totalCount; } set { if (value != totalCount) { totalCount = value; OnPropertyChanged("TotalCount"); } } }

        private int event1Count;
        public int Event1Count { get { return event1Count; } set { if (value != event1Count) { event1Count = value; OnPropertyChanged("Event1Count"); } } }

        private int event2Count;
        public int Event2Count { get { return event2Count; } set { if (value != event2Count) { event2Count = value; OnPropertyChanged("Event2Count"); } } }

        private int event1n2Count;
        public int Event1n2Count { get { return event1n2Count; } set { if (value != event1n2Count) { event1n2Count = value; OnPropertyChanged("Event1n2Count"); } } }

        public float PE1 => event1Count / (float)totalCount;
        public float PE2 => event2Count / (float)totalCount;

        public float PE1n2 => PE1*PE2;

        public float PE1k2 => event1n2Count / (float)event2Count;
    }
}
