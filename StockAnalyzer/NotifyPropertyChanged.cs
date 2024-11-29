using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StockAnalyzer
{
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }
        protected bool SetProperty(ref float field, float newValue, [CallerMemberName] string propertyName = null)
        {
            if (field != newValue)
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }
        protected bool SetProperty(ref int field, int newValue, [CallerMemberName] string propertyName = null)
        {
            if (field != newValue)
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }
        protected bool SetProperty(ref string field, string newValue, [CallerMemberName] string propertyName = null)
        {
            if (field != newValue)
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }
        public void OnPropertyChanged(string name)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}