using StockAnalyzer;

namespace StockAnalyzerApp.CustomControl.MarketReplay
{
    public class MarketReplayTradeViewModel : NotifyPropertyChangedBase
    {
        private float exit;
        private string target1;

        public float Entry { get; set; }
        public float Exit
        {
            get => exit;
            set
            {
                if (exit != value)
                {
                    exit = value;
                    this.OnPropertyChanged("Exit");
                }
            }
        }
        public string Target1
        {
            get => target1;
            set
            {
                if (target1 != value)
                {
                    target1 = value;
                    this.OnPropertyChanged("Target1");
                }
            }
        }

        public string TotalGain => ((Exit - Entry) / Entry).ToString("P2");

    }
}
