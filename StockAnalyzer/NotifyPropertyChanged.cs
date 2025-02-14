using StockAnalyzer.StockLogging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace StockAnalyzer
{
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public Dispatcher Dispatcher { get; set; }

        public bool UseLog { get; protected set; }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (UseLog) StockLog.Write($"==> Set ${propertyName} Old:{field} New: {newValue}");
            if (!Equals(field, newValue))
            {
                field = newValue;
                this.OnPropertyChanged(propertyName);
                return true;
            }

            return false;
        }
        protected bool SetProperty(ref float field, float newValue, [CallerMemberName] string propertyName = null)
        {
            if (UseLog) StockLog.Write($"==> Set ${propertyName} Old:{field} New: {newValue}");
            if (field != newValue)
            {
                field = newValue;
                this.OnPropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        protected bool SetProperty(ref int field, int newValue, [CallerMemberName] string propertyName = null)
        {
            if (UseLog) StockLog.Write($"==> Set ${propertyName} Old:{field} New: {newValue}");
            if (field != newValue)
            {
                field = newValue;
                this.OnPropertyChanged(propertyName);
                return true;
            }

            return false;
        }
        protected bool SetProperty(ref string field, string newValue, [CallerMemberName] string propertyName = null)
        {
            if (UseLog) StockLog.Write($"==> Set ${propertyName} Old:{field} New: {newValue}");
            if (field != newValue)
            {
                field = newValue;
                this.OnPropertyChanged(propertyName);
                return true;
            }

            return false;
        }
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                if (Dispatcher == null)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    });
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}