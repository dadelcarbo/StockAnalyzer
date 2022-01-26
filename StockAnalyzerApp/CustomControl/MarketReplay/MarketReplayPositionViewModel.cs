using StockAnalyzer;
using StockAnalyzer.StockPortfolio;

namespace StockAnalyzerApp.CustomControl.MarketReplay
{
    public class MarketReplayPositionViewModel : NotifyPropertyChangedBase
    {
        private StockPosition position;
        public MarketReplayPositionViewModel(StockPosition position)
        {
            this.position = position;
        }

        public int Qty => this.position.EntryQty;
        public float Entry => this.position.EntryValue;
        public float Value { get; private set; }
        public float Stop
        {
            get => this.position.TrailStop;
            set
            {
                if (this.position.TrailStop != value)
                {
                    this.position.TrailStop = value;
                    this.OnPropertyChanged("Stop");
                    this.OnPropertyChanged("R");
                }
            }
        }

        public string R => ((Value - Entry) / (Entry - Stop)).ToString("0.00");

        public string Gain => ((Value - Entry) / Entry).ToString("P2");

        public void SetValue(float value)
        {
            this.Value = value;
            this.OnPropertyChanged("Value");
            this.OnPropertyChanged("Gain");
            this.OnPropertyChanged("R");
        }
    }
}
