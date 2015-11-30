using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;
using StockAnalyzerSettings.Properties;

namespace StockAnalyzerApp.CustomControl.PortofolioDlgs
{
   public class StockOrderViewModel : INotifyPropertyChanged
   {
      public StockOrderViewModel(float amount)
      {
         this.amount = amount;
      }

      //private StockSerie stockSerie;

      public int Qty
      {
         get { return (int)Math.Floor(Amount / Buy); }
      }

      public float PortofolioValue { get { return Settings.Default.PortofolioValue; } set { Settings.Default.PortofolioValue = value; NotifyAllChanged(); } }

      private float buy;
      public float Buy
      {
         get { return buy; }
         set
         {
            buy = value;
            NotifyAllChanged();
         }
      }

      private float amount;
      public float Amount { get { return amount; } set { amount = value; NotifyAllChanged(); } }

      public float Fee
      {
         get
         {
            if (Amount < 1000) return 2.5f;
            else return 5.0f;
         }
      }

      public void NotifyAllChanged()
      {
         Type type = this.GetType();
         PropertyInfo[] properties = type.GetProperties();

         if (this.PropertyChanged != null)
         {
            foreach (PropertyInfo property in properties)
            {
               this.OnPropertyChanged(property.Name);
            }
         }
      }

      // Create the OnPropertyChanged method to raise the event
      protected void OnPropertyChanged(string name)
      {
         if (PropertyChanged != null)
         {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;
   }

}
