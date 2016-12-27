using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;
using StockAnalyzerSettings.Properties;
using StockAnalyzer.Portofolio;

namespace StockAnalyzerApp.CustomControl.PortofolioDlgs
{
   public class StockOrderViewModel : NotifyPropertyChangedBase
   {
      public StockOrderViewModel(StockOrder stockOrder)
      {
         this.order = stockOrder;
      }

      //private StockSerie stockSerie;

      public int ID { get { return order.ID; } }

      public int Number
      {
         get { return order.Number; }
      }
      public float Value
      {
         get { return order.Value; }
      }
      public float Fee
      {
         get { return order.Fee; }
      }
      public DateTime ExecutionDate
      {
         get { return order.ExecutionDate; }
         set { order.ExecutionDate = value; }
      }
      public StockOrder.OrderType Type
      {
         get { return order.Type; }
      }
      public string StockName
      {
         get
         {
            return order.StockName;
         }
         set
         {
            order.StockName = value;
            OnPropertyChanged("StockName");
            OnPropertyChanged("IsInList");
         }
      }
      public bool IsInList
      {
         get
         {
            return StockDictionary.StockDictionarySingleton.ContainsKey(order.StockName);
         }
      }


      private StockOrder order;
      public StockOrder Order { get { return this.order; } }
   }
}
