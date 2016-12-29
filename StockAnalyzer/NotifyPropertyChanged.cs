using System.ComponentModel;

namespace StockAnalyzer
{
   public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
   {
      public void OnPropertyChanged(string name)
      {
         if (PropertyChanged != null)
         {
            this.PropertyChanged(this, new PropertyChangedEventArgs(name));
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;
   }
}