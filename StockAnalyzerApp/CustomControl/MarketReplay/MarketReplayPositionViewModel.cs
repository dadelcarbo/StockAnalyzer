using StockAnalyzer;

namespace StockAnalyzerApp.CustomControl.MarketReplay
{
    public class MarketReplayPositionViewModel : NotifyPropertyChangedBase
    {
        private string stop;
        private string target1;

        public int Qty { get; set; }
        public float Entry { get; set; }
        public float Value { get; private set; }
        public string Stop
        {
            get => stop;
            set
            {
                if (stop != value)
                {
                    stop = value;
                    this.OnPropertyChanged("Stop");
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
        public string Gain => ((Value - Entry) / Entry).ToString("P2");

        public void SetValue(float value)
        {
            this.Value = value;
            this.OnPropertyChanged("Value");
            this.OnPropertyChanged("Gain");
        }
    }
}
