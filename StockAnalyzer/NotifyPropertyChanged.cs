using System.ComponentModel;

namespace StockAnalyzer
{
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public void OnPropertyChanged(string name)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}