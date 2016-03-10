using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.CustomControl.HorseRaceDlgs
{
   public class HorseRaceViewModel : INotifyPropertyChanged
   {
      private int maxIndex = 0;

      public int MaxIndex
      {
         get { return maxIndex; }
         set
         {
            if (maxIndex != value)
            {
               maxIndex = value;
               OnPropertyChanged("MaxIndex");
            }
         }
      }

      private int minIndex = 0;

      public int MinIndex
      {
         get { return minIndex; }
         set
         {
            if (minIndex != value)
            {
               minIndex = value;
               OnPropertyChanged("MinIndex");
            }
         }
      }

      private int index = 0;

      public int Index
      {
         get { return index; }
         set
         {
            if (index != value)
            {
               index = value;
               this.CalculatePositions();
               OnPropertyChanged("Index");
            }
         }
      }

      static List<int> ranges = new List<int>(){-1,-5,-20,-100,-200}; 
      public List<int> Ranges { get { return ranges; } }

      public HorseRaceViewModel()
      {
         minIndex = -200;
         maxIndex = 0;
         index = 0;

         this.StockPositions = new ObservableCollection<StockPosition>();
      }
      
      private void CalculatePositions()
      {
         foreach (StockPosition stockPosition in this.StockPositions)
         {
            float startClose = stockPosition.StockSerie.Values.ElementAt(Math.Max(0, stockPosition.StockSerie.Count + minIndex - 1)).CLOSE;
            float endClose = stockPosition.StockSerie.Values.ElementAt(Math.Max(0, stockPosition.StockSerie.Count + index - 1)).CLOSE;
            stockPosition.Position = 100f * (endClose - startClose) / startClose;
         }
      }

      private void InitPositions()
      {
         this.StockPositions.Clear();
         List<StockPosition> positions = new List<StockPosition>();
         foreach (StockSerie stockSerie in this.StockList)
         {
            if (stockSerie.Initialise())
            {
               positions.Add(new StockPosition() {Name = stockSerie.StockName, Position = stockSerie.Values.Last().VARIATION * 100f, StockSerie = stockSerie});
            }
         }
         this.StockPositions = new ObservableCollection<StockPosition>(positions);
         this.CalculatePositions();
      }

      public event PropertyChangedEventHandler PropertyChanged;

      public void OnPropertyChanged(string name)
      {
         if (PropertyChanged != null)
         {
            this.PropertyChanged(this, new PropertyChangedEventArgs(name));
         }
      }

      private List<StockSerie> stockList;

      public List<StockSerie> StockList
      {
         get { return stockList; }
         set
         {
            if (stockList != value)
            {
               stockList = value;
               this.InitPositions();
               OnPropertyChanged("StockList");
            }
         }
      }

      private ObservableCollection<StockPosition> stockPositions;

      public ObservableCollection<StockPosition> StockPositions
      {
         get { return stockPositions; }
         set
         {
            if (stockPositions != value)
            {
               stockPositions = value;
               this.CalculatePositions();
               OnPropertyChanged("StockPositions");
            }
         }
      }
   }

   public class StockPosition : INotifyPropertyChanged
   {
      private string name;
      public string Name
      {
         get { return name; }
         set
         {
            if (name != value)
            {
               name = value;
               OnPropertyChanged("Name");
            }
         }
      }
      
      private float position = 0;
      public float Position
      {
         get { return position; }
         set
         {
            if (position != value)
            {
               position = value;
               OnPropertyChanged("Position");
            }
         }
      }

      public StockSerie StockSerie { get; set; }

      public event PropertyChangedEventHandler PropertyChanged;

      public void OnPropertyChanged(string name)
      {
         if (PropertyChanged != null)
         {
            this.PropertyChanged(this, new PropertyChangedEventArgs(name));
         }
      }
   }
}